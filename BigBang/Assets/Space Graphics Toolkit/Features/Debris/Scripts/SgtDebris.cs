using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtDebris))]
	public class SgtDebris_Editor : SgtEditor<SgtDebris>
	{
		protected override void OnInspector()
		{
			DrawDefault("Pool", "Can this debris be pooled?");

			Separator();

			BeginDisabled();
				DrawDefault("State", "The current state of the scaling.");
				DrawDefault("Prefab", "The prefab this was instantiated from.");
				DrawDefault("Scale", "This gets automatically copied when spawning debris.");
				DrawDefault("Cell", "The cell this debris was spawned in.");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component handles a single debris object.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtDebris")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Debris")]
	public class SgtDebris : MonoBehaviour
	{
		public enum StateType
		{
			Hide,
			Fade,
			Show,
		}

		/// <summary>Called when this debris is spawned (if pooling is enabled).</summary>
		public System.Action OnSpawn;

		/// <summary>Called when this debris is despawned (if pooling is enabled).</summary>
		public System.Action OnDespawn;

		/// <summary>Can this debris be pooled?</summary>
		public bool Pool;

		/// <summary>The current state of the scaling.</summary>
		public StateType State;

		/// <summary>The prefab this was instantiated from.</summary>
		public SgtDebris Prefab;

		/// <summary>This gets automatically copied when spawning debris.</summary>
		public Vector3 Scale;

		/// <summary>The cell this debris was spawned in.</summary>
		public SgtVector3L Cell;

		// The initial scale-in
		public float Show;
	}
}