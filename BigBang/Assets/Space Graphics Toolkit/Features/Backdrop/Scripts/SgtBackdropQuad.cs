using UnityEngine;

namespace SpaceGraphicsToolkit
{
	[System.Serializable]
	public class SgtBackdropQuad
	{
		/// <summary>Temp instance used when generating the starfield</summary>
		public static SgtBackdropQuad Temp = new SgtBackdropQuad();

		/// <summary>The coordinate index in the asteroid texture.</summary>
		[Tooltip("The coordinate index in the asteroid texture.")]
		public int Variant;

		/// <summary>Color tint of this star.</summary>
		[Tooltip("Color tint of this star.")]
		public Color Color = Color.white;

		/// <summary>Radius of this star in local space.</summary>
		[Tooltip("Radius of this star in local space.")]
		public float Radius;

		/// <summary>Angle of this star in degrees.</summary>
		[Tooltip("Angle of this star in degrees.")]
		public float Angle;

		/// <summary>Position of the star in local space.</summary>
		[Tooltip("Position of the star in local space.")]
		public Vector3 Position;

		public void CopyFrom(SgtBackdropQuad other)
		{
			Variant  = other.Variant;
			Color    = other.Color;
			Radius   = other.Radius;
			Angle    = other.Angle;
			Position = other.Position;
		}
	}
}