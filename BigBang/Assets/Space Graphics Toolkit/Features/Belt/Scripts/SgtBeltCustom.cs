using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtBeltCustom))]
	public class SgtBeltCustom_Editor : SgtBelt_Editor<SgtBeltCustom>
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

			DrawDefault("Asteroids", ref updateMeshesAndModels, "The custom asteroids in this belt.");

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
	/// <summary>This component allows you to specify the exact position/size/etc of each asteroid in this asteroid belt.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtBeltCustom")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Belt Custom")]
	public class SgtBeltCustom : SgtBelt
	{
		/// <summary>The custom asteroids in this belt.</summary>
		public List<SgtBeltAsteroid> Asteroids;

		public static SgtBeltCustom Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtBeltCustom Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Belt Custom", layer, parent, localPosition, localRotation, localScale);
			var beltCustom = gameObject.AddComponent<SgtBeltCustom>();

			return beltCustom;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Belt Custom", false, 10)]
		public static void CreateMenuItem()
		{
			var parent     = SgtHelper.GetSelectedParent();
			var beltCustom = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(beltCustom);
		}
#endif

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (Asteroids != null)
			{
				for (var i = Asteroids.Count - 1; i >= 0; i--)
				{
					SgtPoolClass<SgtBeltAsteroid>.Add(Asteroids[i]);
				}
			}
		}

		protected override int BeginQuads()
		{
			if (Asteroids != null)
			{
				return Asteroids.Count;
			}

			return 0;
		}

		protected override void NextQuad(ref SgtBeltAsteroid asteroid, int asteroidIndex)
		{
			asteroid.CopyFrom(Asteroids[asteroidIndex]);
		}

		protected override void EndQuads()
		{
		}
	}
}