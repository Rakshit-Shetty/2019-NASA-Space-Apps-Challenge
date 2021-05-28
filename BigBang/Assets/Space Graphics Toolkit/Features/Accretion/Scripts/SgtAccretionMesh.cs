using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAccretionMesh))]
	public class SgtAccretionMesh_Editor : SgtEditor<SgtAccretionMesh>
	{
		protected override void OnInspector()
		{
			var updateMesh  = false;
			var updateApply = false;

			BeginError(Any(t => t.Segments < 1));
				DrawDefault("Segments", ref updateMesh, "The amount of segments the final disc will be comprised of.");
			EndError();
			BeginError(Any(t => t.SegmentDetail < 1));
				DrawDefault("SegmentDetail", ref updateMesh, "The amount of triangle edges along the inner and outer edges of each segment.");
			EndError();
			BeginError(Any(t => t.SegmentTiling < 1));
				DrawDefault("SegmentTiling", ref updateMesh, "The amount of times the main texture is tiled around the ring segment.");
			EndError();

			Separator();

			BeginError(Any(t => t.RadiusMin == t.RadiusMax));
				DrawDefault("RadiusMin", ref updateMesh, "The radius of the inner edge in local space.");
				DrawDefault("RadiusMax", ref updateMesh, "The radius of the outer edge in local space.");
			EndError();
			BeginError(Any(t => t.RadiusDetail < 1));
				DrawDefault("RadiusDetail", ref updateMesh, "The amount of edge loops around the generated disc. If you have a very large ring then you can end up with very skinny triangles, so increasing this can give them a better shape.");
			EndError();

			Separator();

			DrawDefault("BoundsShift", ref updateMesh, "The amount the mesh bounds should get pushed out by in local space. This should be used with 8+ Segments.");

			if (updateMesh  == true) DirtyEach(t => t.UpdateMesh ());
			if (updateApply == true) DirtyEach(t => t.ApplyMesh());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtAccretion.Mesh field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtAccretion))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtAccretionMesh")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Accretion Mesh")]
	public class SgtAccretionMesh : MonoBehaviour
	{
		/// <summary>The amount of segments the final disc will be comprised of.</summary>
		public int Segments = 8;

		/// <summary>The amount of triangle edges along the inner and outer edges of each segment.</summary>
		public int SegmentDetail = 50;

		/// <summary>The amount of times the main texture is tiled around the ring segment.</summary>
		public int SegmentTiling = 1;

		/// <summary>The radius of the inner edge in local space.</summary>
		public float RadiusMin = 1.0f;

		/// <summary>The radius of the outer edge in local space.</summary>
		public float RadiusMax = 2.0f;

		/// <summary>The amount of edge loops around the generated disc. If you have a very large ring then you can end up with very skinny triangles, so increasing this can give them a better shape.</summary>
		public int RadiusDetail = 1;

		/// <summary>The amount the mesh bounds should get pushed out by in local space. This should be used with 8+ Segments.</summary>
		public float BoundsShift;

		[System.NonSerialized]
		private Mesh generatedMesh;

		[System.NonSerialized]
		private SgtAccretion cachedAccretion;

		[System.NonSerialized]
		private bool cachedAccretionSet;

		public Mesh GeneratedMesh
		{
			get
			{
				return generatedMesh;
			}
		}

#if UNITY_EDITOR
		[ContextMenu("Export Mesh")]
		public void ExportMesh()
		{
			if (generatedMesh != null)
			{
				SgtHelper.ExportAssetDialog(generatedMesh, "Accretion Mesh");
			}
		}
#endif

		[ContextMenu("Update Mesh")]
		public void UpdateMesh()
		{
			if (Segments > 0 && SegmentDetail > 0 && RadiusDetail > 0)
			{
				if (generatedMesh == null)
				{
					generatedMesh = SgtHelper.CreateTempMesh("Accretion Mesh (Generated)");

					ApplyMesh();
				}

				var slices     = SegmentDetail + 1;
				var rings      = RadiusDetail + 1;
				var total      = slices * rings * 2;
				var positions  = new Vector3[total];
				var coords1    = new Vector2[total];
				var coords2    = new Vector2[total];
				var colors     = new Color[total];
				var indices    = new int[SegmentDetail * RadiusDetail * 6];
				var yawStep    = (Mathf.PI * 2.0f) / Segments / SegmentDetail;
				var sliceStep  = 1.0f / SegmentDetail;
				var ringStep   = 1.0f / RadiusDetail;

				for (var slice = 0; slice < slices; slice++)
				{
					var a = yawStep * slice;
					var x = Mathf.Sin(a);
					var z = Mathf.Cos(a);

					for (var ring = 0; ring < rings; ring++)
					{
						var v       = rings * slice + ring;
						var slice01 = sliceStep * slice;
						var ring01  = ringStep * ring;
						var radius  = Mathf.Lerp(RadiusMin, RadiusMax, ring01);

						positions[v] = new Vector3(x * radius, 0.0f, z * radius);
						colors[v] = new Color(1.0f, 1.0f, 1.0f, 0.0f);
						coords1[v] = new Vector2(ring01, slice01);
						coords2[v] = new Vector2(radius, slice01 * radius * SegmentTiling);
					}
				}

				for (var slice = 0; slice < SegmentDetail; slice++)
				{
					for (var ring = 0; ring < RadiusDetail; ring++)
					{
						var i  = (slice * RadiusDetail + ring) * 6;
						var v0 = slice * rings + ring;
						var v1 = v0 + rings;

						indices[i + 0] = v0 + 0;
						indices[i + 1] = v0 + 1;
						indices[i + 2] = v1 + 0;
						indices[i + 3] = v1 + 1;
						indices[i + 4] = v1 + 0;
						indices[i + 5] = v0 + 1;
					}
				}

				generatedMesh.Clear(false);
				generatedMesh.vertices  = positions;
				generatedMesh.colors    = colors;
				generatedMesh.uv        = coords1;
				generatedMesh.uv2       = coords2;
				generatedMesh.triangles = indices;
				generatedMesh.RecalculateNormals();
				generatedMesh.RecalculateBounds();

				var bounds = generatedMesh.bounds;

				generatedMesh.bounds = SgtHelper.NewBoundsCenter(bounds, bounds.center + bounds.center.normalized * BoundsShift);
			}
		}

		[ContextMenu("Apply Mesh")]
		public void ApplyMesh()
		{
			if (cachedAccretionSet == false)
			{
				cachedAccretion    = GetComponent<SgtAccretion>();
				cachedAccretionSet = true;
			}

			if (cachedAccretion.Mesh != generatedMesh)
			{
				cachedAccretion.Mesh = generatedMesh;

				cachedAccretion.UpdateMesh();
			}
		}

		protected virtual void OnEnable()
		{
			UpdateMesh();
			ApplyMesh();
		}

		protected virtual void OnDestroy()
		{
			if (generatedMesh != null)
			{
				generatedMesh.Clear(false);

				SgtObjectPool<Mesh>.Add(generatedMesh);
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			SgtHelper.DrawCircle(Vector3.zero, Vector3.up, RadiusMin);
			SgtHelper.DrawCircle(Vector3.zero, Vector3.up, RadiusMax);
		}
#endif
	}
}