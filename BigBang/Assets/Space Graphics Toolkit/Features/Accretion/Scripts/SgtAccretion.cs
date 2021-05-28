using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAccretion))]
	public class SgtAccretion_Editor : SgtEditor<SgtAccretion>
	{
		protected override void OnInspector()
		{
			var updateMaterial = false;
			var updateModels   = false;

			DrawDefault("Color", ref updateMaterial, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness < 0.0f));
				DrawDefault("Brightness", ref updateMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the disc material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(t => t.MainTex == null));
				DrawDefault("MainTex", ref updateMaterial, "The texture applied to the accretion, where the left side is the inside, and the right side is the outside.");
			EndError();

			BeginError(Any(t => t.Segments < 1));
				DrawDefault("Segments", ref updateModels, ref updateModels, "This allows you to set how many copies of the Mesh are required to complete the disc. For example, if the Mesh is 1/4 of the ring, then Segments should be set to 4.");
			EndError();
			BeginError(Any(t => t.Mesh == null));
				DrawDefault("Mesh", ref updateModels, "This allows you to set the mesh used to render the disc.");
			EndError();

			Separator();

			DrawDefault("Detail", ref updateMaterial, "Should the disc have a detail texture? For example, dust noise when you get close.");

			if (Any(t => t.Detail == true))
			{
				BeginIndent();
					BeginError(Any(t => t.DetailTex == null));
						DrawDefault("DetailTex", ref updateMaterial, "This allows you to set the detail texture that gets repeated on the disc surface.");
					EndError();
					BeginError(Any(t => t.DetailScaleX < 0.0f));
						DrawDefault("DetailScaleX", ref updateMaterial, "The detail texture horizontal tiling.");
					EndError();
					BeginError(Any(t => t.DetailScaleY < 1));
						DrawDefault("DetailScaleY", ref updateMaterial, "The detail texture vertical tiling.");
					EndError();
					DrawDefault("DetailOffset", ref updateMaterial, "The UV offset of the detail texture.");
					DrawDefault("DetailSpeed", ref updateMaterial, "The scroll speed of the detail texture UV offset.");
					DrawDefault("DetailTwist", ref updateMaterial, "The amount the detail texture is twisted around the disc.");
					BeginError(Any(t => t.DetailTwistBias < 1.0f));
						DrawDefault("DetailTwistBias", ref updateMaterial, "The amount the twisting is pushed to the outer edge.");
					EndError();
				EndIndent();
			}

			Separator();

			DrawDefault("Near", ref updateMaterial, "Enable this if you want the disc to fade out as the camera approaches.");

			if (Any(t => t.Near == true))
			{
				BeginIndent();
					BeginError(Any(t => t.NearTex == null));
						DrawDefault("NearTex", ref updateMaterial, "The lookup table used to calculate the fade opacity based on distance, where the left side is used when the camera is close, and the right side is used when the camera is far.");
					EndError();
					BeginError(Any(t => t.NearDistance <= 0.0f));
						DrawDefault("NearDistance", ref updateMaterial, "The distance the fading begins from in world space.");
					EndError();
				EndIndent();
			}

			if (Any(t => t.Mesh == null && t.GetComponent<SgtAccretionMesh>() == null))
			{
				Separator();

				if (Button("Add Mesh") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtAccretionMesh>(t.gameObject));
				}
			}

			if (Any(t => t.Near == true && t.NearTex == null && t.GetComponent<SgtAccretionNearTex>() == null))
			{
				Separator();

				if (Button("Add NearTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtAccretionNearTex>(t.gameObject));
				}
			}

			if (updateMaterial == true) DirtyEach(t => t.UpdateMaterial());
			if (updateModels   == true) DirtyEach(t => t.UpdateModels  ());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render an accretion disc. This disc can be animated to spiral dust into the center. This disc can be split into multiple segments to improve depth sorting.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtAccretion")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Accretion")]
	public class SgtAccretion : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the disc material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>The texture applied to the disc, where the left side is the inside, and the right side is the outside.</summary>
		public Texture MainTex;

		/// <summary>This allows you to set the mesh used to render the disc.</summary>
		public Mesh Mesh;

		/// <summary>This allows you to set how many copies of the Mesh are required to complete the disc. For example, if the Mesh is 1/4 of the disc, then Segments should be set to 4.</summary>
		public int Segments = 8;

		/// <summary>Should the disc have a detail texture? For example, dust noise when you get close.</summary>
		public bool Detail;

		/// <summary>This allows you to set the detail texture that gets repeated on the disc surface.</summary>
		public Texture DetailTex;

		/// <summary>The detail texture horizontal tiling.</summary>
		public float DetailScaleX = 1.0f;

		/// <summary>The detail texture vertical tiling.</summary>
		public int DetailScaleY = 1;

		/// <summary>The UV offset of the detail texture.</summary>
		public Vector2 DetailOffset;

		/// <summary>The scroll speed of the detail texture UV offset.</summary>
		public Vector2 DetailSpeed = new Vector2(1.0f, 1.0f);

		/// <summary>The amount the detail texture is twisted around the disc.</summary>
		public float DetailTwist;

		/// <summary>The amount the twisting is pushed to the outer edge.</summary>
		public float DetailTwistBias = 1.0f;

		/// <summary>Enable this if you want the disc to fade out as the camera approaches.</summary>
		public bool Near;

		/// <summary>The lookup table used to calculate the fade opacity based on distance, where the left side is used when the camera is close, and the right side is used when the camera is far.</summary>
		public Texture NearTex;

		/// <summary>The world space distance the fade will begin from.</summary>
		public float NearDistance = 1.0f;

		/// <summary>Each model is used to render one segment of the disc.</summary>
		[SerializeField]
		private List<SgtAccretionModel> models;

		/// <summary>The material applied to each model.</summary>
		[System.NonSerialized]
		private Material material;

		public virtual void UpdateMainTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._MainTex, MainTex);
			}
		}

		public virtual void UpdateNearTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._NearTex, NearTex);
			}
		}

		[ContextMenu("Update Material")]
		public virtual void UpdateMaterial()
		{
			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Accretion (Generated)", SgtHelper.ShaderNamePrefix + "Accretion");

				if (models != null)
				{
					for (var i = models.Count - 1; i >= 0; i--)
					{
						var model = models[i];

						if (model != null)
						{
							model.SetMaterial(material);
						}
					}
				}
			}

			material.renderQueue = RenderQueue;

			material.SetColor(SgtShader._Color, SgtHelper.Brighten(Color, Brightness));
			material.SetTexture(SgtShader._MainTex, MainTex);

			if (Detail == true)
			{
				SgtHelper.EnableKeyword("SGT_B", material); // Detail

				material.SetTexture(SgtShader._DetailTex, DetailTex);
				material.SetVector(SgtShader._DetailScale, new Vector2(DetailScaleX, DetailScaleY));
				material.SetFloat(SgtShader._DetailTwist, DetailTwist);
				material.SetFloat(SgtShader._DetailTwistBias, DetailTwistBias);
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_B", material); // Detail
			}

			if (Near == true)
			{
				SgtHelper.EnableKeyword("SGT_C", material); // Near

				material.SetTexture(SgtShader._NearTex, NearTex);
				material.SetFloat(SgtShader._NearScale, SgtHelper.Reciprocal(NearDistance));
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_C", material); // Near
			}
		}

		[ContextMenu("Update Mesh")]
		public void UpdateMesh()
		{
			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var model = models[i];

					if (model != null)
					{
						model.SetMesh(Mesh);
					}
				}
			}
		}

		[ContextMenu("Update Models")]
		public void UpdateModels()
		{
			var angleStep = SgtHelper.Divide(360.0f, Segments);

			for (var i = 0; i < Segments; i++)
			{
				var model    = SgtHelper.GetIndex(ref models, i);
				var angle    = angleStep * i;
				var rotation = Quaternion.Euler(0.0f, angle, 0.0f);

				if (model == null)
				{
					model = models[i] = SgtAccretionModel.Create(this);
				}

				model.SetMesh(Mesh);
				model.SetMaterial(material);
				model.SetRotation(rotation);
			}

			// Remove any excess
			if (models != null)
			{
				var min = Mathf.Max(0, Segments);

				for (var i = models.Count - 1; i >= min; i--)
				{
					SgtAccretionModel.Pool(models[i]);

					models.RemoveAt(i);
				}
			}
		}

		public static SgtAccretion Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtAccretion Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Accretion", layer, parent, localPosition, localRotation, localScale);
			var accretion  = gameObject.AddComponent<SgtAccretion>();

			return accretion;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Accretion", false, 10)]
		public static void CreateMenuItem()
		{
			var parent    = SgtHelper.GetSelectedParent();
			var accretion = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(accretion);
		}
#endif

		protected virtual void OnEnable()
		{
			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var model = models[i];

					if (model != null)
					{
						model.gameObject.SetActive(true);
					}
				}
			}

			UpdateMaterial();
			UpdateModels();
		}

		protected virtual void LateUpdate()
		{
			if (Detail == true && material != null)
			{
				if (Application.isPlaying == true)
				{
					DetailOffset += DetailSpeed * Time.deltaTime;
				}

				material.SetVector(SgtShader._DetailOffset, DetailOffset);
			}
		}

		protected virtual void OnDisable()
		{
			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var model = models[i];

					if (model != null)
					{
						model.gameObject.SetActive(false);
					}
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					SgtAccretionModel.MarkForDestruction(models[i]);
				}
			}

			SgtHelper.Destroy(material);
		}
	}
}