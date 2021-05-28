using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtShadowSphere))]
	public class SgtShadowSphere_Editor : SgtEditor<SgtShadowSphere>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;

			BeginError(Any(t => t.Width < 1));
				DrawDefault("Width", ref updateTexture, "The width of the generated texture. A higher value can result in a smoother transition.");
			EndError();
			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");
			DrawDefault("SharpnessR", ref updateTexture, "The sharpness of the sunset red channel transition.");
			DrawDefault("SharpnessG", ref updateTexture, "The sharpness of the sunset green channel transition.");
			DrawDefault("SharpnessB", ref updateTexture, "The sharpness of the sunset blue channel transition.");
			BeginError(Any(t => t.Opacity < 0.0f));
				DrawDefault("Opacity", ref updateTexture, "The opacity of the shadow.");
			EndError();
			BeginError(Any(t => t.RadiusMin < 0.0f || t.RadiusMin >= t.RadiusMax));
				DrawDefault("RadiusMin", "The inner radius of the sphere in local space.");
			EndError();
			BeginError(Any(t => t.RadiusMax < 0.0f || t.RadiusMin >= t.RadiusMax));
				DrawDefault("RadiusMax", "The outer radius of the sphere in local space.");
			EndError();

			if (updateTexture == true) DirtyEach(t => t.UpdateTexture());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to cast a sphere shadow from the current GameObject.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtShadowSphere")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shadow Sphere")]
	public class SgtShadowSphere : SgtShadow
	{
		/// <summary>The width of the generated texture. A higher value can result in a smoother transition.</summary>
		public int Width = 256;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The sharpness of the sunset red channel transition.</summary>
		public float SharpnessR = 1.0f;

		/// <summary>The power of the sunset green channel transition.</summary>
		public float SharpnessG = 1.0f;

		/// <summary>The power of the sunset blue channel transition.</summary>
		public float SharpnessB = 1.0f;

		/// <summary>The opacity of the shadow.</summary>
		[Range(0.0f, 1.0f)]
		public float Opacity = 1.0f;

		/// <summary>The inner radius of the sphere in local space.</summary>
		public float RadiusMin = 1.0f;

		/// <summary>The outer radius of the sphere in local space.</summary>
		public float RadiusMax = 1.1f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[SerializeField]
		[HideInInspector]
		private bool startCalled;

		public Texture2D GeneratedTexture
		{
			get
			{
				return generatedTexture;
			}
		}

		public override Texture GetTexture()
		{
			if (generatedTexture == false)
			{
				UpdateTexture();
			}

			return generatedTexture;
		}

		[ContextMenu("Update Textures")]
		public void UpdateTexture()
		{
			if (Width > 0)
			{
				// Destroy if invalid
				if (generatedTexture != null)
				{
					if (generatedTexture.width != Width || generatedTexture.height != 1 || generatedTexture.format != Format)
					{
						generatedTexture = SgtHelper.Destroy(generatedTexture);
					}
				}

				// Create?
				if (generatedTexture == null)
				{
					generatedTexture = SgtHelper.CreateTempTexture2D("Sphere Shadow (Generated)", Width, 1, Format);

					generatedTexture.wrapMode = TextureWrapMode.Clamp;
				}

				var color = Color.clear;
				var stepX = 1.0f / (Width - 1);

				for (var x = 0; x < Width; x++)
				{
					var u = x * stepX;

					WriteTexture(u, x);
				}
			
				generatedTexture.Apply();
			}
		}

		private void WriteTexture(float u, int x)
		{
			var color = default(Color);

			color.r = SgtHelper.Sharpness(u, SharpnessR) * Opacity + (1.0f - Opacity);
			color.g = SgtHelper.Sharpness(u, SharpnessG) * Opacity + (1.0f - Opacity);
			color.b = SgtHelper.Sharpness(u, SharpnessB) * Opacity + (1.0f - Opacity);
			color.a = 1.0f;

			generatedTexture.SetPixel(x, 0, color);
		}

		public override void CalculateShadow(SgtLight light)
		{
			var direction = default(Vector3);
			var position  = default(Vector3);
			var color     = default(Color);

			SgtLight.Calculate(light, transform.position, null, null, ref position, ref direction, ref color);

			var dot      = Vector3.Dot(direction, transform.up);
			var radiusXZ = (transform.lossyScale.x + transform.lossyScale.z) * 0.5f * RadiusMax;
			var radiusY  = transform.lossyScale.y * RadiusMax;
			var radius   = GetRadius(radiusY, radiusXZ, dot * Mathf.PI * 0.5f);
			var rotation = Quaternion.FromToRotation(direction, Vector3.back);
			var vector   = rotation * transform.up;
			var spin     = Quaternion.LookRotation(Vector3.forward, new Vector2(-vector.x, vector.y)); // Orient the shadow ellipse
			var scale    = SgtHelper.Reciprocal3(new Vector3(radiusXZ, radius, 1.0f));
			var shadowT  = Matrix4x4.Translate(-transform.position);
			var shadowR  = Matrix4x4.Rotate(spin * rotation);
			var shadowS  = Matrix4x4.Scale(scale);

			cachedActive  = true;
			cachedMatrix  = shadowS * shadowR * shadowT;
			cachedRatio   = SgtHelper.Divide(RadiusMax, RadiusMax - RadiusMin);
			cachedRadius  = SgtHelper.UniformScale(transform.lossyScale) * RadiusMax;
			cachedTexture = generatedTexture;
		}

		private float GetRadius(float a, float b, float theta)
		{
			var s = Mathf.Sin(theta);
			var c = Mathf.Cos(theta);
			var z = Mathf.Sqrt((a*a)*(s*s)+(b*b)*(c*c));

			if (z != 0.0f)
			{
				return (a * b) / z;
			}

			return a;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			if (startCalled == true)
			{
				CheckUpdateCalls();
			}
		}

		protected virtual void Start()
		{
			if (startCalled == false)
			{
				startCalled = true;

				CheckUpdateCalls();
			}
		}
	
		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(generatedTexture);
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			var mask   = 1 << gameObject.layer;
			var lights = SgtLight.Find(true, mask);

			if (SgtHelper.Enabled(this) == true && lights.Count > 0)
			{
				Gizmos.matrix = transform.localToWorldMatrix;

				Gizmos.DrawWireSphere(Vector3.zero, RadiusMin);
				Gizmos.DrawWireSphere(Vector3.zero, RadiusMax);

				CalculateShadow(lights[0]);

				if (cachedActive == true)
				{
					Gizmos.matrix = cachedMatrix.inverse;

					var distA = 0.0f;
					var distB = 1.0f;
					var scale = 1.0f * Mathf.Deg2Rad;

					for (var i = 0; i < 10; i++)
					{
						var posA  = new Vector3(0.0f, 0.0f, distA);
						var posB  = new Vector3(0.0f, 0.0f, distB);

						Gizmos.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Pow(0.75f, i) * 0.125f);

						for (var a = 1; a <= 360; a++)
						{
							posA.x = posB.x = Mathf.Sin(a * scale);
							posA.y = posB.y = Mathf.Cos(a * scale);

							Gizmos.DrawLine(posA, posB);
						}

						distA = distB;
						distB = distB * 2.0f;
					}
				}
			}
		}
#endif

		private void CheckUpdateCalls()
		{
			if (generatedTexture == null)
			{
				UpdateTexture();
			}
		}
	}
}