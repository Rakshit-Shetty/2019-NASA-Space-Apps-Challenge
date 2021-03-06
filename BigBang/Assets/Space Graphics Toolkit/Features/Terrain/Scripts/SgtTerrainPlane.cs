using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainPlane))]
	public class SgtTerrainPlane_Editor : SgtEditor<SgtTerrainPlane>
	{
		protected override void OnInspector()
		{
			DrawDefault("Detail", "The rows/columns of the collider plane.");
			DrawDefault("Size", "The size of the plane in world space.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component generates a plane undernearth the current GameObject on top of the nearest terrain surface, allowing you to use physics on terrains without colliders.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtTerrainPlane")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Plane")]
	public class SgtTerrainPlane : MonoBehaviour
	{
		/// <summary>The rows/columns of the collider plane.</summary>
		[Range(1, 10)]
		public int Detail = 1;

		/// <summary>The size of the plane in world space.</summary>
		public float Size = 1.0f;

		[SerializeField]
		private Mesh mesh;

		[SerializeField]
		private MeshCollider meshCollider;

		[System.NonSerialized]
		private Vector3[] positions;

		[System.NonSerialized]
		private int[] indices;

		protected virtual void FixedUpdate()
		{
			if (Detail > 0)
			{
				var target       = transform.position;
				var threshold    = Size * Size;
				var bestPoint    = default(Vector3);
				var bestTerrain  = default(SgtTerrain);
				var bestDistance = float.PositiveInfinity;
				var terrain      = SgtTerrain.FirstInstance;

				for (var i = 0; i < SgtTerrain.InstanceCount; i++)
				{
					var point    = terrain.GetWorldPoint(target);
					var distance = Vector3.SqrMagnitude(target - point);

					if (distance < threshold && distance < bestDistance)
					{
						bestPoint    = point;
						bestDistance = distance;
						bestTerrain  = terrain;
					}

					terrain = terrain.NextInstance;
				}

				if (bestTerrain != null)
				{
					Build(bestTerrain, bestPoint); return;
				}
			}

			Pool();
		}

		protected virtual void OnDestroy()
		{
			Pool();
		}

		private void Build(SgtTerrain terrain, Vector3 bestPoint)
		{
			if (meshCollider == null)
			{
				var gameObject = new GameObject("Plane");
	#if UNITY_EDITOR
				gameObject.hideFlags = HideFlags.DontSave;
	#endif
				meshCollider = gameObject.AddComponent<MeshCollider>();
			}

			if (mesh == null)
			{
				mesh = SgtObjectPool<Mesh>.Pop() ?? new Mesh();
	#if UNITY_EDITOR
				mesh.hideFlags = HideFlags.DontSave;
	#endif
				mesh.name = "Plane";
			}

			var sideE        = Detail;
			var sideP        = Detail + 1;
			var vertexCount  = sideP * sideP;
			var indexCount   = sideE * sideE * 6;
			var rotation     = Quaternion.Inverse(terrain.transform.rotation) * Quaternion.LookRotation(bestPoint);
			var distance     = bestPoint.magnitude;
			var uniformScale = SgtHelper.UniformScale(terrain.transform.lossyScale);
			var size         = Size * SgtHelper.Reciprocal(uniformScale);
			var step         = (size * 2.0f) / Detail;

			if (positions == null || positions.Length != vertexCount)
			{
				positions = new Vector3[vertexCount];
			}

			for (var y = 0; y <= Detail; y++)
			{
				for (var x = 0; x <= Detail; x++)
				{
					var index = x + y * sideP;
					var point = rotation * new Vector3(x * step - size, y * step - size, distance);

					positions[index] = (Vector3)terrain.GetLocalPoint(new SgtVector3D(point));
				}
			}

			// Regen indices?
			if (indices == null || indices.Length != indexCount)
			{
				indices = new int[indexCount];

				for (var y = 0; y < sideE; y++)
				{
					for (var x = 0; x < sideE; x++)
					{
						var index  = (x + y * sideE) * 6;
						var vertex = x + y * sideP;

						indices[index + 0] = vertex;
						indices[index + 1] = vertex + 1;
						indices[index + 2] = vertex + sideP;
						indices[index + 3] = vertex + sideP + 1;
						indices[index + 4] = vertex + sideP;
						indices[index + 5] = vertex + 1;
					}
				}

				mesh.Clear();
			}

			mesh.vertices  = positions;
			mesh.triangles = indices;

			meshCollider.sharedMesh = mesh;

			meshCollider.transform.SetParent(terrain.transform, false);
		}

		private void Pool()
		{
			if (meshCollider != null)
			{
				SgtHelper.Destroy(meshCollider.gameObject);

				meshCollider = null;
			}

			if (mesh != null)
			{
				mesh.Clear(false);

				mesh = SgtObjectPool<Mesh>.Add(mesh);
			}
		}
	}
}