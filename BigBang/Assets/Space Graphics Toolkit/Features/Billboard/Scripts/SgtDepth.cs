using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	public abstract class SgtDepth_Editor<T> : SgtEditor<T>
		where T : SgtDepth
	{
		protected void DrawDepth()
		{
			DrawDefault("Layers", "The layers that will be sampled when calculating the optical depth."); // Updated automatically
			DrawDefault("Ease", "The transition style between 0..1 depth."); // Updated automatically
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This is the base class for depth calculations, allowing you to find how opaque the view is between two points.</summary>
	[ExecuteInEditMode]
	public abstract class SgtDepth : SgtLinkedBehaviour<SgtDepth>
	{
		/// <summary>The layers that will be sampled when calculating the optical depth.</summary>
		public LayerMask Layers = Physics.DefaultRaycastLayers;

		/// <summary>The transition style between 0..1 depth.</summary>
		public SgtEase.Type Ease = SgtEase.Type.Linear;

		// Prevent recursive depth calculation from Camera rendering
		private static bool busy;

		// Calculates the 0..1 depth between the eye and target
		public float Calculate(Vector3 eye, Vector3 target)
		{
			if (busy == true)
			{
				Debug.LogError("Calculate is being called recursively");

				return 0.0f;
			}

			var coverage = default(float);

			busy = true;
			{
				coverage = DoCalculate(eye, target);
			}
			busy = false;

			return 1.0f - SgtEase.Evaluate(Ease, 1.0f - coverage);
		}

		protected abstract float DoCalculate(Vector3 eye, Vector3 target);
	}
}