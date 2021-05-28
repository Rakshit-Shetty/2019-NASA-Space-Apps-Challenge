using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFollow))]
	public class SgtFollow_Editor : SgtEditor<SgtFollow>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Target == null));
				DrawDefault("Target", "The transform that will be followed.");
			EndError();
			DrawDefault("Dampening", "");
			DrawDefault("Rotate", "");
			DrawDefault("FollowIn", "");

			Separator();

			DrawDefault("LocalPosition", "");
			DrawDefault("LocalRotation", "");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This makes the current Transform follow the target Transform as if it were a child.</summary>
	public class SgtFollow : MonoBehaviour
	{
		public enum UpdateType
		{
			Update,
			LateUpdate
		}

		/// <summary>The transform that will be followed.</summary>
		public Transform Target;

		public float Dampening = 5.0f;

		public bool Rotate = true;

		public UpdateType FollowIn;

		public Vector3 LocalPosition;

		public Vector3 LocalRotation;

		[ContextMenu("UpdatePosition")]
		public void UpdatePosition()
		{
			if (Target != null)
			{
				var targetPosition = Target.TransformPoint(LocalPosition);
				var factor         = SgtHelper.DampenFactor(Dampening, Time.deltaTime);

				transform.position = Vector3.Lerp(transform.position, targetPosition, factor);

				if (Rotate == true)
				{
					var targetRotation = Target.rotation * Quaternion.Euler(LocalRotation);

					transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, factor);
				}
			}
		}

		protected virtual void Update()
		{
			if (FollowIn == UpdateType.Update)
			{
				UpdatePosition();
			}
		}

		protected virtual void LateUpdate()
		{
			if (FollowIn == UpdateType.LateUpdate)
			{
				UpdatePosition();
			}
		}
	}
}