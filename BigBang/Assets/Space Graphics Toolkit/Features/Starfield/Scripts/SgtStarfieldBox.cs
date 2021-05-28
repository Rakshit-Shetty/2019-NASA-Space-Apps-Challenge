using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtStarfieldBox))]
	public class SgtStarfieldBox_Editor : SgtStarfield_Editor<SgtStarfieldBox>
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
			BeginError(Any(t => t.Extents == Vector3.zero));
				DrawDefault("Extents", ref updateMeshesAndModels, "The +- size of the starfield.");
			EndError();
			DrawDefault("Offset", ref updateMeshesAndModels, "How far from the center the distribution begins.");
			DrawDefault("Bias", ref updateMeshesAndModels, "Invert the distribution?");

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
			DrawDefault("StarRadiusBias", ref updateMeshesAndModels, "How likely the size picking will pick smaller stars over larger ones (1 = default/linear).");
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
	/// <summary>This component allows you to render a starfield with a distribution of a box.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtStarfieldBox")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Starfield Box")]
	public class SgtStarfieldBox : SgtStarfield
	{
		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public SgtSeed Seed;

		/// <summary>The +- size of the starfield.</summary>
		public Vector3 Extents = Vector3.one;

		/// <summary>How far from the center the distribution begins.</summary>
		[Range(0.0f, 1.0f)]
		public float Offset;

		/// <summary>Invert the distribution?</summary>
		public float Bias;

		/// <summary>The amount of stars that will be generated in the starfield.</summary>
		public int StarCount = 1000;

		/// <summary>Each star is given a random color from this gradient.</summary>
		public Gradient StarColors;

		/// <summary>The minimum radius of stars in the starfield.</summary>
		public float StarRadiusMin = 0.01f;

		/// <summary>The maximum radius of stars in the starfield.</summary>
		public float StarRadiusMax = 0.05f;

		/// <summary>How likely the size picking will pick smaller stars over larger ones (1 = default/linear).</summary>
		public float StarRadiusBias;

		/// <summary>The maximum amount a star's size can pulse over time. A value of 1 means the star can potentially pulse between its maximum size, and 0.</summary>
		[Range(0.0f, 1.0f)]
		public float StarPulseMax = 1.0f;

		public static SgtStarfieldBox Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtStarfieldBox Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject   = SgtHelper.CreateGameObject("Starfield Box", layer, parent, localPosition, localRotation, localScale);
			var starfieldBox = gameObject.AddComponent<SgtStarfieldBox>();

			return starfieldBox;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Starfield Box", false, 10)]
		private static void CreateMenuItem()
		{
			var parent       = SgtHelper.GetSelectedParent();
			var starfieldBox = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(starfieldBox);
		}
#endif

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireCube(Vector3.zero, 2.0f * Extents);
			Gizmos.DrawWireCube(Vector3.zero, 2.0f * Extents * Offset);
		}
#endif

		protected override int BeginQuads()
		{
			SgtHelper.BeginRandomSeed(Seed);

			if (StarColors == null)
			{
				StarColors = SgtHelper.CreateGradient(Color.white);
			}

			return StarCount;
		}

		protected override void NextQuad(ref SgtStarfieldStar star, int starIndex)
		{
			var x        = Random.Range( -1.0f, 1.0f);
			var y        = Random.Range( -1.0f, 1.0f);
			var z        = Random.Range(Offset, 1.0f);
			var position = default(Vector3);

			if (Random.value >= 0.5f)
			{
				z = -z;
			}

			switch (Random.Range(0, 3))
			{
				case 0: position = new Vector3(z, x, y); break;
				case 1: position = new Vector3(x, z, y); break;
				case 2: position = new Vector3(x, y, z); break;
			}

			star.Variant     = Random.Range(int.MinValue, int.MaxValue);
			star.Color       = StarColors.Evaluate(Random.value);
			star.Radius      = Mathf.Lerp(StarRadiusMin, StarRadiusMax, SgtHelper.Sharpness(Random.value, StarRadiusBias));
			star.Angle       = Random.Range(-180.0f, 180.0f);
			star.Position    = Vector3.Scale(position, Extents);
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