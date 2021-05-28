using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainFace))]
	public class SgtTerrainFace_Editor : SgtEditor<SgtTerrainFace>
	{
		protected override void OnInspector()
		{
			BeginDisabled();
				DrawDefault("Terrain");
				DrawDefault("Side");
				DrawDefault("Depth");
				DrawDefault("Bounds");

				Separator();

				DrawDefault("NeighbourL");
				DrawDefault("NeighbourR");
				DrawDefault("NeighbourB");
				DrawDefault("NeighbourT");

				Separator();

				DrawDefault("CornerBL");
				DrawDefault("CornerBR");
				DrawDefault("CornerTL");
				DrawDefault("CornerTR");

				Separator();

				DrawDefault("Split");
				DrawDefault("ChildBL");
				DrawDefault("ChildBR");
				DrawDefault("ChildTL");
				DrawDefault("ChildTR");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtTerrainFace : MonoBehaviour
	{
		public SgtTerrain     Terrain;
		public CubemapFace    Side;
		public int            Depth;
		public SgtBoundsD     Bounds;

		public bool           Split;
		public SgtTerrainFace ChildBL;
		public SgtTerrainFace ChildBR;
		public SgtTerrainFace ChildTL;
		public SgtTerrainFace ChildTR;

		public SgtTerrainFace GetChild(int index)
		{
			if (index == 0) return ChildBL;
			if (index == 1) return ChildBR;
			if (index == 2) return ChildTL;
			return ChildTR;
		}

		public SgtVector3D CornerBL;
		public SgtVector3D CornerBR;
		public SgtVector3D CornerTL;
		public SgtVector3D CornerTR;

		public SgtTerrainNeighbour NeighbourL;
		public SgtTerrainNeighbour NeighbourR;
		public SgtTerrainNeighbour NeighbourB;
		public SgtTerrainNeighbour NeighbourT;

		public void SetNeighbour(int index, SgtTerrainFace face)
		{
			if (index == 0) NeighbourL.Face = face;
			else if (index == 1) NeighbourR.Face = face;
			else if (index == 2) NeighbourB.Face = face;
			else NeighbourT.Face = face;
		}
		public Mesh mesh;

		[System.NonSerialized]
		private MeshFilter cachedMeshFilter;

		[System.NonSerialized]
		private MeshRenderer cachedMeshRenderer;

		[System.NonSerialized]
		private MeshCollider cachedMeshCollider;

		// Used to assign renderer.sharedMaterials
		private static Material[] materials0 = new Material[0];
		private static Material[] materials1 = new Material[1];
		private static Material[] materials2 = new Material[2];

		[ContextMenu("Update Renderers")]
		public void UpdateRenderers()
		{
			UpdateRenderer();

			if (Split == true)
			{
				ChildBL.UpdateRenderers();
				ChildBR.UpdateRenderers();
				ChildTL.UpdateRenderers();
				ChildTR.UpdateRenderers();
			}
		}

		[ContextMenu("Update Renderer")]
		public void UpdateRenderer()
		{
			if (cachedMeshRenderer == null) cachedMeshRenderer = GetComponent<MeshRenderer>();

			var material = Terrain.Material;

			if (SgtHelper.Enabled(Terrain.SharedMaterial) == true)
			{
				materials2[0] = material;
				materials2[1] = Terrain.SharedMaterial.Material;

				cachedMeshRenderer.sharedMaterials = materials2;
			}
			else if (material != null)
			{
				materials1[0] = material;

				cachedMeshRenderer.sharedMaterials = materials1;
			}
			else
			{
				cachedMeshRenderer.sharedMaterials = materials0;
			}
		}

		[ContextMenu("Update Colliders")]
		public void UpdateColliders()
		{
			UpdateCollider();

			if (Split == true)
			{
				ChildBL.UpdateColliders();
				ChildBR.UpdateColliders();
				ChildTL.UpdateColliders();
				ChildTR.UpdateColliders();
			}
		}

		[ContextMenu("Update Collider")]
		public void UpdateCollider()
		{
			var shouldExist = false;

			if (Depth < Terrain.MaxColliderDepth)
			{
				if (Split == false || (Terrain.MaxColliderDepth - 1) == Depth)
				{
					shouldExist = true;
				}
			}

			if (cachedMeshCollider == null) cachedMeshCollider = GetComponent<MeshCollider>();

			if (shouldExist == true)
			{
				if (cachedMeshCollider == null) cachedMeshCollider = gameObject.AddComponent<MeshCollider>();

				cachedMeshCollider.enabled = true;

				if (cachedMeshCollider.sharedMesh == mesh)
				{
					cachedMeshCollider.sharedMesh = null;
				}

				cachedMeshCollider.sharedMesh = mesh;
			}
			else if (cachedMeshCollider != null)
			{
				cachedMeshCollider.enabled = false;
			}
		}

		public bool WriteVertex(int x, int y)
		{
			if (Depth == 0)
			{
				return true;
			}

			if (y % 2 == 1)
			{
				return true;
			}

			return x % 2 == 1;
		}

		private SgtVector3D GetPosition(SgtVector3D origin, SgtVector3D sideDelta, SgtVector3D edgeDelta)
		{
			var eps      = 0.00000001;
			var position = origin + sideDelta;

			// Wrap position around cube if it goes off the edge
			if (position.x < -1.0 - eps || position.x > 1.0 + eps || position.y < -1.0 - eps || position.y > 1.0 + eps || position.z < -1.0 - eps || position.z > 1.0 + eps)
			{
				position = origin + edgeDelta;
			}

			position = position.normalized * Terrain.GetLocalHeight(position);

			return position;
		}

		public void BuildVertices()
		{
			var mesh = GetMesh();

			mesh.Clear(false);

			Bounds.Clear();

			var step = Terrain.NormalStep;

			if (Terrain.Normals == SgtTerrain.NormalType.QuadStep)
			{
				var verts = (2 << Terrain.Subdivisions) + 1;
				var scale = 1 << Depth;

				step = 1.0f / (verts * scale);
			}

			SgtTerrainCompute.BeginFace(Terrain.ForceCPU);
			SgtTerrainCompute.SetFaceCorners(Side, CornerBL, CornerBR, CornerTL);
			SgtTerrainCompute.SetMain(Terrain.Radius, step, Terrain.NormalStrength, Terrain.HeightMap, Terrain.HeightScale);
			SgtTerrainCompute.SetDetail(Terrain.MaskMap, Terrain.DetailTiling, Terrain.DetailMapA, Terrain.DetailScaleA, Terrain.DetailMapA, Terrain.DetailScaleB);
			SgtTerrainCompute.DispatchFace(mesh, Terrain.Subdivisions, ref Bounds, Terrain.Point != null);

			Terrain.PositionChanged(Terrain.transform, this);

			//mesh.RecalculateBounds();
		}

		private static List<int> buildingIndices = new List<int>();

		[ContextMenu("Build Indices")]
		public void BuildIndices()
		{
			var level  = SgtTerrainLevel.Levels[Terrain.Subdivisions];
			var indexL = level.GetIndex(Depth - NeighbourL.Face.Depth);
			var indexR = level.GetIndex(Depth - NeighbourR.Face.Depth);
			var indexB = level.GetIndex(Depth - NeighbourB.Face.Depth);
			var indexT = level.GetIndex(Depth - NeighbourT.Face.Depth);

			buildingIndices.Clear();

			buildingIndices.AddRange(level.Indices);
			buildingIndices.AddRange(level.IndicesL[indexL]);
			buildingIndices.AddRange(level.IndicesR[indexR]);
			buildingIndices.AddRange(level.IndicesB[indexB]);
			buildingIndices.AddRange(level.IndicesT[indexT]);

			//link to parent

			mesh.SetTriangles(buildingIndices, 0);

			mesh.RecalculateBounds();

			UpdateCollider();
		}

		public static SgtTerrainFace Create(string name, int layer, Transform parent)
		{
			return SgtComponentPool<SgtTerrainFace>.Pop(parent, name, layer);
		}

		public static SgtTerrainFace Pool(SgtTerrainFace face)
		{
			if (face != null)
			{
				face.Terrain = null;

				if (face.Split == true)
				{
					face.Split   = false;
					face.ChildBL = Pool(face.ChildBL);
					face.ChildBR = Pool(face.ChildBR);
					face.ChildTL = Pool(face.ChildTL);
					face.ChildTR = Pool(face.ChildTR);
				}

				SgtComponentPool<SgtTerrainFace>.Add(face);
			}

			return null;
		}

		public static SgtTerrainFace MarkForDestruction(SgtTerrainFace face)
		{
			if (face != null)
			{
				face.Terrain = null;

				face.gameObject.SetActive(true);
			}

			return null;
		}

		public void UpdateSplit()
		{
			var distance = GetDistance();

			// Split?
			if (distance < Terrain.LocalDistances[Depth])
			{
				if (Split == false && CanSplit == true)
				{
					SplitNow();
					UpdateCollider();
				}
			}
			// Unsplit?
			else
			{
				if (Split == true && CanUnsplit == true)
				{
					UnsplitNow();
					UpdateCollider();
				}
			}
		}

		public void Unsplit()
		{
			if (Split == true)
			{
				UnsplitNow();
			}
		}

		public void UpdateSplits()
		{
			UpdateSplit();

			if (Split == true)
			{
				ChildBL.UpdateSplits();
				ChildBR.UpdateSplits();
				ChildTL.UpdateSplits();
				ChildTR.UpdateSplits();
			}
		}

		private bool CanSplit
		{
			get
			{
				return NeighbourL.Face.Depth >= Depth && NeighbourR.Face.Depth >= Depth && NeighbourB.Face.Depth >= Depth && NeighbourT.Face.Depth >= Depth;
			}
		}

		private bool CanUnsplit
		{
			get
			{
				if (NeighbourL.Face.Split == true) if (ChildBL.NeighbourL.Face.Split == true || ChildTL.NeighbourL.Face.Split == true) return false;
				if (NeighbourR.Face.Split == true) if (ChildBR.NeighbourR.Face.Split == true || ChildTR.NeighbourR.Face.Split == true) return false;
				if (NeighbourB.Face.Split == true) if (ChildBL.NeighbourB.Face.Split == true || ChildBR.NeighbourB.Face.Split == true) return false;
				if (NeighbourT.Face.Split == true) if (ChildTL.NeighbourT.Face.Split == true || ChildTR.NeighbourT.Face.Split == true) return false;

				return true;
			}
		}

		private double GetDistance()
		{
			var bestDistance = double.PositiveInfinity;

			for (var i = Terrain.LocalPoints.Count - 1; i >= 0; i--)
			{
				var dist = (Bounds.Center - Terrain.LocalPoints[i]).sqrMagnitude;

				if (dist < bestDistance)
				{
					bestDistance = dist;
				}
			}

			return bestDistance;
		}

		public void UpdateInvalid()
		{
			BuildVertices();
			BuildIndices();
			UpdateRenderer();
			UpdateCollider();

			//mesh.RecalculateNormals();
			//mesh.RecalculateTangents();

			if (Split == true)
			{
				UpdateInvalidChildren();
			}

			if (Terrain.OnSpawnFace != null)
			{
				Terrain.OnSpawnFace.Invoke(this);
			}
		}

		public override int GetHashCode()
		{
			var mid = CornerBL + CornerTR;

			return mid.GetHashCode();
		}

		private void SplitNow()
		{
			var cornerLL = (CornerBL + CornerTL) * 0.5f;
			var cornerRR = (CornerBR + CornerTR) * 0.5f;
			var cornerTT = (CornerTL + CornerTR) * 0.5f;
			var cornerBB = (CornerBL + CornerBR) * 0.5f;
			var cornerMM = (CornerBL + CornerTR) * 0.5f;

			Split   = true;
			ChildBL = Create("ChildBL", CornerBL, cornerBB, cornerLL, cornerMM);
			ChildBR = Create("ChildBR", cornerBB, CornerBR, cornerMM, cornerRR);
			ChildTL = Create("ChildTL", cornerLL, cornerMM, CornerTL, cornerTT);
			ChildTR = Create("ChildTR", cornerMM, cornerRR, cornerTT, CornerTR);

			// Link children
			ChildBL.NeighbourR.Set(ChildBR, 1, 1, 3, 0, 0, 2, 0);
			ChildBL.NeighbourT.Set(ChildTL, 3, 2, 3, 2, 0, 1, 2);
			ChildBR.NeighbourL.Set(ChildBL, 0, 0, 2, 1, 1, 3, 1);
			ChildBR.NeighbourT.Set(ChildTR, 3, 2, 3, 2, 0, 1, 2);
			ChildTL.NeighbourR.Set(ChildTR, 1, 1, 3, 0, 0, 2, 0);
			ChildTL.NeighbourB.Set(ChildBL, 2, 0, 1, 3, 2, 3, 3);
			ChildTR.NeighbourL.Set(ChildTL, 0, 0, 2, 1, 1, 3, 1);
			ChildTR.NeighbourB.Set(ChildBR, 2, 0, 1, 3, 2, 3, 3);

			// Link to neighbours
			LinkNeighbours(ChildBL, ChildTL, ref ChildBL.NeighbourL, ref ChildTL.NeighbourL, ref NeighbourL);
			LinkNeighbours(ChildBR, ChildTR, ref ChildBR.NeighbourR, ref ChildTR.NeighbourR, ref NeighbourR);
			LinkNeighbours(ChildBL, ChildBR, ref ChildBL.NeighbourB, ref ChildBR.NeighbourB, ref NeighbourB);
			LinkNeighbours(ChildTL, ChildTR, ref ChildTL.NeighbourT, ref ChildTR.NeighbourT, ref NeighbourT);

			UpdateInvalidChildren();

			UpdateNeighbourData(NeighbourL.Face, ref NeighbourL);
			UpdateNeighbourData(NeighbourR.Face, ref NeighbourR);
			UpdateNeighbourData(NeighbourB.Face, ref NeighbourB);
			UpdateNeighbourData(NeighbourT.Face, ref NeighbourT);

			cachedMeshRenderer.enabled = false;
		}

		private void LinkNeighbours(SgtTerrainFace childA, SgtTerrainFace childB, ref SgtTerrainNeighbour childNeighbourA, ref SgtTerrainNeighbour childNeighbourB, ref SgtTerrainNeighbour neighbour)
		{
			if (neighbour.Face.Split == true)
			{
				var neighbourChildC = neighbour.Face.GetChild(neighbour.C);
				var neighbourChildD = neighbour.Face.GetChild(neighbour.D);

				neighbourChildC.SetNeighbour(neighbour.O, childA);
				neighbourChildD.SetNeighbour(neighbour.O, childB);

				childNeighbourA.Set(neighbourChildC, neighbour.I, neighbour.A, neighbour.B, neighbour.O, neighbour.C, neighbour.D, neighbour.Z);
				childNeighbourB.Set(neighbourChildD, neighbour.I, neighbour.A, neighbour.B, neighbour.O, neighbour.C, neighbour.D, neighbour.Z);
			}
			else
			{
				childNeighbourA.Set(neighbour.Face, neighbour.I, neighbour.A, neighbour.B, neighbour.O, neighbour.C, neighbour.D, neighbour.Z);
				childNeighbourB.Set(neighbour.Face, neighbour.I, neighbour.A, neighbour.B, neighbour.O, neighbour.C, neighbour.D, neighbour.Z);
			}
		}

		private void UpdateNeighbourData(SgtTerrainFace face, ref SgtTerrainNeighbour neighbour)
		{
			if (face.Split == true)
			{
				var childC = face.GetChild(neighbour.C);
				var childD = face.GetChild(neighbour.D);

				childC.BuildIndices();
				childD.BuildIndices();

				UpdateNeighbourData(childC, ref neighbour);
				UpdateNeighbourData(childD, ref neighbour);
			}
		}

		private void UpdateInvalidChildren()
		{
			ChildBL.UpdateInvalid();
			ChildBR.UpdateInvalid();
			ChildTL.UpdateInvalid();
			ChildTR.UpdateInvalid();
		}

		private void UnsplitNow()
		{
			if (ChildBL.Split == true) ChildBL.UnsplitNow();
			if (ChildBR.Split == true) ChildBR.UnsplitNow();
			if (ChildTL.Split == true) ChildTL.UnsplitNow();
			if (ChildTR.Split == true) ChildTR.UnsplitNow();

			if (Terrain.OnDespawnFace != null)
			{
				Terrain.OnDespawnFace.Invoke(ChildBL);
				Terrain.OnDespawnFace.Invoke(ChildBR);
				Terrain.OnDespawnFace.Invoke(ChildTL);
				Terrain.OnDespawnFace.Invoke(ChildTR);
			}

			Split   = false;
			ChildBL = Pool(ChildBL);
			ChildBR = Pool(ChildBR);
			ChildTL = Pool(ChildTL);
			ChildTR = Pool(ChildTR);

			BuildIndices();

			UnlinkNeighbours(ref NeighbourL);
			UnlinkNeighbours(ref NeighbourR);
			UnlinkNeighbours(ref NeighbourB);
			UnlinkNeighbours(ref NeighbourT);

			cachedMeshRenderer.enabled = true;
		}

		private void UnlinkNeighbours(ref SgtTerrainNeighbour childNeighbour)
		{
			if (childNeighbour.Face.Split == true)
			{
				var childC = childNeighbour.Face.GetChild(childNeighbour.C);
				var childD = childNeighbour.Face.GetChild(childNeighbour.D);

				childC.SetNeighbour(childNeighbour.O, this);
				childD.SetNeighbour(childNeighbour.O, this);

				childC.BuildIndices();
				childD.BuildIndices();
			}
		}

		private SgtTerrainFace Create(string childName, SgtVector3D cornerBL, SgtVector3D cornerBR, SgtVector3D cornerTL, SgtVector3D cornerTR)
		{
			var face = Create(childName, gameObject.layer, transform);

			face.Terrain  = Terrain;
			face.Side     = Side;
			face.Depth    = Depth + 1;
			face.CornerBL = cornerBL;
			face.CornerBR = cornerBR;
			face.CornerTL = cornerTL;
			face.CornerTR = cornerTR;

			return face;
		}

		private Mesh GetMesh()
		{
			if (mesh == null)
			{
				mesh = SgtObjectPool<Mesh>.Pop() ?? new Mesh();
#if UNITY_EDITOR
				mesh.hideFlags = HideFlags.DontSave;
#endif
				mesh.name = "Terrain";
			}

			if (cachedMeshFilter == null) cachedMeshFilter = GetComponent<MeshFilter>();

			cachedMeshFilter.sharedMesh = mesh;

			return mesh;
		}
	}
}