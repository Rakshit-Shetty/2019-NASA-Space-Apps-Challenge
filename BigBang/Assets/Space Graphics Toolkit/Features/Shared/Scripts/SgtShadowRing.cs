using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtShadowRing))]
	public class SgtShadowRing_Editor : SgtEditor<SgtShadowRing>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Texture == null));
				DrawDefault("Texture", "The texture of the shadow (left = inside, right = outside).");
			EndError();
			BeginError(Any(t => t.RadiusMin < 0.0f || t.RadiusMin >= t.RadiusMax));
				DrawDefault("RadiusMin", "The inner radius of the ring casting this shadow (auto set if Ring is set).");
			EndError();
			BeginError(Any(t => t.RadiusMax < 0.0f || t.RadiusMin >= t.RadiusMax));
				DrawDefault("RadiusMax", "The outer radius of the ring casting this shadow (auto set if Ring is set).");
			EndError();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to cast a ring shadow from the current GameObject.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtShadowRing")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shadow Ring")]
	public class SgtShadowRing : SgtShadow
	{
		/// <summary>The texture of the shadow (left = inside, right = outside).</summary>
		public Texture Texture;

		/// <summary>The inner radius of the ring casting this shadow (auto set if Ring is set).</summary>
		public float RadiusMin = 1.0f;

		/// <summary>The outer radius of the ring casting this shadow (auto set if Ring is set).</summary>
		public float RadiusMax = 2.0f;

		public override Texture GetTexture()
		{
			return Texture;
		}

		public override void CalculateShadow(SgtLight light)
		{
			if (Texture != null)
			{
				var direction = default(Vector3);
				var position  = default(Vector3);
				var color     = default(Color);

				SgtLight.Calculate(light, transform.position, null, null, ref position, ref direction, ref color);

				var rotation = Quaternion.FromToRotation(direction, Vector3.back);
				var squash   = Vector3.Dot(direction, transform.up); // Find how squashed the ellipse is based on light direction
				var width    = transform.lossyScale.x * RadiusMax;
				var length   = transform.lossyScale.z * RadiusMax;
				var axis     = rotation * transform.up; // Find the transformed up axis
				var spin     = Quaternion.LookRotation(Vector3.forward, new Vector2(-axis.x, axis.y)); // Orient the shadow ellipse
				var scale    = SgtHelper.Reciprocal3(new Vector3(width, length * Mathf.Abs(squash), 1.0f));
				var skew     = Mathf.Tan(SgtHelper.Acos(-squash));

				var shadowT = Matrix4x4.Translate(-transform.position);
				var shadowR = Matrix4x4.Rotate(spin * rotation); // Spin the shadow so lines up with its tilt
				var shadowS = Matrix4x4.Scale(scale); // Scale the ring into an oval
				var shadowK = SgtHelper.ShearingZ(new Vector2(0.0f, skew)); // Skew the shadow so it aligns with the ring plane

				cachedActive  = true;
				cachedMatrix  = shadowS * shadowK * shadowR * shadowT;
				cachedRatio   = SgtHelper.Divide(RadiusMax, RadiusMax - RadiusMin);
				cachedRadius  = SgtHelper.UniformScale(transform.lossyScale) * RadiusMax;
				cachedTexture = Texture;
			}
			else
			{
				cachedActive = false;
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			var mask   = 1 << gameObject.layer;
			var lights = SgtLight.Find(true, mask);

			if (SgtHelper.Enabled(this) == true && lights.Count > 0)
			{
				CalculateShadow(lights[0]);

				if (cachedActive == true)
				{
					Gizmos.matrix = cachedMatrix.inverse;

					var distA = 0.0f;
					var distB = 1.0f;
					var scale = 1.0f * Mathf.Deg2Rad;
					var inner = SgtHelper.Divide(RadiusMin, RadiusMax);

					for (var i = 1; i < 10; i++)
					{
						var posA  = new Vector3(0.0f, 0.0f, distA);
						var posB  = new Vector3(0.0f, 0.0f, distB);

						Gizmos.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Pow(0.75f, i) * 0.125f);

						for (var a = 1; a <= 360; a++)
						{
							posA.x = posB.x = Mathf.Sin(a * scale);
							posA.y = posB.y = Mathf.Cos(a * scale);

							Gizmos.DrawLine(posA, posB);

							posA.x = posB.x = posA.x * inner;
							posA.y = posB.y = posA.y * inner;

							Gizmos.DrawLine(posA, posB);
						}

						distA = distB;
						distB = distB * 2.0f;
					}
				}
			}
		}
#endif
	}
}