using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtPlanetMeshConverter))]
	public class SgtPlanetMeshConverter_Editor : SgtEditor<SgtPlanetMeshConverter>
	{
		protected override void OnInspector()
		{
			var rebuildMesh = false;

			BeginError(Any(t => t.OriginalMesh == null));
				DrawDefault("OriginalMesh", ref rebuildMesh, "The original mesh we want to displace.");
			EndError();
			BeginError(Any(t => t.CapScale == 0.0f));
				DrawDefault("CapScale", ref rebuildMesh, "The original mesh we want to displace.");
			EndError();

			if (rebuildMesh == true) DirtyEach(t => t.RebuildMesh());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component converts a normal spherical mesh into one suitable for the SGT Planet shader.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtPlanetMeshConverter")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Planet Mesh Converter")]
	public class SgtPlanetMeshConverter : MonoBehaviour
	{
		/// <summary>The original mesh we want to displace.</summary>
		public Mesh OriginalMesh;

		/// <summary>The UV scale of the cap.</summary>
		public float CapScale = 0.5f;

		[System.NonSerialized]
		private Mesh generatedMesh;

		[System.NonSerialized]
		private MeshFilter meshFilter;

		[System.NonSerialized]
		private bool meshFilterSet;

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

			if (OriginalMesh != null)
			{
				// Duplicate original
				generatedMesh = Instantiate(OriginalMesh);
#if UNITY_EDITOR
				generatedMesh.hideFlags = HideFlags.DontSave;
#endif
				generatedMesh.name = OriginalMesh.name + " (Planet)";

				// Displace vertices
				var uv        = OriginalMesh.uv;
				var positions = OriginalMesh.vertices;
				var tangents  = OriginalMesh.tangents;
				var coords    = new List<Vector2>();

				for (var i = 0; i < uv.Length; i++)
				{
					var position = positions[i];
					var tangent  = tangents[i];
					var coord    = default(Vector2);

					if (position.y > 0.0f)
					{
						coord.x = -position.x;
						coord.y = position.z;
					}
					else
					{
						coord.x = position.x;
						coord.y = position.z;
					}

					/*
					position.y = 0.0f;

					if (position.sqrMagnitude < 0.0001f)
					{
						var pos = position.normalized;
						tangent = new Vector4(pos.x, pos.y, pos.z, -1.0f);
					}
					*/

					coords.Add(coord * CapScale);
					tangents[i] = tangent;
				}

				generatedMesh.SetUVs(1, coords);
				generatedMesh.tangents = tangents;
			}

			if (meshFilterSet == false)
			{
				meshFilter    = GetComponent<MeshFilter>();
				meshFilterSet = true;
			}

			meshFilter.sharedMesh = generatedMesh;
		}

		protected virtual void OnEnable()
		{
			if (meshFilterSet == false)
			{
				meshFilter    = GetComponent<MeshFilter>();
				meshFilterSet = true;
			}

			if (OriginalMesh == null)
			{
				OriginalMesh = meshFilter.sharedMesh;
			}

			if (generatedMesh == null)
			{
				RebuildMesh();
			}

			meshFilter.sharedMesh = generatedMesh;
		}

		protected virtual void OnDisable()
		{
			meshFilter.sharedMesh = OriginalMesh;
		}
	}
}