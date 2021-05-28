using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtSharedMaterial))]
	public class SgtSharedMaterial_Editor : SgtEditor<SgtSharedMaterial>
	{
		protected override void OnInspector()
		{
			BeginDisabled();
				DrawDefault("material", "The material that will be applied to all renderers.");
			EndDisabled();

			Each(t => t.RemoveMaterial());
			DrawDefault("renderers", "The renderers the Material will be applied to.");
			Each(t => { if (SgtHelper.Enabled(t) == true) { t.ApplyMaterial(); } });
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component stores a Material and a list of Renderers, and maintains that all renderers use the material as long as they are all part of this component.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtSharedMaterial")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shared Material")]
	public class SgtSharedMaterial : MonoBehaviour
	{
		/// <summary>The material that will be applied to all renderers.</summary>
		public Material Material
		{
			set
			{
				if (material != value)
				{
					if (material != null)
					{
						RemoveMaterial();
					}

					material = value;

					if (material != null)
					{
						ApplyMaterial();
					}
				}
			}

			get
			{
				return material;
			}
		}

		public int RendererCount
		{
			get
			{
				if (renderers != null)
				{
					return renderers.Count;
				}

				return 0;
			}
		}

		[SerializeField]
		private Material material;

		[SerializeField]
		private List<Renderer> renderers;

		public void ApplyMaterial()
		{
			if (renderers != null)
			{
				for (var i = renderers.Count - 1; i >= 0; i--)
				{
					SgtHelper.AddMaterial(renderers[i], material);
				}
			}
		}

		public void RemoveMaterial()
		{
			if (renderers != null)
			{
				for (var i = renderers.Count - 1; i >= 0; i--)
				{
					SgtHelper.RemoveMaterial(renderers[i], material);
				}
			}
		}

		public void AddRenderer(Renderer renderer)
		{
			if (renderer != null)
			{
				if (renderers == null)
				{
					renderers = new List<Renderer>();
				}

				if (renderers.Contains(renderer) == false)
				{
					renderers.Add(renderer);

					if (material != null)
					{
						SgtHelper.AddMaterial(renderer, material);
					}
				}
			}
		}

		public void RemoveRenderer(Renderer renderer)
		{
			if (renderers != null && renderer != null)
			{
				if (renderers.Remove(renderer) == true)
				{
					SgtHelper.RemoveMaterial(renderer, material);
				}
			}
		}

		[ContextMenu("Remove Null Renderers")]
		public void RemoveNullRenderers()
		{
			if (renderers != null)
			{
				for (var i = renderers.Count - 1; i >= 0; i--)
				{
					if (renderers[i] == null)
					{
						renderers.RemoveAt(i);
					}
				}
			}
		}

		protected virtual void OnEnable()
		{
			ApplyMaterial();
		}

		protected virtual void OnDisable()
		{
			RemoveMaterial();
		}
	}
}