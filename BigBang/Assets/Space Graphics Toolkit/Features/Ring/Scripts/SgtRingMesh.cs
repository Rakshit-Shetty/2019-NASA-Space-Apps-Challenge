using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtRingMesh))]
	public class SgtRingMesh_Editor : SgtEditor<SgtRingMesh>
	{
		protected override void OnInspector()
		{
			var updateMesh  = false;
			var updateApply = false;

			BeginError(Any(t => t.Segments < 1));
				DrawDefault("segments", ref updateMesh, "The amount of segments the final ring will be comprised of.");
			EndError();
			BeginError(Any(t => t.SegmentDetail < 1));
				DrawDefault("segmentDetail", ref updateMesh, "The amount of triangle edges along the inner and outer edges of each segment.");
			EndError();
			BeginError(Any(t => t.RadiusMin == t.RadiusMax));
				DrawDefault("radiusMin", ref updateMesh, "The radius of the inner edge in local space.");
				DrawDefault("radiusMax", ref updateMesh, "The radius of the outer edge in local space.");
			EndError();
			BeginError(Any(t => t.RadiusDetail < 1));
				DrawDefault("radiusDetail", ref updateMesh, "The amount of edge loops around the generated ring. If you have a very large ring then you can end up with very skinny triangles, so increasing this can give them a better shape.");
			EndError();
			DrawDefault("boundsShift", ref updateMesh, "The amount the mesh bounds should get pushed out by in local space. This should be used with 8+ Segments.");
			DrawDefault("shadow", ref updateMesh, "If you want these values to control the shadow RadiusMin/Max, then set this here.");

			if (updateMesh  == true) Each(t => t.UpdateMesh());
			if (updateApply == true) Each(t => t.ApplyMesh ());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtRing.Mesh field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtRing))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtRingMesh")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Ring Mesh")]
	public class SgtRingMesh : MonoBehaviour
	{
		/// <summary>The amount of segments the final ring will be comprised of.</summary>
		public int Segments { set { segments = value; } get { return segments; } } [FormerlySerializedAs("Segments")] [SerializeField] private int segments = 8;
		public void SetSegments(int value) { segments = value; UpdateMesh(); }

		/// <summary>The amount of triangle edges along the inner and outer edges of each segment.</summary>
		public int SegmentDetail { set { segmentDetail = value; } get { return segmentDetail; } } [FormerlySerializedAs("SegmentDetail")] [SerializeField] private int segmentDetail = 50;
		public void SetSegmentDetail(int value) { segmentDetail = value; UpdateMesh(); }

		/// <summary>The radius of the inner edge in local space.</summary>
		public float RadiusMin { set { radiusMin = value; } get { return radiusMin; } } [FormerlySerializedAs("RadiusMin")] [SerializeField] private float radiusMin = 1.0f;
		public void SetRadiusMin(float value) { radiusMin = value; UpdateMesh(); }

		/// <summary>The radius of the outer edge in local space.</summary>
		public float RadiusMax { set { radiusMax = value; } get { return radiusMax; } } [FormerlySerializedAs("RadiusMax")] [SerializeField] private float radiusMax = 2.0f;
		public void SetRadiusMax(float value) { radiusMax = value; UpdateMesh(); }

		/// <summary>The amount of edge loops around the generated ring. If you have a very large ring then you can end up with very skinny triangles, so increasing this can give them a better shape.</summary>
		public int RadiusDetail { set { radiusDetail = value; } get { return radiusDetail; } } [FormerlySerializedAs("RadiusDetail")] [SerializeField] private int radiusDetail = 1;
		public void SetRadiusDetail(int value) { radiusDetail = value; UpdateMesh(); }

		/// <summary>The amount the mesh bounds should get pushed out by in local space. This should be used with 8+ Segments.</summary>
		public float BoundsShift { set { boundsShift = value; } get { return boundsShift; } } [FormerlySerializedAs("BoundsShift")] [SerializeField] private float boundsShift;
		public void SetBoundsShift(float value) { boundsShift = value; UpdateMesh(); }

		/// <summary>If you want these values to control the shadow RadiusMin/Max, then set this here.</summary>
		public SgtShadowRing Shadow { set { shadow = value; } get { return shadow; } } [FormerlySerializedAs("Shadow")] [SerializeField] private SgtShadowRing shadow;

		[System.NonSerialized]
		private Mesh generatedMesh;

		[System.NonSerialized]
		private SgtRing cachedRing;

		[System.NonSerialized]
		private bool cachedRingSet;

		public Mesh GeneratedMesh
		{
			get
			{
				return generatedMesh;
			}
		}

#if UNITY_EDITOR
		[ContextMenu("Export Mesh")]
		public void ExportOuterTexture()
		{
			if (generatedMesh != null)
			{
				SgtHelper.ExportAssetDialog(generatedMesh, "Ring Mesh");
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
					generatedMesh = SgtHelper.CreateTempMesh("Ring Mesh (Generated)");

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
						coords2[v] = new Vector2(radius, slice01 * radius);
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

			if (Shadow != null)
			{
				Shadow.RadiusMin = RadiusMin;
				Shadow.RadiusMax = RadiusMax;
			}
		}

		[ContextMenu("Apply Mesh")]
		public void ApplyMesh()
		{
			if (cachedRingSet == false)
			{
				cachedRing    = GetComponent<SgtRing>();
				cachedRingSet = true;
			}

			if (cachedRing.Mesh != generatedMesh)
			{
				cachedRing.Mesh = generatedMesh;

				cachedRing.UpdateMesh();
			}
		}

		[ContextMenu("Remove Mesh")]
		public void RemoveMesh()
		{
			if (cachedRingSet == false)
			{
				cachedRing    = GetComponent<SgtRing>();
				cachedRingSet = true;
			}

			if (cachedRing.Mesh == generatedMesh)
			{
				cachedRing.Mesh = null;

				cachedRing.UpdateMesh();
			}
		}

		protected virtual void OnEnable()
		{
			UpdateMesh();
			ApplyMesh();
		}

		protected virtual void OnDisable()
		{
			RemoveMesh();
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