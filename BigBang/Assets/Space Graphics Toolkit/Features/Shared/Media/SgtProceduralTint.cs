using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtProceduralTint))]
	public class SgtProceduralTint_Editor : SgtProcedural_Editor<SgtProceduralTint>
	{
		protected override void OnInspector()
		{
			base.OnInspector();

			DrawDefault("Colors", "A color will be randomly picked from this gradient.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to procedurally generate the SpriteRenderer.color setting.</summary>
	[RequireComponent(typeof(SpriteRenderer))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtProceduralTint")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Procedural Tint")]
	public class SgtProceduralTint : SgtProcedural
	{
		/// <summary>A color will be randomly picked from this gradient.</summary>
		public Gradient Colors;

		protected override void DoGenerate()
		{
			var spriteRenderer = GetComponent<SpriteRenderer>();

			spriteRenderer.color = Colors.Evaluate(Random.value);
		}
	}
}