using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtRing))]
	public class SgtRing_Editor : SgtEditor<SgtRing>
	{
		protected override void OnInspector()
		{
			var updateMaterial = false;
			var updateModels   = false;

			DrawDefault("Color", ref updateMaterial, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness < 0.0f));
				DrawDefault("Brightness", ref updateMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the ring material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(t => t.MainTex == null));
				DrawDefault("MainTex", ref updateMaterial, "The texture applied to the ring, where the left side is the inside, and the right side is the outside.");
			EndError();

			BeginError(Any(t => t.Segments < 1));
				DrawDefault("Segments", ref updateModels, ref updateModels, "This allows you to set how many copies of the Mesh are required to complete the ring. For example, if the Mesh is 1/4 of the ring, then Segments should be set to 4.");
			EndError();
			BeginError(Any(t => t.Mesh == null));
				DrawDefault("Mesh", ref updateModels, "This allows you to set the mesh used to render the ring.");
			EndError();

			Separator();

			DrawDefault("Detail", ref updateMaterial, "Should the ring have a detail texture? For example, dust noise when you get close.");

			if (Any(t => t.Detail == true))
			{
				BeginIndent();
					BeginError(Any(t => t.DetailTex == null));
						DrawDefault("DetailTex", ref updateMaterial, "This allows you to set the detail texture that gets repeated on the ring surface.");
					EndError();
					BeginError(Any(t => t.DetailScaleX < 0.0f));
						DrawDefault("DetailScaleX", ref updateMaterial, "The detail texture horizontal tiling.");
					EndError();
					BeginError(Any(t => t.DetailScaleY < 1));
						DrawDefault("DetailScaleY", ref updateMaterial, "The detail texture vertical tiling.");
					EndError();
					DrawDefault("DetailOffset", ref updateMaterial, "The UV offset of the detail texture.");
					DrawDefault("DetailSpeed", ref updateMaterial, "The scroll speed of the detail texture UV offset.");
					DrawDefault("DetailTwist", ref updateMaterial, "The amount the detail texture is twisted around the ring.");
					BeginError(Any(t => t.DetailTwistBias < 1.0f));
						DrawDefault("DetailTwistBias", ref updateMaterial, "The amount the twisting is pushed to the outer edge.");
					EndError();
				EndIndent();
			}

			Separator();

			DrawDefault("Near", ref updateMaterial, "Enable this if you want the ring to fade out as the camera approaches.");

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

			Separator();

			DrawDefault("Lit", ref updateMaterial, "If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.");

			if (Any(t => t.Lit == true))
			{
				if (SgtLight.InstanceCount == 0)
				{
					EditorGUILayout.HelpBox("You need to add the SgtLight component to your scene lights for them to work with SGT.", MessageType.Warning);
				}

				BeginIndent();
					BeginError(Any(t => t.LightingTex == null));
						DrawDefault("LightingTex", ref updateMaterial, "The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
					EndError();
					DrawDefault("Scattering", ref updateMaterial, "If you enable this then light will scatter through the ring atmosphere. This means light entering the eye will come from all angles, especially around the light point.");

					if (Any(t => t.Scattering == true))
					{
						BeginIndent();
							BeginError(Any(t => t.ScatteringMie <= 0.0f));
								DrawDefault("ScatteringMie", ref updateMaterial, "The mie scattering term, allowing you to adjust the distribution of front scattered light.");
							EndError();
							DrawDefault("ScatteringStrength", "The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect."); // Updated in LateUpdate
						EndIndent();
					}
				EndIndent();
			}

			if (Any(t => t.Mesh == null && t.GetComponent<SgtRingMesh>() == null))
			{
				Separator();

				if (Button("Add Mesh") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtRingMesh>(t.gameObject));
				}
			}

			if (Any(t => t.Near == true && t.NearTex == null && t.GetComponent<SgtRingNearTex>() == null))
			{
				Separator();

				if (Button("Add NearTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtRingNearTex>(t.gameObject));
				}
			}

			if (Any(t => t.Lit == true && t.LightingTex == null && t.GetComponent<SgtRingLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtRingLightingTex>(t.gameObject));
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
	/// <summary>This component allows you to render a planetary ring. This ring can be split into multiple segments to improve depth sorting.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtRing")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Ring")]
	public class SgtRing : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the ring material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>The texture applied to the ring, where the left side is the inside, and the right side is the outside.</summary>
		public Texture MainTex;

		/// <summary>This allows you to set the mesh used to render the ring.</summary>
		public Mesh Mesh;

		/// <summary>This allows you to set how many copies of the Mesh are required to complete the ring. For example, if the Mesh is 1/4 of the ring, then Segments should be set to 4.</summary>
		public int Segments = 8;

		/// <summary>Should the ring have a detail texture? For example, dust noise when you get close.</summary>
		public bool Detail;

		/// <summary>This allows you to set the detail texture that gets repeated on the ring surface.</summary>
		public Texture DetailTex;

		/// <summary>The detail texture horizontal tiling.</summary>
		public float DetailScaleX = 1.0f;

		/// <summary>The detail texture vertical tiling.</summary>
		public int DetailScaleY = 1;

		/// <summary>The UV offset of the detail texture.</summary>
		public Vector2 DetailOffset;

		/// <summary>The scroll speed of the detail texture UV offset.</summary>
		public Vector2 DetailSpeed;

		/// <summary>The amount the detail texture is twisted around the ring.</summary>
		public float DetailTwist;

		/// <summary>The amount the twisting is pushed to the outer edge.</summary>
		public float DetailTwistBias = 1.0f;

		/// <summary>Enable this if you want the ring to fade out as the camera approaches.</summary>
		public bool Near;

		/// <summary>The lookup table used to calculate the fade opacity based on distance, where the left side is used when the camera is close, and the right side is used when the camera is far.</summary>
		public Texture NearTex;

		/// <summary>The distance the fading begins from in world space.</summary>
		public float NearDistance = 1.0f;

		/// <summary>If you enable this then light will scatter through the ring atmosphere. This means light entering the eye will come from all angles, especially around the light point.</summary>
		public bool Scattering;

		/// <summary>The mie scattering term, allowing you to adjust the distribution of front scattered light.</summary>
		public float ScatteringMie = 8.0f;

		/// <summary>The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect.</summary>
		public float ScatteringStrength = 25.0f;

		/// <summary>If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.</summary>
		public bool Lit;

		/// <summary>The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture LightingTex;

		// The models used to render the full ring
		[SerializeField]
		private List<SgtRingModel> models;

		// The material applied to all models
		[System.NonSerialized]
		private Material material;

		/// <summary>This is used to optimize shader calculations.</summary>
		[System.NonSerialized]
		private bool renderedThisFrame;

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

		public virtual void UpdateLightingTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._LightingTex, LightingTex);
			}
		}

		[ContextMenu("Update Material")]
		public virtual void UpdateMaterial()
		{
			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Ring (Generated)", SgtHelper.ShaderNamePrefix + "Ring");

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

			var color = SgtHelper.Brighten(Color, Brightness);

			material.renderQueue = RenderQueue;

			material.SetColor(SgtShader._Color, color);
			material.SetTexture(SgtShader._MainTex, MainTex);

			if (Detail == true)
			{
				SgtHelper.EnableKeyword("SGT_B", material); // Detail

				material.SetTexture(SgtShader._DetailTex, DetailTex);
				material.SetVector(SgtShader._DetailOffset, DetailOffset);
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

			if (Lit == true)
			{
				material.SetTexture(SgtShader._LightingTex, LightingTex);
			}

			if (Scattering == true)
			{
				SgtHelper.EnableKeyword("SGT_A", material); // Scattering

				material.SetFloat(SgtShader._ScatteringMie, ScatteringMie * ScatteringMie);
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_A", material); // Scattering
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
				var model    = GetOrAddModel(i);
				var angle    = angleStep * i;
				var rotation = Quaternion.Euler(0.0f, angle, 0.0f);

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
					SgtRingModel.Pool(models[i]);

					models.RemoveAt(i);
				}
			}
		}

		public static SgtRing Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtRing Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Ring", layer, parent, localPosition, localRotation, localScale);
			var ring       = gameObject.AddComponent<SgtRing>();

			return ring;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Ring", false, 10)]
		public static void CreateMenuItem()
		{
			var parent = SgtHelper.GetSelectedParent();
			var ring   = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(ring);
		}
