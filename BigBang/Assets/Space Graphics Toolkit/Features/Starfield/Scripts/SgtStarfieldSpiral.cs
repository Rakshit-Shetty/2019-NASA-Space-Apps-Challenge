using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtStarfieldSpiral))]
	public class SgtStarfieldSpiral_Editor : SgtStarfield_Editor<SgtStarfieldSpiral>
	{
		protected override void OnInspector()
		{
			var updateMaterial        = false;
			var updateMeshesAndModels = false;

			DrawMaterial(ref updateMaterial);

			Separator();

			DrawMainTex(ref updateMaterial, ref updateMeshesAndModels);
			DrawLayout(ref updateMaterial, ref updateMeshesAndModels);

			Separator();

			DrawPointMaterial(ref updateMaterial);

			Separator();

			DrawDefault("Seed", ref updateMeshesAndModels, "This allows you to set the random seed used during procedural generation.");
			DrawDefault("Radius", ref updateMeshesAndModels, "The radius of the starfield.");
			BeginError(Any(t => t.ArmCount <= 0));
				DrawDefault("ArmCount", ref updateMeshesAndModels, "The amount of spiral arms.");
			EndError();
			DrawDefault("Twist", ref updateMeshesAndModels, "The amound each arm twists.");
			DrawDefault("ThicknessInner", ref updateMeshesAndModels, "This allows you to set the thickness of the star distribution at the center of the spiral.");
			DrawDefault("ThicknessOuter", ref updateMeshesAndModels, "This allows you to set the thickness of the star distribution at the edge of the spiral.");
			DrawDefault("ThicknessPower", ref updateMeshesAndModels, "This allows you to push stars away from the spiral, giving you a smoother distribution.");

			Separator();

			BeginError(Any(t => t.StarCount < 0));
				DrawDefault("StarCount", ref updateMeshesAndModels, "The amount of stars that will be generated in the starfield.");
			EndError();
			DrawDefault("StarColors", ref updateMeshesAndModels, "Each star is given a random color from this gradient.");
			BeginError(Any(t => t.StarRadiusMin < 0.0f || t.StarRadiusMin > t.StarRadiusMax));
				DrawDefault("StarRadiusMin", ref updateMeshesAndModels, "The minimum radius of stars in the starfield.");
			EndError();
			BeginError(Any(t => t.StarRadiusMax < 0.0f || t.StarRadiusMin > t.StarRadiusMax));
				DrawDefault("StarRadiusMax", ref updateMeshesAndModels, "The maximum radius of stars in the starfield.");
			EndError();
			DrawDefault("StarRadiusBias", ref updateMeshesAndModels, "How likely the size picking will pick smaller stars over larger ones (0 = default/linear).");
			DrawDefault("StarPulseMax", ref updateMeshesAndModels, "The maximum amount a star's size can pulse over time. A value of 1 means the star can potentially pulse between its maximum size, and 0.");

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
	/// <summary>This component allows you to generate a starfield in a spiral pattern.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtStarfieldSpiral")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Starfield Spiral")]
	public class SgtStarfieldSpiral : SgtStarfield
	{
		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public SgtSeed Seed;

		/// <summary>The radius of the starfield.</summary>
		public float Radius = 1.0f;

		/// <summary>The amount of spiral arms.</summary>
		public int ArmCount = 1;

		/// <summary>The amound each arm twists.</summary>
		public float Twist = 1.0f;

		/// <summary>This allows you to set the thickness of the star distribution at the center of the spiral.</summary>
		public float ThicknessInner = 0.1f;

		/// <summary>This allows you to set the thickness of the star distribution at the edge of the spiral.</summary>
		public float ThicknessOuter = 0.3f;

		/// <summary>This allows you to push stars away from the spiral, giving you a smoother distribution.</summary>
		public float ThicknessPower = 1.0f;

		/// <summary>The amount of stars that will be generated in the starfield.</summary>
		public int StarCount = 1000;

		/// <summary>Each star is given a random color from this gradient.</summary>
		public Gradient StarColors;

		/// <summary>The minimum radius of stars in the starfield.</summary>
		public float StarRadiusMin = 0.0f;

		/// <summary>The maximum radius of stars in the starfield.</summary>
		public float StarRadiusMax = 0.05f;

		/// <summary>How likely the size picking will pick smaller stars over larger ones (1 = default/linear).</summary>
		public float StarRadiusBias = 0.0f;

		/// <summary>The maximum amount a star's size can pulse over time. A value of 1 means the star can potentially pulse between its maximum size, and 0.</summary>
		[Range(0.0f, 1.0f)]
		public float StarPulseMax = 1.0f;

		// Temp vars used during generation
		private static float armStep;
		private static float twistStep;

		public static SgtStarfieldSpiral Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtStarfieldSpiral Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject      = SgtHelper.CreateGameObject("Starfield Spiral", layer, parent, localPosition, localRotation, localScale);
			var starfieldSpiral = gameObject.AddComponent<SgtStarfieldSpiral>();

			return starfieldSpiral;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Starfield Spiral", false, 10)]
		private static void CreateMenuItem()
		{
			var parent          = SgtHelper.GetSelectedParent();
			var starfieldSpiral = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(starfieldSpiral);
		}
#endif

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireSphere(Vector3.zero, Radius);
		}
#endif

		protected override int BeginQuads()
		{
			SgtHelper.BeginRandomSeed(Seed);

			if (StarColors == null)
			{
				StarColors = SgtHelper.CreateGradient(Color.white);
			}

			armStep   = 360.0f * SgtHelper.Reciprocal(ArmCount);
			twistStep = 360.0f * Twist;

			return StarCount;
		}

		protected override void NextQuad(ref SgtStarfieldStar star, int starIndex)
		{
			var magnitude = Random.value;
			var position  = Random.insideUnitSphere * Mathf.Lerp(ThicknessInner, ThicknessOuter, magnitude);

			position *= Mathf.Pow(2.0f, Random.value * ThicknessPower);

			position += Quaternion.AngleAxis(starIndex * armStep + magnitude * twistStep, Vector3.up) * Vector3.forward * magnitude;

			star.Variant     = Random.Range(int.MinValue, int.MaxValue);
			star.Color       = StarColors.Evaluate(Random.value);
			star.Radius      = Mathf.Lerp(StarRadiusMin, StarRadiusMax, SgtHelper.Sharpness(Random.value, StarRadiusBias));
			star.Angle       = Random.Range(-180.0f, 180.0f);
			star.Position    = position * Radius;
			star.PulseRange  = Random.value * StarPulseMax;
			star.PulseSpeed  = Random.value;
			star.PulseOffset = Random.value;
		}

		protected override void EndQuads()
		{
			SgtHelper.EndRandomSeed();
		}
	}
}