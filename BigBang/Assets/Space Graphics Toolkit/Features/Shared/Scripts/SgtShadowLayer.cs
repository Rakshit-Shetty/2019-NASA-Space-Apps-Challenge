using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtShadowLayer))]
	public class SgtShadowLayer_Editor : SgtEditor<SgtShadowLayer>
	{
		protected override void OnInspector()
		{
			DrawDefault("Radius", "The radius of this shadow receiver.");

			Separator();

			Each(t => { if (t.isActiveAndEnabled == true) t.RemoveMaterial(); });
				BeginError(Any(t => t.Renderers != null && t.Renderers.Exists(s => s == null)));
					DrawDefault("Renderers", "The renderers you want the shadows to be applied to.");
				EndError();
			Each(t => { if (t.isActiveAndEnabled == true) t.ApplyMaterial(); });
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to add shadows cast from an SgtShadow___ component to any opaque renderer in your scene.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtShadowLayer")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shadow Layer")]
	public class SgtShadowLayer : MonoBehaviour
	{
		/// <summary>The radius of this shadow receiver.</summary>
		public float Radius = 1.0f;

		/// <summary>The renderers you want the shadows to be applied to.</summary>
		public List<MeshRenderer> Renderers;

		// The material added to all spacetime renderers
		[System.NonSerialized]
		private Material material;

		[ContextMenu("Apply Material")]
		public void ApplyMaterial()
		{
			if (Renderers != null)
			{
				for (var i = Renderers.Count - 1; i >= 0; i--)
				{
					SgtHelper.AddMaterial(Renderers[i], material);
				}
			}
		}

		[ContextMenu("Remove Material")]
		public void RemoveMaterial()
		{
			if (Renderers != null)
			{
				for (var i = Renderers.Count - 1; i >= 0; i--)
				{
					SgtHelper.RemoveMaterial(Renderers[i], material);
				}
			}
		}

		public void AddRenderer(MeshRenderer renderer)
		{
			if (renderer != null)
			{
				if (Renderers == null)
				{
					Renderers = new List<MeshRenderer>();
				}

				if (Renderers.Contains(renderer) == false)
				{
					Renderers.Add(renderer);

					SgtHelper.AddMaterial(renderer, material);
				}
			}
		}

		public void RemoveRenderer(MeshRenderer renderer)
		{
			if (renderer != null && Renderers != null)
			{
				if (Renderers.Remove(renderer) == true)
				{
					SgtHelper.RemoveMaterial(renderer, material);
				}
			}
		}

		protected virtual void OnEnable()
		{
			Camera.onPreRender += CameraPreRender;

			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Shadow Layer (Generated)", SgtHelper.ShaderNamePrefix + "ShadowLayer");
			}

			if (Renderers == null)
			{
				AddRenderer(GetComponent<MeshRenderer>());
			}

			ApplyMaterial();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(transform.position, SgtHelper.UniformScale(transform.lossyScale) * Radius);
		}
#endif

		protected virtual void OnDisable()
		{
			Camera.onPreRender -= CameraPreRender;

			RemoveMaterial();
		}

		protected virtual void CameraPreRender(Camera camera)
		{
			if (material != null)
			{
				SgtHelper.SetTempMaterial(material);

				var mask   = 1 << gameObject.layer;
				var lights = SgtLight.Find(true, mask);

				SgtShadow.Find(true, mask, lights);
				SgtShadow.FilterOutSphere(transform.position);
				SgtShadow.FilterOutMiss(transform.position, SgtHelper.UniformScale(transform.lossyScale) * Radius);
				SgtShadow.Write(true, mask, 2);
			}
		}
	}
}