#endif

		protected virtual void OnEnable()
		{
			Camera.onPreRender  += CameraPreRender;
			Camera.onPostRender += CameraPostRender;

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
			// Update scrolling?
			if (Detail == true)
			{
				if (Application.isPlaying == true)
				{
					DetailOffset += DetailSpeed * Time.deltaTime;
				}

				if (material != null)
				{
					material.SetVector(SgtShader._DetailOffset, DetailOffset);
				}
			}
		}

		protected virtual void OnDisable()
		{
			Camera.onPreRender  -= CameraPreRender;
			Camera.onPostRender -= CameraPostRender;

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
					SgtRingModel.MarkForDestruction(models[i]);
				}
			}

			SgtHelper.Destroy(material);
		}

		private void CameraPreRender(Camera camera)
		{
			// Write these once to save CPU
			if (renderedThisFrame == false && material != null)
			{
				renderedThisFrame = true;

				// Write lights and shadows
				SgtHelper.SetTempMaterial(material);

				var mask   = 1 << gameObject.layer;
				var lights = SgtLight.Find(Lit, mask);

				SgtShadow.Find(Lit, mask, lights);
				SgtShadow.FilterOutRing(transform.position);
				SgtShadow.Write(Lit, mask, 2);

				SgtLight.FilterOut(transform.position);
				SgtLight.Write(Lit, transform.position, null, null, ScatteringStrength, 2);
			}
		}

		private void CameraPostRender(Camera camera)
		{
			renderedThisFrame = false;
		}

		private SgtRingModel GetOrAddModel(int index)
		{
			var model = default(SgtRingModel);

			if (models == null)
			{
				models = new List<SgtRingModel>();
			}

			if (index < models.Count)
			{
				model = models[index];

				if (model == null)
				{
					model = SgtRingModel.Create(this);

					models[index] = model;
				}
			}
			else
			{
				model = SgtRingModel.Create(this);

				models.Add(model);
			}

			return model;
		}
	}
}