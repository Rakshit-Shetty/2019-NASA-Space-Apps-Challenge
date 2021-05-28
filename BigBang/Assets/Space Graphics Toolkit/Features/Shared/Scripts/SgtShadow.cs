using UnityEngine;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	/// <summary>This base class handles calculation of a shadow matrix and shadow texture.</summary>
	public abstract class SgtShadow : SgtLinkedBehaviour<SgtShadow>
	{
		private static List<ShadowProperties> cachedShadowProperties = new List<ShadowProperties>();

		private static List<string> cachedShadowKeywords = new List<string>();

		private static List<SgtShadow> tempShadows = new List<SgtShadow>();

		public abstract Texture GetTexture();

		public abstract void CalculateShadow(SgtLight light);

		[System.NonSerialized]
		private bool calculatedThisFrame;

		[System.NonSerialized]
		protected bool cachedActive;

		[System.NonSerialized]
		protected Texture cachedTexture;

		[System.NonSerialized]
		protected Matrix4x4 cachedMatrix;

		[System.NonSerialized]
		protected float cachedRatio;

		[System.NonSerialized]
		protected float cachedRadius;

		private class ShadowProperties
		{
			public int Texture;
			public int Matrix;
			public int Ratio;
		}

		private static ShadowProperties GetShadowProperties(int index)
		{
			for (var i = cachedShadowProperties.Count; i <= index; i++)
			{
				var properties = new ShadowProperties();
				var prefix     = "_Shadow" + (i + 1);

				properties.Texture = Shader.PropertyToID(prefix + "Texture");
				properties.Matrix  = Shader.PropertyToID(prefix + "Matrix");
				properties.Ratio   = Shader.PropertyToID(prefix + "Ratio");

				cachedShadowProperties.Add(properties);
			}

			return cachedShadowProperties[index];
		}

		private static string GetShadowKeyword(int index)
		{
			for (var i = cachedShadowKeywords.Count; i <= index; i++)
			{
				cachedShadowKeywords.Add("SHADOW_" + i);
			}

			return cachedShadowKeywords[index];
		}

		public static List<SgtShadow> Find(bool lit, int mask, List<SgtLight> lights)
		{
			tempShadows.Clear();

			if (lit == true && lights != null && lights.Count > 0)
			{
				var shadow = FirstInstance;

				for (var i = 0; i < InstanceCount; i++)
				{
					var mask2 = 1 << shadow.gameObject.layer;

					if ((mask & mask2) != 0)
					{
						if (shadow.calculatedThisFrame == false)
						{
							shadow.calculatedThisFrame = true;

							shadow.CalculateShadow(lights[0]);
						}

						if (shadow.cachedActive == true)
						{
							tempShadows.Add(shadow);
						}
					}

					shadow = shadow.NextInstance;
				}
			}

			return tempShadows;
		}

		public static void FilterOutSphere(Vector3 center)
		{
			for (var i = tempShadows.Count - 1; i >= 0; i--)
			{
				var tempShadow = tempShadows[i];

				if (tempShadow is SgtShadowSphere && tempShadow.transform.position == center)
				{
					tempShadows.RemoveAt(i);
				}
			}
		}

		public static void FilterOutRing(Vector3 center)
		{
			for (var i = tempShadows.Count - 1; i >= 0; i--)
			{
				var tempShadow = tempShadows[i];

				if (tempShadow is SgtShadowRing && tempShadow.transform.position == center)
				{
					tempShadows.RemoveAt(i);
				}
			}
		}

		public static void FilterOutMiss(Vector3 center, float radius)
		{
			for (var i = tempShadows.Count - 1; i >= 0; i--)
			{
				var tempShadow = tempShadows[i];
				var point      = tempShadow.cachedMatrix.MultiplyPoint(center);

				if (point.z > 0.0f)
				{
					var distance = Mathf.Sqrt(point.x * point.x + point.y * point.y);

					if (distance * tempShadow.cachedRadius <= radius + tempShadow.cachedRadius)
					{
						continue;
					}
				}

				tempShadows.RemoveAt(i);
			}
		}

		public static void Write(bool lit, int mask, int maxShadows)
		{
			var shadowCount = 0;

			for (var i = 0; i < tempShadows.Count; i++)
			{
				var shadow     = tempShadows[i];
				var properties = GetShadowProperties(shadowCount++);

				for (var j = SgtHelper.tempMaterials.Count - 1; j >= 0; j--)
				{
					var tempMaterial = SgtHelper.tempMaterials[j];

					if (tempMaterial != null)
					{
						tempMaterial.SetTexture(properties.Texture, shadow.cachedTexture);
						tempMaterial.SetMatrix(properties.Matrix, shadow.cachedMatrix);
						tempMaterial.SetFloat(properties.Ratio, shadow.cachedRatio);
					}
				}

				if (shadowCount >= maxShadows)
				{
					break;
				}
			}

			for (var i = 0; i <= maxShadows; i++)
			{
				var keyword = GetShadowKeyword(i);

				if (lit == true && i == shadowCount)
				{
					SgtHelper.EnableKeyword(keyword);
				}
				else
				{
					SgtHelper.DisableKeyword(keyword);
				}
			}
		}

		protected virtual void Update()
		{
			calculatedThisFrame = false;
		}
	}
}