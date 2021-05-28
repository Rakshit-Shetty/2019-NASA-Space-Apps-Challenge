using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtStarfieldCustom))]
	public class SgtStarfieldCustom_Editor : SgtStarfield_Editor<SgtStarfieldCustom>
	{
		protected override void OnInspector()
		{
			var updateMaterial        = false;
			var updateMeshesAndModels = false;

			DrawMaterial(ref updateMaterial);

			Separator();

			DrawMainTex(ref updateMaterial, ref updateMeshesAndModels);

			Separator();

			DrawPointMaterial(ref updateMaterial);

			Separator();

			DrawDefault("Stars", ref updateMeshesAndModels, "The stars that will be rendered by this starfield.");

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
	/// <summary>This component allows you to specify the exact position/size/etc of each star in this starfield.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtStarfieldCustom")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Starfield Custom")]
	public class SgtStarfieldCustom : SgtStarfield
	{
		/// <summary>The stars that will be rendered by this starfield.</summary>
		public List<SgtStarfieldStar> Stars;

		public static SgtStarfieldCustom Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtStarfieldCustom Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject      = SgtHelper.CreateGameObject("Starfield Custom", layer, parent, localPosition, localRotation, localScale);
			var starfieldCustom = gameObject.AddComponent<SgtStarfieldCustom>();

			return starfieldCustom;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Starfield Custom", false, 10)]
		private static void CreateMenuItem()
		{
			var parent          = SgtHelper.GetSelectedParent();
			var starfieldCustom = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(starfieldCustom);
		}
#endif

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (Stars != null)
			{
				for (var i = Stars.Count - 1; i >= 0; i--)
				{
					SgtPoolClass<SgtStarfieldStar>.Add(Stars[i]);
				}
			}
		}

		protected override int BeginQuads()
		{
			if (Stars != null)
			{
				return Stars.Count;
			}

			return 0;
		}

		protected override void NextQuad(ref SgtStarfieldStar quad, int starIndex)
		{
			quad.CopyFrom(Stars[starIndex]);
		}

		protected override void EndQuads()
		{
		}
	}
}