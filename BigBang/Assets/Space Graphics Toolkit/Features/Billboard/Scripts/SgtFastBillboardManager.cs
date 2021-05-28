using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFastBillboardManager))]
	public class SgtFastBillboardManager_Editor : SgtEditor<SgtFastBillboardManager>
	{
		[InitializeOnLoad]
		public class ExecutionOrder
		{
			static ExecutionOrder()
			{
				ForceExecutionOrder(-100);
			}
		}

		protected override void OnInspector()
		{
			EditorGUILayout.HelpBox("This component marks where all spawned SgtFloatingObjects will be attached to.", MessageType.Info);
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>All SgtFastBillboards will be updated from here.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFastBillboardManager")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Fast Billboard Manager")]
	public class SgtFastBillboardManager : SgtLinkedBehaviour<SgtFastBillboardManager>
	{
		protected override void OnEnable()
		{
			if (InstanceCount > 0)
			{
				Debug.LogWarning("Your scene already contains an instance of SgtFastBillboardManager!", FirstInstance);
			}

			base.OnEnable();

			Camera.onPreCull += PreCull;
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			Camera.onPreCull -= PreCull;
		}

		private void PreCull(Camera camera)
		{
			if (this == FirstInstance)
			{
				var cameraRotation = camera.transform.rotation;
				var rollRotation   = cameraRotation;
				var observer       = default(SgtCamera);

				if (SgtCamera.TryFind(camera, ref observer) == true)
				{
					rollRotation *= observer.RollQuaternion;
				}

				var billboard = SgtFastBillboard.FirstInstance;
				var mask      = camera.cullingMask;
				var position  = camera.transform.position;

				for (var i = 0; i < SgtFastBillboard.InstanceCount; i++)
				{
					if ((billboard.Mask & mask) != 0)
					{
						var rotation = default(Quaternion);

						if (billboard.RollWithCamera == true)
						{
							rotation = rollRotation * billboard.Rotation;
						}
						else
						{
							rotation = cameraRotation * billboard.Rotation;
						}

						if (billboard.AvoidClipping == true)
						{
							var directionA = Vector3.Normalize(billboard.transform.position - position);
							var directionB = rotation * Vector3.forward;
							var theta      = Vector3.Angle(directionA, directionB);
							var axis       = Vector3.Cross(directionA, directionB);

							rotation = Quaternion.AngleAxis(theta, -axis) * rotation;
						}

						billboard.cachedTransform.rotation = rotation;
					}

					billboard = billboard.NextInstance;
				}
			}
		}
	}
}