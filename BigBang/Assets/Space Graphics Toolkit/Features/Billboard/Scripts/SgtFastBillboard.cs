using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFastBillboard))]
	public class SgtFastBillboard_Editor : SgtEditor<SgtFastBillboard>
	{
		protected override void OnInspector()
		{
			DrawDefault("RollWithCamera", "If the camera rolls, should this billboard roll with it?");
			DrawDefault("AvoidClipping", "If your billboard is clipping out of view at extreme angles, then enable this.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component rotates the current Gameobject to the rendering camera.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFastBillboard")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Fast Billboard")]
	public class SgtFastBillboard : SgtLinkedBehaviour<SgtFastBillboard>
	{
		/// <summary>If the camera rolls, should this billboard roll with it?</summary>
		public bool RollWithCamera;

		/// <summary>If your billboard is clipping out of view at extreme angles, then enable this.</summary>
		public bool AvoidClipping;

		[HideInInspector]
		public Quaternion Rotation = Quaternion.identity;

		[System.NonSerialized]
		public int Mask;

		[System.NonSerialized]
		public Transform cachedTransform;

		public void RandomlyRotate(int seed)
		{
			Rotation = Quaternion.Euler(0.0f, 0.0f, Random.value * 360.0f);
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			Mask = 1 << gameObject.layer;

			cachedTransform = GetComponent<Transform>();
		}
	}
}