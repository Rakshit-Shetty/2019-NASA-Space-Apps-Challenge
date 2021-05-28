using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtLightningSpawner))]
	public class SgtLightningSpawner_Editor : SgtEditor<SgtLightningSpawner>
	{
		protected override void OnInspector()
		{
			var updateMesh = false;

			BeginError(Any(t => t.DelayMin > t.DelayMax));
				DrawDefault("DelayMin", "The minimum delay between lightning spawns.");
				DrawDefault("DelayMax", "The maximum delay between lightning spawns.");
			EndError();

			Separator();

			BeginError(Any(t => t.LifeMin > t.LifeMax));
				DrawDefault("LifeMin", "The minimum life of each spawned lightning.");
				DrawDefault("LifeMax", "The maximum life of each spawned lightning.");
			EndError();

			Separator();

			BeginError(Any(t => t.Radius <= 0.0f));
				DrawDefault("Radius", ref updateMesh, "The radius of the spawned lightning mesh in local coordinates.");
			EndError();
			BeginError(Any(t => t.Size < 0.0f));
				DrawDefault("Size", ref updateMesh, "The size of the lightning in degrees.");
			EndError();
			BeginError(Any(t => t.Detail <= 0.0f));
				DrawDefault("Detail", ref updateMesh, "The amount of rows and columns in the lightning mesh.");
			EndError();
			DrawDefault("Colors", "When lightning is spawned, its base color will be randomly picked from this gradient.");
			DrawDefault("Brightness", "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			BeginError(Any(t => t.Sprites == null || t.Sprites.Count == 0));
				DrawDefault("Sprites", "The random sprite used by the lightning.");
			EndError();

			if (updateMesh == true) DirtyEach(t => t.UpdateMesh());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to spawn animated lightning sprites around a planet.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtLightningSpawner")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Lightning Spawner")]
	public class SgtLightningSpawner : MonoBehaviour
	{
		/// <summary>The minimum delay between lightning spawns.</summary>
		public float DelayMin = 0.25f;

		/// <summary>The maximum delay between lightning spawns.</summary>
		public float DelayMax = 5.0f;

		/// <summary>The minimum life of each spawned lightning.</summary>
		public float LifeMin = 0.5f;

		/// <summary>The maximum life of each spawned lightning.</summary>
		public float LifeMax = 1.0f;

		/// <summary>The radius of the spawned lightning mesh in local coordinates.</summary>
		public float Radius = 1.0f;

		/// <summary>The size of the lightning in degrees.</summary>
		public float Size = 10.0f;

		/// <summary>The amount of rows and columns in the lightning mesh.</summary>
		[Range(1, 100)]
		public int Detail = 10;

		/// <summary>When lightning is spawned, its base color will be randomly picked from this gradient.</summary>
		public Gradient Colors;

		/// <summary>The lightning color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness = 1.0f;

		/// <summary>The random sprite used by the lightning.</summary>
		public List<Sprite> Sprites;

		[System.NonSerialized]
		private Mesh mesh;

		// When this reaches 0 a new lightning is spawned
		[System.NonSerialized]
		private float cooldown;

		public Sprite RandomSprite
		{
			get
			{
				if (Sprites != null)
				{
					var count = Sprites.Count;

					if (count > 0)
					{
						var index = Random.Range(0, count);

						return Sprites[index];
					}
				}

				return null;
			}
		}

		public Color RandomColor
		{
			get
			{
				if (Colors == null)
				{
					Colors = SgtHelper.CreateGradient(Color.white);
				}

				return Colors.Evaluate(Random.value);
			}
		}

		[ContextMenu("Update Mesh")]
		public void UpdateMesh()
		{
			if (mesh == null)
			{
				mesh = SgtHelper.CreateTempMesh("Lightning");
			}
			else
			{
				mesh.Clear(false);
			}

			var detailAddOne = Detail + 1;
			var positions    = new Vector3[detailAddOne * detailAddOne];
			var coords       = new Vector2[detailAddOne * detailAddOne];
			var indices      = new int[Detail * Detail * 6];
			var invDetail    = SgtHelper.Reciprocal(Detail);

			for (var y = 0; y < detailAddOne; y++)
			{
				for (var x = 0; x < detailAddOne; x++)
				{
					var vertex = x + y * detailAddOne;
					var fracX  = x * invDetail;
					var fracY  = y * invDetail;
					var angX   = (fracX - 0.5f) * Size;
					var angY   = (fracY - 0.5f) * Size;

					// TODO: Manually do this rotation
					positions[vertex] = Quaternion.Euler(angX, angY, 0.0f) * new Vector3(0.0f, 0.0f, Radius);

					coords[vertex] = new Vector2(fracX, fracY);
				}
			}

			for (var y = 0; y < Detail; y++)
			{
				for (var x = 0; x < Detail; x++)
				{
					var index  = (x + y * Detail) * 6;
					var vertex = x + y * detailAddOne;

					indices[index + 0] = vertex;
					indices[index + 1] = vertex + 1;
					indices[index + 2] = vertex + detailAddOne;
					indices[index + 3] = vertex + detailAddOne + 1;
					indices[index + 4] = vertex + detailAddOne;
					indices[index + 5] = vertex + 1;
				}
			}

			mesh.vertices  = positions;
			mesh.uv        = coords;
			mesh.triangles = indices;
		}

		public SgtLightning Spawn()
		{
			if (mesh != null && LifeMin > 0.0f && LifeMax > 0.0f)
			{
				var sprite = RandomSprite;

				if (sprite != null)
				{
					var lightning = SgtLightning.Create(this);
					var material  = lightning.Material;
					var uv        = SgtHelper.CalculateSpriteUV(sprite);

					if (material == null)
					{
						material = SgtHelper.CreateTempMaterial("Lightning (Generated)", SgtHelper.ShaderNamePrefix + "Lightning");

						lightning.SetMaterial(material);
					}

					lightning.Life = Random.Range(LifeMin, LifeMax);
					lightning.Age  = 0.0f;

					lightning.SetMesh(mesh);

					material.SetTexture(SgtShader._MainTex, sprite.texture);
					material.SetColor(SgtShader._Color, SgtHelper.Brighten(RandomColor, Brightness));
					material.SetFloat(SgtShader._Age, 0.0f);
					material.SetVector(SgtShader._Offset, new Vector2(uv.x, uv.y));
					material.SetVector(SgtShader._Scale, new Vector2(uv.z - uv.x, uv.w - uv.y));

					lightning.transform.localRotation = Random.rotation;

					return lightning;
				}
			}

			return null;
		}

		public static SgtLightningSpawner Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtLightningSpawner Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject       = SgtHelper.CreateGameObject("Lightning Spawner", layer, parent, localPosition, localRotation, localScale);
			var lightningSpawner = gameObject.AddComponent<SgtLightningSpawner>();

			return lightningSpawner;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Lightning Spawner", false, 10)]
		public static void CreateMenuItem()
		{
			var parent           = SgtHelper.GetSelectedParent();
			var lightningSpawner = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(lightningSpawner);
		}
#endif

		protected virtual void Awake()
		{
			ResetDelay();
		}

		protected virtual void OnEnable()
		{
			UpdateMesh();
		}

		protected virtual void Update()
		{
			cooldown -= Time.deltaTime;

			// Spawn new lightning?
			if (cooldown <= 0.0f)
			{
				ResetDelay();

				Spawn();
			}
		}

		protected virtual void OnDestroy()
		{
			if (mesh != null)
			{
				mesh.Clear(false);

				SgtObjectPool<Mesh>.Add(mesh);
			}
		}

		private void ResetDelay()
		{
			cooldown = Random.Range(DelayMin, DelayMax);
		}
	}
}