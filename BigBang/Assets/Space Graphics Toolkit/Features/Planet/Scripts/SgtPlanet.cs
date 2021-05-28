using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtPlanet))]
	public class SgtPlanet_Editor : SgtEditor<SgtPlanet>
	{
		protected override void OnInspector()
		{
			EditorGUILayout.HelpBox("If the attached Renderer uses the SGT Planet shader, then this component allows you to control its settings in game. You can control the settings using unity events from buttons/sliders/etc.", MessageType.Info);

			Separator();

			DrawDefault("Dampening", "The speed the values changes (-1 = instant).");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component rotates the current GameObject.</summary>
	[RequireComponent(typeof(Renderer))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtPlanet")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Planet")]
	public class SgtPlanet : MonoBehaviour
	{
		/// <summary>The speed the values changes (-1 = instant).</summary>
		public float Dampening = 5.0f;

		[SerializeField]
		private float targetWaterLevel;

		[SerializeField]
		private Material clonedMaterial;

		public void SetWaterLevel(float value)
		{
			targetWaterLevel = value;

			UpdateMaterial();
		}

		protected virtual void Update()
		{
			if (clonedMaterial != null)
			{
				var factor = SgtHelper.DampenFactor(Dampening, Time.deltaTime);
				var level  = clonedMaterial.GetFloat(SgtShader._WaterLevel);

				level = Mathf.Lerp(level, targetWaterLevel, factor);

				clonedMaterial.SetFloat(SgtShader._WaterLevel, level);
			}
		}

		private void UpdateMaterial()
		{
			if (clonedMaterial == null)
			{
				var renderer        = GetComponent<Renderer>();
				var sharedMaterials = renderer.sharedMaterials;

				if (sharedMaterials.Length > 0)
				{
					clonedMaterial = sharedMaterials[0] = Instantiate(sharedMaterials[0]);

					renderer.sharedMaterials = sharedMaterials;
				}
			}
		}
	}
}