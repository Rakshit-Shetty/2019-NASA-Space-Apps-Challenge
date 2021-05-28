using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtSpacetimeWell))]
	public class SgtSpacetimeWell_Editor : SgtEditor<SgtSpacetimeWell>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Radius < 0.0f));
				DrawDefault("Radius", "The radius of this spacetime well.");
			EndError();
			DrawDefault("Strength", "The minimum strength of the well.");

			Separator();

			DrawDefault("Distribution", "The method used to deform the spacetime.");
			BeginIndent();
				if (Any(t => t.Distribution == SgtSpacetimeWell.DistributionType.Ripple || t.Distribution == SgtSpacetimeWell.DistributionType.Twist))
				{
					DrawDefault("Frequency", "The frequency of the ripple.");
				}

				if (Any(t => t.Distribution == SgtSpacetimeWell.DistributionType.Ripple))
				{
					DrawDefault("Offset", "The frequency offset.");
					DrawDefault("OffsetSpeed", "The frequency offset speed per second.");
				}

				if (Any(t => t.Distribution == SgtSpacetimeWell.DistributionType.Twist))
				{
					BeginError(Any(t => t.HoleSize < 0.0f));
						DrawDefault("HoleSize", "The size of the twist hole.");
					EndError();
					DrawDefault("HolePower", "The power of the twist hole.");
				}
			EndIndent();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to deform SgtSpacetime grids.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtSpacetimeWell")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Spacetime Well")]
	public class SgtSpacetimeWell : SgtLinkedBehaviour<SgtSpacetimeWell>
	{
		public enum DistributionType
		{
			Gaussian,
			Ripple,
			Twist
		}

		/// <summary>The method used to deform the spacetime.</summary>
		public DistributionType Distribution = DistributionType.Gaussian;

		/// <summary>The radius of this spacetime well.</summary>
		public float Radius = 1.0f;

		/// <summary>The frequency of the ripple.</summary>
		public float Frequency = 1.0f;

		/// <summary>The minimum strength of the well.</summary>
		public float Strength = 1.0f;

		/// <summary>The frequency offset.</summary>
		public float Offset;

		/// <summary>The frequency offset speed per second.</summary>
		public float OffsetSpeed;

		/// <summary>The size of the twist hole.</summary>
		[Range(0.0f, 0.9f)]
		public float HoleSize;

		/// <summary>The power of the twist hole.</summary>
		public float HolePower = 10.0f;

		public static SgtSpacetimeWell Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtSpacetimeWell Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject    = SgtHelper.CreateGameObject("Spacetime Well", layer, parent, localPosition, localRotation, localScale);
			var spacetimeWell = gameObject.AddComponent<SgtSpacetimeWell>();

			return spacetimeWell;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Spacetime Well", false, 10)]
		public static void CreateItem()
		{
			var parent        = SgtHelper.GetSelectedParent();
			var spacetimeWell = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(spacetimeWell);
		}
#endif

		protected virtual void Update()
		{
#if UNITY_EDITOR
		if (Application.isPlaying == false)
		{
			return;
		}
#endif
			Offset += OffsetSpeed * Time.deltaTime;
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(transform.position, Radius);
		}
#endif
	}
}