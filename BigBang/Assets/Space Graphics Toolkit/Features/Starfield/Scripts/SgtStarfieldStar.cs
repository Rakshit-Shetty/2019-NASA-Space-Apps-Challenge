using UnityEngine;

namespace SpaceGraphicsToolkit
{
	[System.Serializable]
	public class SgtStarfieldStar
	{
		// Temp instance used when generating the starfield
		public static SgtStarfieldStar Temp = new SgtStarfieldStar();

		/// <summary>The coordinate index in the asteroid texture.</summary>
		[Tooltip("The coordinate index in the asteroid texture.")]
		public int Variant;

		/// <summary>Color tint of this star.</summary>
		[Tooltip("Color tint of this star.")]
		public Color Color = Color.white;

		/// <summary>Radius of this star in local space.</summary>
		[Tooltip("Radius of this star in local space.")]
		public float Radius;

		/// <summary>Angle in degrees.</summary>
		[Tooltip("Angle in degrees.")]
		public float Angle;

		/// <summary>Local position of this star relative to the starfield.</summary>
		[Tooltip("Local position of this star relative to the starfield.")]
		public Vector3 Position;

		/// <summary>How fast this star pulses (requires AllowPulse).</summary>
		[Tooltip("How fast this star pulses (requires AllowPulse).")]
		[Range(0.0f, 1.0f)]
		public float PulseSpeed = 1.0f;

		/// <summary>How much this star can pulse in size (requires AllowPulse).</summary>
		[Tooltip("How much this star can pulse in size (requires AllowPulse).")]
		[Range(0.0f, 1.0f)]
		public float PulseRange;

		/// <summary>The original pulse offset (requires AllowPulse).</summary>
		[Tooltip("The original pulse offset (requires AllowPulse).")]
		[Range(0.0f, 1.0f)]
		public float PulseOffset;

		public void CopyFrom(SgtStarfieldStar other)
		{
			Variant     = other.Variant;
			Color       = other.Color;
			Radius      = other.Radius;
			Angle       = other.Angle;
			Position    = other.Position;
			PulseSpeed  = other.PulseSpeed;
			PulseRange  = other.PulseRange;
			PulseOffset = other.PulseOffset;
		}
	}
}