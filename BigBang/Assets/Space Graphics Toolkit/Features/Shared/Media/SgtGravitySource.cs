using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtGravitySource))]
	public class SgtGravitySource_Editor : SgtEditor<SgtGravitySource>
	{
		protected override void OnInspector()
		{
			DrawDefault("Mass", "The mass of this gravity source.");
			DrawDefault("AutoSetMass", "If you enable this then the Mass setting will be automatically copied from the attached Rigidbody.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to define a gravity source, which can be used to attract Rigidbodyies with the SgtGravityReceiver attached.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtGravitySource")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Gravity Source")]
	public class SgtGravitySource : SgtLinkedBehaviour<SgtGravitySource>
	{
		/// <summary>The mass of this gravity source.</summary>
		public float Mass = 100.0f;

		/// <summary>If you enable this then the Mass setting will be automatically copied from the attached Rigidbody.</summary>
		public bool AutoSetMass;

		[System.NonSerialized]
		private Rigidbody cachedRigidbody;

		protected virtual void Update()
		{
			if (AutoSetMass == true)
			{
				if (cachedRigidbody == null)
				{
					cachedRigidbody = GetComponent<Rigidbody>();
				}

				if (cachedRigidbody != null)
				{
					Mass = cachedRigidbody.mass;
				}
			}
		}
	}
}