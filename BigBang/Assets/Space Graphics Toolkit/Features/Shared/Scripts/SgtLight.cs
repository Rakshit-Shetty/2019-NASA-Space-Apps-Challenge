using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtLight))]
	public class SgtLight_Editor : SgtEditor<SgtLight>
	{
		protected override void OnInspector()
		{
			EditorGUILayout.HelpBox("This component marks the attached Light as one that will be used by SGT. Most SGT features only work with a limited amount of lights, so having to explicitly define which ones will be used helps stop you going over this limit.", MessageType.Info);
			
			Separator();

			DrawDefault("TreatAsPoint");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component marks the attached Light as one that will be used by SGT. Most SGT features only work with a limited amount of lights, so having to explicitly define which ones will be used helps stop you going over this limit.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Light))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtLight")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Light")]
	public class SgtLight : SgtLinkedBehaviour<SgtLight>
	{
		/// <summary>The SgtLightPointer component allows you to treat a Directional light like a Point light, and enabling this allows you to notify SGT about this.</summary>
		public bool TreatAsPoint = false;

		[System.NonSerialized]
		private Light cachedLight;

		[System.NonSerialized]
		private bool cachedLightSet;

		private static List<LightProperties> cachedLightProperties = new List<LightProperties>();

		private static List<string> cachedLightKeywords = new List<string>();

		private static List<SgtLight> tempLights = new List<SgtLight>();

		public Light CachedLight
		{
			get
			{
				if (cachedLightSet == false)
				{
					cachedLight    = GetComponent<Light>();
					cachedLightSet = true;
				}

				return cachedLight;
			}
		}

		public static List<SgtLight> Find(bool lit, int mask)
		{
			tempLights.Clear();

			if (lit == true)
			{
				var light = FirstInstance;

				for (var i = 0; i < InstanceCount; i++)
				{
					var cachedLight = light.CachedLight;

					if (SgtHelper.Enabled(cachedLight) == true && cachedLight.intensity > 0.0f && (cachedLight.cullingMask & mask) != 0)
					{
						tempLights.Add(light);
					}

					light = light.NextInstance;
				}

				// TODO: Sort by apparent brightness
			}

			return tempLights;
		}

		public static void FilterOut(Vector3 center)
		{
			for (var i = tempLights.Count - 1; i >= 0; i--)
			{
				var tempLight = tempLights[i];

				if (tempLight.transform.position == center)
				{
					tempLights.RemoveAt(i);
				}
			}
		}

		public static void Calculate(SgtLight light, Vector3 center, Transform directionTransform, Transform positionTransform, ref Vector3 position, ref Vector3 direction, ref Color color)
		{
			if (light != null)
			{
				var cachedLight = light.CachedLight;

				direction = -light.transform.forward;
				position  = light.transform.position;
				color     = SgtHelper.Brighten(cachedLight.color, cachedLight.intensity);

				switch (cachedLight.type)
				{
					case LightType.Point:
					{
						direction = Vector3.Normalize(position - center);
					}
					break;

					case LightType.Directional:
					{
						if (light.TreatAsPoint == true)
						{
							direction = Vector3.Normalize(position - center);
						}
						else
						{
							position = center + direction * 10000000.0f;
						}
					}
					break;
				}

				// Transform into local space?
				if (directionTransform != null)
				{
					direction = directionTransform.InverseTransformDirection(direction);
				}

				if (positionTransform != null)
				{
					position = positionTransform.InverseTransformPoint(position);
				}
			}
		}

		public static void Write(bool lit, Vector3 center, Transform directionTransform, Transform positionTransform, float scatterStrength, int maxLights)
		{
			var lightCount = 0;

			for (var i = 0; i < tempLights.Count; i++)
			{
				var light      = tempLights[i];
				var properties = GetLightProperties(lightCount++);
				var direction  = default(Vector3);
				var position   = default(Vector3);
				var color      = default(Color);

				Calculate(light, center, directionTransform, positionTransform, ref position, ref direction, ref color);

				for (var j = SgtHelper.tempMaterials.Count - 1; j >= 0; j--)
				{
					var tempMaterial = SgtHelper.tempMaterials[j];

					if (tempMaterial != null)
					{
						tempMaterial.SetVector(properties.Direction, direction);
						tempMaterial.SetVector(properties.Position, SgtHelper.NewVector4(position, 1.0f));
						tempMaterial.SetColor(properties.Color, color);
						tempMaterial.SetColor(properties.Scatter, color * scatterStrength);
					}
				}

				if (lightCount >= maxLights)
				{
					break;
				}
			}

			for (var i = 0; i <= maxLights; i++)
			{
				var keyword = GetLightKeyword(i);

				if (lit == true && i == lightCount)
				{
					SgtHelper.EnableKeyword(keyword);
				}
				else
				{
					SgtHelper.DisableKeyword(keyword);
				}
			}
		}

		private class LightProperties
		{
			public int Direction;
			public int Position;
			public int Color;
			public int Scatter;
		}

		private static LightProperties GetLightProperties(int index)
		{
			for (var i = cachedLightProperties.Count; i <= index; i++)
			{
				var properties = new LightProperties();
				var prefix     = "_Light" + (i + 1);

				properties.Direction = Shader.PropertyToID(prefix + "Direction");
				properties.Position  = Shader.PropertyToID(prefix + "Position");
				properties.Color     = Shader.PropertyToID(prefix + "Color");
				properties.Scatter   = Shader.PropertyToID(prefix + "Scatter");

				cachedLightProperties.Add(properties);
			}

			return cachedLightProperties[index];
		}

		private static string GetLightKeyword(int index)
		{
			for (var i = cachedLightKeywords.Count; i <= index; i++)
			{
				cachedLightKeywords.Add("LIGHT_" + i);
			}

			return cachedLightKeywords[index];
		}
	}
}