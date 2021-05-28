using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtMeshDisplacer))]
	public class SgtMeshDisplacer_Editor : SgtEditor<SgtMeshDisplacer>
	{
		protected override void OnInspector()
		{
			var rebuildMesh = false;

			BeginError(Any(t => t.OriginalMesh == null));
				DrawDefault("OriginalMesh", ref rebuildMesh, "The original mesh we want to displace.");
			EndError();
			BeginError(Any(t => t.Heightmap == null));
				DrawDefault("Heightmap", ref rebuildMesh, "The height map texture used to displace the mesh (Height must be stored in alpha channel).");
			EndError();
			DrawDefault("Encoding", ref rebuildMesh, "The way the height data is stored in the texture.");

			Separator();

			BeginError(Any(t => t.InnerRadius == t.OuterRadius));
				DrawDefault("InnerRadius", ref rebuildMesh, "The mesh radius represented by a 0 alpha value.");
				DrawDefault("OuterRadius", ref rebuildMesh, "The mesh radius represented by a 255 alpha value.");
			EndError();
			DrawDefault("HeightmapCutoff", ref rebuildMesh, "If you want to bake in the water height, then set the cutoff level here.");
			DrawDefault("ZeroCutoff", ref rebuildMesh, "Make all samples below the cutoff have a height of 0?");

			if (rebuildMesh == true) DirtyEach(t => t.RebuildMesh());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component converts a normal spherical mesh into one displaced by a heightmap.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtMeshDisplacer")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Mesh Displacer")]
	public class SgtMeshDisplacer : MonoBehaviour
	{
		public enum EncodingType
		{
			Alpha,
			RedGreen
		}

		/// <summary>The original mesh we want to displace.</summary>
		public Mesh OriginalMesh;

		/// <summary>The height map texture used to displace the mesh (Height must be stored in alpha channel).</summary>
		public Texture2D Heightmap;

		/// <summary>The way the height data is stored in the texture.</summary>
		public EncodingType Encoding = EncodingType.Alpha;

		/// <summary>The mesh radius represented by a 0 alpha value.</summary>
		public float InnerRadius = 1.0f;

		/// <summary>The mesh radius represented by a 255 alpha value.</summary>
		public float OuterRadius = 1.1f;

		/// <summary>If you want to bake in the water height, then set the cutoff level here.</summary>
		[Range(0.0f, 1.0f)]
		public float HeightmapCutoff;

		/// <summary>Make all samples below the cutoff have a height of 0?</summary>
		public bool ZeroCutoff = true;

		[System.NonSerialized]
		private Mesh generatedMesh;

#if UNITY_EDITOR
		[ContextMenu("Export Mesh")]
		public void ExportOuterTexture()
		{
			if (generatedMesh != null)
			{
				SgtHelper.ExportAssetDialog(generatedMesh, generatedMesh.name);
			}
		}
#endif

		// Call this if you've made any changes from code and need the mesh to get rebuilt
		[ContextMenu("Rebuild Mesh")]
		public void RebuildMesh()
		{
			generatedMesh = SgtHelper.Destroy(generatedMesh);

			if (OriginalMesh != null && Heightmap != null)
			{
	#if UNITY_EDITOR
				SgtHelper.MakeTextureReadable(Heightmap);
	#endif
				// Duplicate original
				generatedMesh = Instantiate(OriginalMesh);
	#if UNITY_EDITOR
				generatedMesh.hideFlags = HideFlags.DontSave;
	#endif
				generatedMesh.name = OriginalMesh.name + " (Displaced)";

				// Displace vertices
				var positions = OriginalMesh.vertices;

				for (var i = 0; i < positions.Length; i++)
				{
					var direction = positions[i].normalized;

					positions[i] = direction * GetSurfaceHeightLocal(direction);
				}

				generatedMesh.vertices = positions;

				generatedMesh.RecalculateBounds();
				generatedMesh.RecalculateNormals();
				generatedMesh.RecalculateTangents();
			}

			ApplyMesh();
		}

		public void ApplyMesh()
		{
			var finalMesh = generatedMesh != null ? generatedMesh : OriginalMesh;

			// MeshFilter
			var meshFilter = GetComponent<MeshFilter>();

			if (meshFilter != null)
			{
				meshFilter.sharedMesh = finalMesh;
			}

			// MeshCollider
			var meshCollider = GetComponent<MeshCollider>();

			if (meshCollider != null)
			{
				meshCollider.sharedMesh = finalMesh;
			}
		}

		protected virtual void OnEnable()
		{
			SgtHelper.OnCalculateDistance += CalculateDistance;

			if (OriginalMesh == null)
			{
				var meshFilter = GetComponent<MeshFilter>();

				if (meshFilter != null)
				{
					OriginalMesh = meshFilter.sharedMesh;
				}
			}

			if (OriginalMesh == null)
			{
				var meshCollider = GetComponent<MeshCollider>();

				if (meshCollider != null)
				{
					OriginalMesh = meshCollider.sharedMesh;
				}
			}

			if (generatedMesh == null)
			{
				RebuildMesh();
			}
			else
			{
				ApplyMesh();
			}
		}

		protected virtual void OnDisable()
		{
			SgtHelper.OnCalculateDistance -= CalculateDistance;

			var meshFilter = GetComponent<MeshFilter>();

			if (meshFilter != null)
			{
				meshFilter.sharedMesh = OriginalMesh;
			}

			var meshCollider = GetComponent<MeshCollider>();

			if (meshCollider != null)
			{
				meshCollider.sharedMesh = OriginalMesh;
			}
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(generatedMesh);
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireSphere(Vector3.zero, InnerRadius);
			Gizmos.DrawWireSphere(Vector3.zero, OuterRadius);
		}
#endif

		// This will return the local terrain height at the given local position
		public float GetSurfaceHeightLocal(Vector3 localPosition)
		{
			var uv       = SgtHelper.CartesianToPolarUV(localPosition);
			var color    = SampleBilinear(uv);
			var height01 = default(float);

			switch (Encoding)
			{
				case EncodingType.Alpha:
				{
					height01 = color.a;
				}
				break;

				case EncodingType.RedGreen:
				{
					height01 = (color.r * 255.0f + color.g) / 256.0f;
				}
				break;
			}

			if (ZeroCutoff == true)
			{
				height01 = Mathf.InverseLerp(HeightmapCutoff, 1.0f, height01);
			}
			else
			{
				height01 = Mathf.Max(height01, HeightmapCutoff);
			}

			return Mathf.Lerp(InnerRadius, OuterRadius, height01);
		}

		private Color SampleBilinear(Vector2 uv)
		{
			return Heightmap.GetPixelBilinear(uv.x, uv.y);
		}

		private void CalculateDistance(Vector3 worldPosition, ref float bestDistance)
		{
			if (Heightmap != null)
			{
				var localPosition        = transform.InverseTransformPoint(worldPosition);
				var surfaceLocalPosition = localPosition.normalized * GetSurfaceHeightLocal(localPosition);
				var surfaceWorldPosition = transform.TransformPoint(surfaceLocalPosition);
				var distance             = Vector3.Distance(surfaceWorldPosition, worldPosition);

				if (distance < bestDistance)
				{
					bestDistance = distance;
				}
			}
		}
	}
}