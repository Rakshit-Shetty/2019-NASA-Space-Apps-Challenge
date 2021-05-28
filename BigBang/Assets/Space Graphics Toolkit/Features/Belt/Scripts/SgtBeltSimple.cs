using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtBeltSimple))]
	public class SgtBeltSimple_Editor : SgtBelt_Editor<SgtBeltSimple>
	{
		protected override void OnInspector()
		{
			var updateMaterial        = false;
			var updateMeshesAndModels = false;

			DrawMaterial(ref updateMaterial);

			Separator();

			DrawMainTex(ref updateMaterial, ref updateMeshesAndModels);

			Separator();

			DrawLighting(ref updateMaterial);

			Separator();

			DrawDefault("Seed", ref updateMeshesAndModels, "This allows you to set the random seed used during procedural generation.");
			DrawDefault("Thickness", ref updateMeshesAndModels, "The thickness of the belt in local coordinates.");
			BeginError(Any(t => t.ThicknessBias < 1.0f));
				DrawDefault("ThicknessBias", ref updateMeshesAndModels, "The higher this value, the less large asteroids will be generated.");
			EndError();
			BeginError(Any(t => t.InnerRadius < 0.0f || t.InnerRadius > t.OuterRadius));
				DrawDefault("InnerRadius", ref updateMeshesAndModels, "The radius of the inner edge of the belt in local coordinates.");
			EndError();
			DrawDefault("InnerSpeed", ref updateMeshesAndModels, "The speed of asteroids orbiting on the inner edge of the belt in radians.");
			BeginError(Any(t => t.OuterRadius < 0.0f || t.InnerRadius > t.OuterRadius));
				DrawDefault("OuterRadius", ref updateMeshesAndModels, "The radius of the outer edge of the belt in local coordinates.");
			EndError();
			DrawDefault("OuterSpeed", ref updateMeshesAndModels, "The speed of asteroids orbiting on the outer edge of the belt in radians.");

			Separator();

			DrawDefault("RadiusBias", ref updateMeshesAndModels, "The higher this value, the more likely asteroids will spawn on the inner edge of the ring.");
			DrawDefault("SpeedSpread", ref updateMeshesAndModels, "How much random speed can be added to each asteroid.");

			Separator();

			BeginError(Any(t => t.AsteroidCount < 0));
				DrawDefault("AsteroidCount", ref updateMeshesAndModels, "The amount of asteroids generated in the belt.");
			EndError();
			DrawDefault("AsteroidColors", ref updateMeshesAndModels, "Each asteroid is given a random color from this gradient.");
			DrawDefault("AsteroidSpin", ref updateMeshesAndModels, "The maximum amount of angular velcoity each asteroid has.");
			BeginError(Any(t => t.AsteroidRadiusMin < 0.0f || t.AsteroidRadiusMin > t.AsteroidRadiusMax));
				DrawDefault("AsteroidRadiusMin", ref updateMeshesAndModels, "The minimum asteroid radius in local coordinates.");
			EndError();
			BeginError(Any(t => t.AsteroidRadiusMax < 0.0f || t.AsteroidRadiusMin > t.AsteroidRadiusMax));
				DrawDefault("AsteroidRadiusMax", ref updateMeshesAndModels, "The maximum asteroid radius in local coordinates.");
			EndError();
			DrawDefault("AsteroidRadiusBias", ref updateMeshesAndModels, "How likely the size picking will pick smaller asteroids over larger ones (0 = default/linear).");

			RequireCamera();

			serializedObject.ApplyModifiedProperties();

			if (updateMaterial        == true) DirtyEach(t => t.UpdateMaterial       ());
			if (updateMeshesAndModels == true) DirtyEach(t => t.UpdateMeshesAndModels());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate an asteroid belt with a simple exponential distribution.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtBeltSimple")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Belt Simple")]
	public class SgtBeltSimple : SgtBelt
	{
		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public SgtSeed Seed;

		/// <summary>The thickness of the belt in local coordinates.</summary>
		public float Thickness;

		/// <summary>The higher this value, the less large asteroids will be generated.</summary>
		public float ThicknessBias = 1.0f;

		/// <summary>The radius of the inner edge of the belt in local coordinates.</summary>
		public float InnerRadius = 1.0f;

		/// <summary>The speed of asteroids orbiting on the inner edge of the belt in radians.</summary>
		public float InnerSpeed = 0.1f;

		/// <summary>The radius of the outer edge of the belt in local coordinates.</summary>
		public float OuterRadius = 2.0f;

		/// <summary>The speed of asteroids orbiting on the outer edge of the belt in radians.</summary>
		public float OuterSpeed = 0.05f;

		/// <summary>The higher this value, the more likely asteroids will spawn on the inner edge of the ring.</summary>
		public float RadiusBias = 0.25f;

		/// <summary>How much random speed can be added to each asteroid.</summary>
		public float SpeedSpread;

		/// <summary>The amount of asteroids generated in the belt.</summary>
		public int AsteroidCount = 1000;

		/// <summary>Each asteroid is given a random color from this gradient.</summary>
		public Gradient AsteroidColors;

		/// <summary>The maximum amount of angular velcoity each asteroid has.</summary>
		public float AsteroidSpin = 1.0f;

		/// <summary>The minimum asteroid radius in local coordinates.</summary>
		public float AsteroidRadiusMin = 0.025f;

		/// <summary>The maximum asteroid radius in local coordinates.</summary>
		public float AsteroidRadiusMax = 0.05f;

		/// <summary>How likely the size picking will pick smaller asteroids over larger ones (1 = default/linear).</summary>
		public float AsteroidRadiusBias = 0.0f;

		public static SgtBeltSimple Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtBeltSimple Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Belt Simple", layer, parent, localPosition, localRotation, localScale);
			var simpleBelt = gameObject.AddComponent<SgtBeltSimple>();

			return simpleBelt;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Belt Simple", false, 10)]
		public static void CreateMenuItem()
		{
			var parent     = SgtHelper.GetSelectedParent();
			var simpleBelt = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(simpleBelt);
		}
#endif

		protected override int BeginQuads()
		{
			SgtHelper.BeginRandomSeed(Seed);

			if (AsteroidColors == null)
			{
				AsteroidColors = SgtHelper.CreateGradient(Color.white);
			}

			return AsteroidCount;
		}

		protected override void NextQuad(ref SgtBeltAsteroid asteroid, int asteroidIndex)
		{
			var distance01 = SgtHelper.Sharpness(Random.value * Random.value, RadiusBias);

			asteroid.Variant       = Random.Range(int.MinValue, int.MaxValue);
			asteroid.Color         = AsteroidColors.Evaluate(Random.value);
			asteroid.Radius        = Mathf.Lerp(AsteroidRadiusMin, AsteroidRadiusMax, SgtHelper.Sharpness(Random.value, AsteroidRadiusBias));
			asteroid.Height        = Mathf.Pow(Random.value, ThicknessBias) * Thickness * (Random.value < 0.5f ? -0.5f : 0.5f);
			asteroid.Angle         = Random.Range(0.0f, Mathf.PI * 2.0f);
			asteroid.Spin          = Random.Range(-AsteroidSpin, AsteroidSpin);
			asteroid.OrbitAngle    = Random.Range(0.0f, Mathf.PI * 2.0f);
			asteroid.OrbitSpeed    = Mathf.Lerp(InnerSpeed, OuterSpeed, distance01);
			asteroid.OrbitDistance = Mathf.Lerp(InnerRadius, OuterRadius, distance01);

			asteroid.OrbitSpeed += Random.Range(-SpeedSpread, SpeedSpread) * asteroid.OrbitSpeed;
		}

		protected override void EndQuads()
		{
			SgtHelper.EndRandomSeed();
		}
	}
}