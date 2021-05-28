using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrain))]
	public class SgtTerrain_Editor : SgtEditor<SgtTerrain>
	{
		protected override void OnInspector()
		{
			var rebuild         = false;
			var updateSplits    = false;
			var updateRenderer  = false;
			var updateColliders = false;

			DrawDefault("Material", ref updateRenderer, "The base material for the terrain. If you're using one material per face or something more complex then use the SgtTerrainMaterial or SgtTerraimCubeMaterials component instead.");
			DrawDefault("SharedMaterial", ref updateRenderer, "If you want to apply a shared material (e.g. atmosphere) to this terrain, then specify it here.");

			Separator();

			BeginError(Any(t => t.HeightMap == null));
				DrawDefault("HeightMap", ref rebuild, "This allows you to set the texture used to read height data from. The height data is read from the alpha channel.");
			EndError();
			DrawDefault("HeightScale", ref rebuild, "This allows you to set the strength of the height map samples, where maximum alpha (255) increases the terrain height by this amount.");

			BeginError(Any(t => t.MaskMap == null));
				DrawDefault("MaskMap", ref rebuild, "This allows you to set the detail texture mask, where the red channel is used for DetailMapA, and the green channel is used for DetailMapB.");
			EndError();

			BeginError(Any(t => t.DetailTiling <= 0.0f));
				DrawDefault("DetailTiling", ref rebuild, "The allows you to set how many times the detail textures are tiled along the planet surface.");
			EndError();

			BeginError(Any(t => t.DetailMapA == null));
				DrawDefault("DetailMapA", ref rebuild, "This allows you to set the detail texture, where the alpha channel is used as the height source.");
			EndError();
			DrawDefault("DetailScaleA", ref rebuild, "This allows you to set the detail texture strength, where maximum alpha (255) increases the terrain height by this amount.");

			BeginError(Any(t => t.DetailMapB == null));
				DrawDefault("DetailMapB", ref rebuild, "This allows you to set the detail texture, where the alpha channel is used as the height source.");
			EndError();
			DrawDefault("DetailScaleB", ref rebuild, "This allows you to set the detail texture strength, where maximum alpha (255) increases the terrain height by this amount.");
			
			Separator();

			BeginError(Any(t => t.TargetMainCamera == false && (t.Targets == null || t.Targets.Count == 0)));
				DrawDefault("TargetMainCamera", ref updateSplits, "If you want the LOD to update based on the distance to the main camera, then enable this.");
				DrawDefault("Targets", ref updateSplits, "If you want the LOD to update based on the distance to other transforms, then set them here.");
			EndError();
			BeginError(Any(t => t.Radius <= 0.0));
				DrawDefault("Radius", ref rebuild, "The base radius of the terrain. This can be deformed by another component like SgtTerrainSimplex.");
			EndError();

			Separator();

			DrawDefault("Normals", ref rebuild, "The strategy used to generate the normal maps. FixedStep uses the NormalStep setting as the sample distance. QuadStep uses the current quad size as the sample distance.");
			DrawDefault("NormalStep", ref rebuild, "This allows you to set the sample distance when calculating the surface normal of a given point. A higher value leads to smoother results.");
			DrawDefault("NormalStrength", ref rebuild, "This allows you to adjust the overall effect of surface normals.");
			DrawDefault("Subdivisions", ref rebuild, "The detail of each LOD level.");
			DrawDefault("Budget", "The maximum amount of time that can be spent updating the terrain each frame in seconds.");
			Each(t => t.UnregisterPoint());
				DrawDefault("Point", ref rebuild, "If you have a large planet then your terrain faces must use the floating origin system, set the planet's point here to use it.");
			Each(t => t.RegisterPoint());
			DrawDefault("ForceCPU", ref rebuild, "Terrains will be generated using Compute Shaders if possible, but this can force them to use CPU.");
			DrawDefault("CenterBounds", ref rebuild, "Should the bounds match the size of the terrain?");
			BeginError(Any(t => t.MaxColliderDepth < 0 || (t.Distances != null && t.MaxColliderDepth > t.Distances.Count)));
				DrawDefault("MaxColliderDepth", ref updateColliders, "The maximum LOD depth that colliders will be generated for (0 = none).");
			EndError();
			BeginError(Any(t => DistancesValid(t.Distances) == false));
				DrawDefault("Distances", ref updateSplits, "The LOD distances in local space, these should be sorted from high to low.");
			EndError();

			Separator();

			if (Button("Add Distance") == true)
			{
				Each(t => AddDistance(ref t.Distances));
			}

			Separator();

			if (Button("Rebuild") == true)
			{
				Each(t => t.Rebuild());
			}

			if (Button("Unsplit") == true)
			{
				Each(t => t.Unsplit());
			}

			if (Button("Update Splits") == true)
			{
				Each(t => t.UpdateSplits());
			}

			if (rebuild         == true) DirtyEach(t => t.Rebuild        ());
			if (updateSplits    == true) DirtyEach(t => t.UpdateSplits   ());
			if (updateRenderer  == true) DirtyEach(t => t.UpdateRenderers());
			if (updateColliders == true) DirtyEach(t => t.UpdateColliders());
		}

		private static void AddDistance(ref List<double> distances)
		{
			if (distances == null)
			{
				distances = new List<double>();
			}

			var lastDistance = 2.0;

			if (distances.Count > 0)
			{
				var distance = distances[distances.Count - 1];

				if (distance > 0.0)
				{
					lastDistance = distance;
				}
			}

			distances.Add(lastDistance * 0.5);
		}

		private static bool DistancesValid(List<double> distances)
		{
			if (distances == null)
			{
				return false;
			}

			var lastDistance = double.PositiveInfinity;

			for (var i = 0; i < distances.Count; i++)
			{
				var distance = distances[i];

				if (distance <= 0.0 || distance >= lastDistance)
				{
					return false;
				}

				lastDistance = distance;
			}

			return true;
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to make a planet or star whose surface is procedurally generated, and has LOD based on camera (or another Transform) distance.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtTerrain")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain")]
	public partial class SgtTerrain : SgtLinkedBehaviour<SgtTerrain>
	{
		public delegate void CalculateFaceDelegate(SgtTerrainFace face);

		public enum NormalType
		{
			FixedStep,
			QuadStep
		}

		/// <summary>The base material for the terrain. If you're using one material per face or something more complex then use the SgtTerrainMaterial or SgtTerraimCubeMaterials component instead.</summary>
		public Material Material;

		/// <summary>If you want to apply a shared material (e.g. atmosphere) to this terrain, then specify it here.</summary>
		public SgtSharedMaterial SharedMaterial;

		/// <summary>This allows you to set the texture used to read height data from. The height data is read from the alpha channel.</summary>
		public Texture2D HeightMap;

		/// <summary>This allows you to set the strength of the height map samples, where maximum alpha (255) increases the terrain height by this amount.</summary>
		public float HeightScale = 0.1f;

		/// <summary>This allows you to set the detail texture mask, where the red channel is used for DetailMapA, and the green channel is used for DetailMapB.</summary>
		public Texture MaskMap;

		/// <summary>The allows you to set how many times the detail textures are tiled along the planet surface.</summary>
		public float DetailTiling = 50.0f;

		/// <summary>This allows you to set the detail texture, where the alpha channel is used as the height source.</summary>
		public Texture DetailMapA;

		/// <summary>This allows you to set the detail texture strength, where maximum alpha (255) increases the terrain height by this amount.</summary>
		public float DetailScaleA = 0.01f;

		/// <summary>This allows you to set the detail texture, where the alpha channel is used as the height source.</summary>
		public Texture DetailMapB;

		/// <summary>This allows you to set the detail texture strength, where maximum alpha (255) increases the terrain height by this amount.</summary>
		public float DetailScaleB = 0.01f;

		/// <summary>If you want the LOD to update based on the distance to the main camera, then enable this.</summary>
		public bool TargetMainCamera = true;

		/// <summary>If you want the LOD to update based on the distance to other transforms, then set them here.</summary>
		public List<Transform> Targets;

		/// <summary>The base radius of the terrain. This can be deformed by another component like SgtTerrainSimplex.</summary>
		public double Radius = 1.0f;

		/// <summary>The strategy used to generate the normal maps. FixedStep uses the NormalStep setting as the sample distance. QuadStep uses the current quad size as the sample distance.</summary>
		public NormalType Normals = NormalType.QuadStep;

		/// <summary>This allows you to set the sample distance when calculating the surface normal of a given point. A higher value leads to smoother results.</summary>
		[Range(0.0000001f, 0.1f)]
		public float NormalStep = 0.001f;

		/// <summary>This allows you to adjust the overall effect of surface normals.</summary>
		[Range(0.01f, 2.0f)]
		public float NormalStrength = 1.0f;

		/// <summary>The detail of each LOD level.</summary>
		[Range(0, 3)]
		public int Subdivisions = 3;

		/// <summary>The maximum amount of time that can be spent updating the terrain each frame in seconds.</summary>
		[Range(0.001f, 0.02f)]
		public float Budget = 0.005f;

		/// <summary>If you have a large planet then your terrain faces must use the floating origin system, set the planet's point here to use it.</summary>
		public SgtFloatingPoint Point;

		/// <summary>Terrains will be generated using Compute Shaders if possible, but this can force them to use CPU.</summary>
		public bool ForceCPU;

		/// <summary>Should the bounds match the size of the terrain?</summary>
		public bool CenterBounds;

		/// <summary>The maximum LOD depth that colliders will be generated for (0 = none).</summary>
		public int MaxColliderDepth;

		/// <summary>The LOD distances in local space, these should be sorted from high to low.</summary>
		public List<double> Distances;

		public Material ExpectedSharedMaterial;

		[System.NonSerialized]
		private bool invalid = true;

		[System.NonSerialized]
		public bool DelayRebuild;

		[System.NonSerialized]
		public bool DelayUpdateRenderers;

		[System.NonSerialized]
		public bool DelayUpdateColliders;

		[System.NonSerialized]
		public List<SgtVector3D> LocalPoints = new List<SgtVector3D>();

		[System.NonSerialized]
		public List<double> LocalDistances = new List<double>();

		// Each face handles a cube face
		public SgtTerrainFace NegativeX;
		public SgtTerrainFace NegativeY;
		public SgtTerrainFace NegativeZ;
		public SgtTerrainFace PositiveX;
		public SgtTerrainFace PositiveY;
		public SgtTerrainFace PositiveZ;

		// Called when a face is spawned
		public System.Action<SgtTerrainFace> OnSpawnFace;

		public System.Action<SgtTerrainFace> OnDespawnFace;

		public void RegisterPoint()
		{
			if (Point != null)
			{
				Point.OnPositionChanged += PositionChanged;
			}
		}

		public void UnregisterPoint()
		{
			if (Point != null)
			{
				Point.OnPositionChanged -= PositionChanged;
			}
		}

		private void PositionChanged()
		{
			if (NegativeX != null) PositionChanged(transform, NegativeX);
			if (NegativeY != null) PositionChanged(transform, NegativeY);
			if (NegativeZ != null) PositionChanged(transform, NegativeZ);
			if (PositiveX != null) PositionChanged(transform, PositiveX);
			if (PositiveY != null) PositionChanged(transform, PositiveY);
			if (PositiveZ != null) PositionChanged(transform, PositiveZ);
		}

		public void PositionChanged(Transform parent, SgtTerrainFace face)
		{
			if (Point != null)
			{
				var center = (Vector3)face.Bounds.Center;

				face.transform.parent     = SgtFloatingRoot.GetRoot();
				face.transform.position   = transform.position + transform.rotation * center * transform.lossyScale.x;
				face.transform.rotation   = transform.rotation;
				face.transform.localScale = transform.lossyScale;
			}
			else
			{
				face.transform.parent        = parent;
				face.transform.localPosition = Vector3.zero;
				face.transform.localRotation = Quaternion.identity;
				face.transform.localScale    = Vector3.one;
			}

			if (face.Split == true)
			{
				PositionChanged(face.transform, face.ChildBL);
				PositionChanged(face.transform, face.ChildBR);
				PositionChanged(face.transform, face.ChildTL);
				PositionChanged(face.transform, face.ChildTR);
			}
		}

		private void CameraSnapped(SgtFloatingCamera floatingCamera, Vector3 delta)
		{
			PositionChanged();
		}

		private void CalculateLocalValues()
		{
			LocalPoints.Clear();

			if (TargetMainCamera == true)
			{
				var camera = Camera.main;

				if (camera != null)
				{
					var point = new SgtVector3D(transform.InverseTransformPoint(camera.transform.position));

					LocalPoints.Add(point);
				}
			}

			if (Targets != null)
			{
				for (var i = Targets.Count - 1; i >= 0; i--)
				{
					var target = Targets[i];

					if (target != null)
					{
						var point = new SgtVector3D(transform.InverseTransformPoint(target.position));

						LocalPoints.Add(point);
					}
				}
			}

			LocalDistances.Clear();

			for (var i = 0; i < 32; i++)
			{
				if (Distances != null && i < Distances.Count)
				{
					var distance = Distances[i];

					LocalDistances.Add(distance * distance);
				}
				else
				{
					LocalDistances.Add(double.NegativeInfinity);
				}
			}
		}

		// Gets the surface height under the input point in local space
		public double GetLocalHeight(SgtVector3D localPoint)
		{
			return SampleLocalOutput(localPoint).Vertex.magnitude;
		}

		public float GetWorldHeight(Vector3 worldPoint)
		{
			var surfacePoint = GetWorldPoint(worldPoint);

			return Vector3.Distance(transform.position, surfacePoint);
		}

		public SgtVector3D GetLocalPoint(SgtVector3D localCube)
		{
			return localCube.normalized * GetLocalHeight(localCube);
		}

		public Vector3 GetWorldPoint(Vector3 position)
		{
			var localPosition = transform.InverseTransformPoint(position);
			var localPoint    = new SgtVector3D(localPosition);

			localPoint = GetLocalPoint(localPoint);

			return transform.TransformPoint((Vector3)localPoint);
		}

		public Vector3 GetLocalNormal(SgtVector3D point)
		{
			return SampleLocalOutput(point).Normal.normalized;
		}

		public SgtVector3D GetLocalNormal(SgtVector3D point, SgtVector3D right, SgtVector3D up)
		{
			var localPoint  = GetLocalPoint(point);
			var localPointR = GetLocalPoint(localPoint + right);
			var localPointU = GetLocalPoint(localPoint - up);
			var vectorR     = localPointR - localPoint;
			var vectorU     = localPointU - localPoint;

			return SgtVector3D.Cross(vectorR, vectorU).normalized;
		}

		public Vector3 GetWorldNormal(SgtVector3D point)
		{
			return transform.TransformDirection(GetLocalNormal(point));
		}

		public Vector3 GetWorldNormal(Vector3 point, Vector3 right, Vector3 up)
		{
			var worldPoint  = GetWorldPoint(point);
			var worldPointR = GetWorldPoint(point + right);
			var worldPointU = GetWorldPoint(point + up);
			var vectorR     = worldPointR - worldPoint;
			var vectorU     = worldPointU - worldPoint;

			return Vector3.Cross(vectorR, vectorU).normalized;
		}

		public SgtTerrainCompute.Output SampleLocalOutput(SgtVector3D localPoint)
		{
			SgtTerrainCompute.BeginPoint(ForceCPU);
			SgtTerrainCompute.SetMain(Radius, NormalStep, NormalStrength, HeightMap, HeightScale);
			SgtTerrainCompute.SetDetail(MaskMap, DetailTiling, DetailMapA, DetailScaleA, DetailMapA, DetailScaleB);

			return SgtTerrainCompute.DispatchPoint((Vector3)localPoint);
		}

		public Vector3 GetWorldNormal(Vector3 point)
		{
			return Vector3.Normalize(point - transform.position);
		}

		[ContextMenu("Rebuild")]
		public void Rebuild()
		{
			DelayRebuild = false;
		
			ValidateFaces();

			var sw = System.Diagnostics.Stopwatch.StartNew();

			NegativeX.UpdateInvalid();
			NegativeY.UpdateInvalid();
			NegativeZ.UpdateInvalid();
			PositiveX.UpdateInvalid();
			PositiveY.UpdateInvalid();
			PositiveZ.UpdateInvalid();

			sw.Stop();

			Debug.Log("Rebuild Took " + sw.ElapsedMilliseconds);
		}

		[ContextMenu("Unsplit")]
		public void Unsplit()
		{
			if (NegativeX != null) NegativeX.Unsplit();
			if (NegativeY != null) NegativeY.Unsplit();
			if (NegativeZ != null) NegativeZ.Unsplit();
			if (PositiveX != null) PositiveX.Unsplit();
			if (PositiveY != null) PositiveY.Unsplit();
			if (PositiveZ != null) PositiveZ.Unsplit();
		}

		[ContextMenu("Update Splits")]
		public void UpdateSplits()
		{
			ValidateFaces();
			CalculateLocalValues();

			var sw = System.Diagnostics.Stopwatch.StartNew();

			if (NegativeX != null) NegativeX.UpdateSplits();
			if (NegativeY != null) NegativeY.UpdateSplits();
			if (NegativeZ != null) NegativeZ.UpdateSplits();
			if (PositiveX != null) PositiveX.UpdateSplits();
			if (PositiveY != null) PositiveY.UpdateSplits();
			if (PositiveZ != null) PositiveZ.UpdateSplits();

			sw.Stop();

			Debug.Log("UpdateSplits Took " + sw.ElapsedMilliseconds);
		}

		[ContextMenu("Update Renderers")]
		public void UpdateRenderers()
		{
			DelayUpdateRenderers = false;

			if (SharedMaterial != null)
			{
				ExpectedSharedMaterial = SharedMaterial.Material;
			}

			if (NegativeX != null) NegativeX.UpdateRenderers();
			if (NegativeY != null) NegativeY.UpdateRenderers();
			if (NegativeZ != null) NegativeZ.UpdateRenderers();
			if (PositiveX != null) PositiveX.UpdateRenderers();
			if (PositiveY != null) PositiveY.UpdateRenderers();
			if (PositiveZ != null) PositiveZ.UpdateRenderers();
		}

		[ContextMenu("Update Colliders")]
		public void UpdateColliders()
		{
			DelayUpdateColliders = false;

			if (NegativeX != null) NegativeX.UpdateColliders();
			if (NegativeY != null) NegativeY.UpdateColliders();
			if (NegativeZ != null) NegativeZ.UpdateColliders();
			if (PositiveX != null) PositiveX.UpdateColliders();
			if (PositiveY != null) PositiveY.UpdateColliders();
			if (PositiveZ != null) PositiveZ.UpdateColliders();
		}

		public void RunFaces(CalculateFaceDelegate method)
		{
			RunFaces(NegativeX, method);
			RunFaces(NegativeY, method);
			RunFaces(NegativeZ, method);
			RunFaces(PositiveX, method);
			RunFaces(PositiveY, method);
			RunFaces(PositiveZ, method);
		}

		public void RunFaces(SgtTerrainFace face, CalculateFaceDelegate method)
		{
			if (face != null)
			{
				method(face);

				if (face.Split == true)
				{
					RunFaces(face.ChildBL, method);
					RunFaces(face.ChildBR, method);
					RunFaces(face.ChildTL, method);
					RunFaces(face.ChildTR, method);
				}
			}
		}

		public void ValidateFaces()
		{
			if (NegativeX == null) NegativeX = CreateFace(CubemapFace.NegativeX, new Vector3(-1.0f, -1.0f,  1.0f), new Vector3(-1.0f, -1.0f, -1.0f), new Vector3(-1.0f,  1.0f,  1.0f), new Vector3(-1.0f,  1.0f, -1.0f));
			if (NegativeY == null) NegativeY = CreateFace(CubemapFace.NegativeY, new Vector3( 1.0f, -1.0f, -1.0f), new Vector3(-1.0f, -1.0f, -1.0f), new Vector3( 1.0f, -1.0f,  1.0f), new Vector3(-1.0f, -1.0f,  1.0f));
			if (NegativeZ == null) NegativeZ = CreateFace(CubemapFace.NegativeZ, new Vector3(-1.0f, -1.0f, -1.0f), new Vector3( 1.0f, -1.0f, -1.0f), new Vector3(-1.0f,  1.0f, -1.0f), new Vector3( 1.0f,  1.0f, -1.0f));
			if (PositiveX == null) PositiveX = CreateFace(CubemapFace.PositiveX, new Vector3( 1.0f, -1.0f, -1.0f), new Vector3( 1.0f, -1.0f,  1.0f), new Vector3( 1.0f,  1.0f, -1.0f), new Vector3( 1.0f,  1.0f,  1.0f));
			if (PositiveY == null) PositiveY = CreateFace(CubemapFace.PositiveY, new Vector3( 1.0f,  1.0f,  1.0f), new Vector3(-1.0f,  1.0f,  1.0f), new Vector3( 1.0f,  1.0f, -1.0f), new Vector3(-1.0f,  1.0f, -1.0f));
			if (PositiveZ == null) PositiveZ = CreateFace(CubemapFace.PositiveZ, new Vector3( 1.0f, -1.0f,  1.0f), new Vector3(-1.0f, -1.0f,  1.0f), new Vector3( 1.0f,  1.0f,  1.0f), new Vector3(-1.0f,  1.0f,  1.0f));

			NegativeX.NeighbourL.Set(PositiveZ, 0, 0, 2, 1, 1, 3, 1);
			NegativeX.NeighbourR.Set(NegativeZ, 1, 1, 3, 0, 0, 2, 0);
			NegativeX.NeighbourB.Set(NegativeY, 2, 0, 1, 1, 3, 1, 5);
			NegativeX.NeighbourT.Set(PositiveY, 3, 2, 3, 1, 1, 3, 1);

			NegativeY.NeighbourL.Set(PositiveX, 0, 0, 2, 2, 0, 1, 2);
			NegativeY.NeighbourR.Set(NegativeX, 1, 1, 3, 2, 1, 0, 6);
			NegativeY.NeighbourB.Set(NegativeZ, 2, 0, 1, 2, 1, 0, 6);
			NegativeY.NeighbourT.Set(PositiveZ, 3, 2, 3, 2, 0, 1, 2);

			NegativeZ.NeighbourL.Set(NegativeX, 0, 0, 2, 1, 1, 3, 1);
			NegativeZ.NeighbourR.Set(PositiveX, 1, 1, 3, 0, 0, 2, 0);
			NegativeZ.NeighbourB.Set(NegativeY, 2, 0, 1, 2, 1, 0, 6);
			NegativeZ.NeighbourT.Set(PositiveY, 3, 2, 3, 3, 3, 2, 7);

			PositiveX.NeighbourL.Set(NegativeZ, 0, 0, 2, 1, 1, 3, 1);
			PositiveX.NeighbourR.Set(PositiveZ, 1, 1, 3, 0, 0, 2, 0);
			PositiveX.NeighbourB.Set(NegativeY, 2, 0, 1, 0, 0, 2, 0);
			PositiveX.NeighbourT.Set(PositiveY, 3, 2, 3, 0, 2, 0, 4);

			PositiveY.NeighbourL.Set(PositiveX, 0, 0, 2, 3, 3, 2, 7);
			PositiveY.NeighbourR.Set(NegativeX, 1, 1, 3, 3, 2, 3, 3);
			PositiveY.NeighbourB.Set(PositiveZ, 2, 0, 1, 3, 2, 3, 3);
			PositiveY.NeighbourT.Set(NegativeZ, 3, 2, 3, 3, 3, 2, 7);

			PositiveZ.NeighbourL.Set(PositiveX, 0, 0, 2, 1, 1, 3, 1);
			PositiveZ.NeighbourR.Set(NegativeX, 1, 1, 3, 0, 0, 2, 0);
			PositiveZ.NeighbourB.Set(NegativeY, 2, 0, 1, 3, 2, 3, 3);
			PositiveZ.NeighbourT.Set(PositiveY, 3, 2, 3, 2, 0, 1, 2);
		}

		public static SgtTerrain Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtTerrain Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Terrain", layer, parent, localPosition, localRotation, localScale);
			var terrain    = gameObject.AddComponent<SgtTerrain>();

			return terrain;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Terrain", false, 10)]
		public static void CreateMenuItem()
		{
			var parent  = SgtHelper.GetSelectedParent();
			var terrain = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(terrain);
		}
#endif

		protected override void OnEnable()
		{
			base.OnEnable();

			SgtHelper.OnCalculateDistance += CalculateDistance;

			SgtFloatingCamera.OnSnap += CameraSnapped;

			if (NegativeX != null) NegativeX.gameObject.SetActive(true);
			if (NegativeY != null) NegativeY.gameObject.SetActive(true);
			if (NegativeZ != null) NegativeZ.gameObject.SetActive(true);
			if (PositiveX != null) PositiveX.gameObject.SetActive(true);
			if (PositiveY != null) PositiveY.gameObject.SetActive(true);
			if (PositiveZ != null) PositiveZ.gameObject.SetActive(true);

			if (invalid == true)
			{
				Rebuild();
			}

			if (Application.isPlaying == true)
			{
				StartCoroutine(UpdateFaces());
			}
		}

		protected virtual void Update()
		{
			if (DelayRebuild == true)
			{
				Rebuild();
			}

			if (SharedMaterial != null && ExpectedSharedMaterial != SharedMaterial.Material)
			{
				DelayUpdateRenderers = true;
			}

			if (DelayUpdateRenderers == true)
			{
				UpdateRenderers();
			}

			if (DelayUpdateColliders == true)
			{
				UpdateColliders();
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			SgtHelper.OnCalculateDistance -= CalculateDistance;

			SgtFloatingCamera.OnSnap -= CameraSnapped;

			if (NegativeX != null) NegativeX.gameObject.SetActive(false);
			if (NegativeY != null) NegativeY.gameObject.SetActive(false);
			if (NegativeZ != null) NegativeZ.gameObject.SetActive(false);
			if (PositiveX != null) PositiveX.gameObject.SetActive(false);
			if (PositiveY != null) PositiveY.gameObject.SetActive(false);
			if (PositiveZ != null) PositiveZ.gameObject.SetActive(false);

			StopAllCoroutines();

			SgtTerrainCompute.Destroy();
		}

		protected virtual void OnDestroy()
		{
			SgtTerrainFace.MarkForDestruction(NegativeX);
			SgtTerrainFace.MarkForDestruction(NegativeY);
			SgtTerrainFace.MarkForDestruction(NegativeZ);
			SgtTerrainFace.MarkForDestruction(PositiveX);
			SgtTerrainFace.MarkForDestruction(PositiveY);
			SgtTerrainFace.MarkForDestruction(PositiveZ);
		}

		[System.NonSerialized]
		private List<SgtTerrainFace> faces = new List<SgtTerrainFace>();

		private IEnumerator UpdateFaces()
		{
			var timer     = new System.Diagnostics.Stopwatch();
			var frequency = 1.0 / System.Diagnostics.Stopwatch.Frequency;

			while (true)
			{
				faces.Clear();

				faces.Add(NegativeX);
				faces.Add(NegativeY);
				faces.Add(NegativeZ);
				faces.Add(PositiveX);
				faces.Add(PositiveY);
				faces.Add(PositiveZ);

				BeginSplitChecks(ref timer);

				while (faces.Count > 0)
				{
					var index   = faces.Count - 1;
					var face    = faces[index];
					var seconds = timer.ElapsedTicks * frequency;

					faces.RemoveAt(index);

					if (seconds > Budget)
					{
						yield return new WaitForEndOfFrame();

						BeginSplitChecks(ref timer);
					}

					face.UpdateSplit();

					if (face.Split == true)
					{
						faces.Add(face.ChildBL);
						faces.Add(face.ChildBR);
						faces.Add(face.ChildTL);
						faces.Add(face.ChildTR);
					}
				}

				//Debug.Log("Took " + (timer.ElapsedTicks * frequency));

				yield return new WaitForEndOfFrame();
			}
		}

		private void BeginSplitChecks(ref System.Diagnostics.Stopwatch timer)
		{
			timer.Stop();
			timer.Reset();
			timer.Start();

			CalculateLocalValues();
		}

		private SgtTerrainFace CreateFace(CubemapFace side, Vector3 cornerBL, Vector3 cornerBR, Vector3 cornerTL, Vector3 cornerTR)
		{
			var face     = SgtTerrainFace.Create(side.ToString(), gameObject.layer, transform);
			var z        = Mathf.Atan(Mathf.Sqrt(0.5f)) * Mathf.Rad2Deg;
			var rotation = Quaternion.Euler(-z, 0.0f, 45.0f);

			face.Terrain  = this;
			face.Side     = side;
			face.Depth    = 0;
			face.CornerBL = new SgtVector3D(rotation * cornerBL);
			face.CornerBR = new SgtVector3D(rotation * cornerBR);
			face.CornerTL = new SgtVector3D(rotation * cornerTL);
			face.CornerTR = new SgtVector3D(rotation * cornerTR);

			return face;
		}

		private void SetPosition(Vector3 position)
		{
			if (NegativeX != null) NegativeX.transform.localPosition = position;
			if (NegativeY != null) NegativeY.transform.localPosition = position;
			if (NegativeZ != null) NegativeZ.transform.localPosition = position;
			if (PositiveX != null) PositiveX.transform.localPosition = position;
			if (PositiveY != null) PositiveY.transform.localPosition = position;
			if (PositiveZ != null) PositiveZ.transform.localPosition = position;
		}

		private void CalculateDistance(Vector3 worldPosition, ref float bestDistance)
		{
			var localPosition        = transform.InverseTransformPoint(worldPosition);
			var surfaceLocalPosition = SampleLocalOutput(new SgtVector3D(localPosition)).Vertex;
			var surfaceWorldPosition = transform.TransformPoint(surfaceLocalPosition);
			var distance             = Vector3.Distance(surfaceWorldPosition, worldPosition);

			if (distance < bestDistance)
			{
				bestDistance = distance;
			}
		}
	}
}