using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCloudsphere))]
	public class SgtCloudsphere_Editor : SgtEditor<SgtCloudsphere>
	{
		protected override void OnInspector()
		{
			var updateMaterial = false;
			var updateModel   = false;

			DrawDefault("Color", ref updateMaterial, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness <= 0.0f));
				DrawDefault("Brightness", ref updateMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the cloudsphere material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(t => t.MainTex == null));
				DrawDefault("MainTex", ref updateMaterial, "The cube map applied to the cloudsphere surface.");
			EndError();
			BeginError(Any(t => t.DepthTex == null));
				DrawDefault("DepthTex", ref updateMaterial, "The look up table associating optical depth with cloud color. The left side is used when the depth is thin (e.g. edge of the cloudsphere when looking from space). The right side is used when the depth is thick (e.g. center of the cloudsphere when looking from space).");
			EndError();
			BeginError(Any(t => t.Radius < 0.0f));
				DrawDefault("Radius", ref updateModel, "This allows you to set the radius of the cloudsphere in local space.");
			EndError();
			DrawDefault("CameraOffset", "This allows you to offset the camera distance in world space when rendering the cloudsphere, giving you fine control over the render order."); // Updated automatically

			Separator();

			DrawDefault("Softness", ref updateMaterial, "Should the stars fade out if they're intersecting solid geometry?");

			if (Any(t => t.Softness > 0.0f))
			{
				foreach (var camera in Camera.allCameras)
				{
					if (SgtHelper.Enabled(camera) == true && camera.depthTextureMode == DepthTextureMode.None)
					{
						if ((camera.cullingMask & (1 << Target.gameObject.layer)) != 0)
						{
							if (HelpButton("You have enabled soft particles, but the '" + camera.name + "' camera does not write depth textures.", MessageType.Error, "Fix", 50.0f) == true)
							{
								var dtm = SgtHelper.GetOrAddComponent<SgtDepthTextureMode>(camera.gameObject);

								dtm.DepthMode = DepthTextureMode.Depth;

								dtm.UpdateDepthMode();

								Selection.activeObject = dtm;
							}
						}
					}
				}
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
				EndIndent();
			}

			Separator();

			DrawDefault("Near", ref updateMaterial, "Enable this if you want the cloudsphere to fade out as the camera approaches.");

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

			DrawDefault("Detail", ref updateMaterial, "");

			if (Any(t => t.Detail == true))
			{
				BeginIndent();
					BeginError(Any(t => t.DetailTex == null));
						DrawDefault("DetailTex", ref updateMaterial, "");
					EndError();
					BeginError(Any(t => t.DetailScale <= 0.0f));
						DrawDefault("DetailScale", ref updateMaterial, "");
					EndError();
					BeginError(Any(t => t.DetailTiling <= 0.0f));
						DrawDefault("DetailTiling", ref updateMaterial, "");
					EndError();
				EndIndent();
			}

			Separator();
			
			BeginError(Any(t => t.Mesh == null));
				DrawDefault("Mesh", ref updateModel, "This allows you to set the mesh used to render the cloudsphere. This should be a sphere.");
			EndError();
			BeginError(Any(t => t.MeshRadius <= 0.0f));
				DrawDefault("MeshRadius", ref updateModel, "This allows you to set the radius of the Mesh. If this is incorrectly set then the cloudsphere will render incorrectly.");
			EndError();

			if (Any(t => t.DepthTex == null && t.GetComponent<SgtCloudsphereDepthTex>() == null))
			{
				Separator();

				if (Button("Add InnerDepthTex & OuterDepthTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtCloudsphereDepthTex>(t.gameObject));
				}
			}

			if (Any(t => t.Lit == true && t.LightingTex == null && t.GetComponent<SgtCloudsphereLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtCloudsphereLightingTex>(t.gameObject));
				}
			}

			if (Any(t => t.Near == true && t.NearTex == null && t.GetComponent<SgtCloudsphereNearTex>() == null))
			{
				Separator();

				if (Button("Add NearTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtCloudsphereNearTex>(t.gameObject));
				}
			}

			if (Any(t => SetMeshAndMeshRadius(t, false)))
			{
				Separator();

				if (Button("Set Mesh & Mesh Radius") == true)
				{
					Each(t => SetMeshAndMeshRadius(t, true));
				}
			}

			if (updateMaterial == true) DirtyEach(t => t.UpdateMaterial());
			if (updateModel    == true) DirtyEach(t => t.UpdateModel   ());
		}

		private bool SetMeshAndMeshRadius(SgtCloudsphere cloudsphere, bool apply)
		{
			if (cloudsphere.Mesh == null)
			{
				var mesh = SgtHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						cloudsphere.Mesh       = mesh;
						cloudsphere.MeshRadius = SgtHelper.GetMeshRadius(mesh);

						cloudsphere.UpdateMaterial();
						cloudsphere.UpdateModel();
					}

					return true;
				}
			}

			return false;
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a sphere around a planet with a cloud cubemap.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCloudsphere")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Cloudsphere")]
	public class SgtCloudsphere : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the cloudsphere material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>This allows you to set the radius of the cloudsphere in local space.</summary>
		public float Radius = 1.5f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the cloudsphere, giving you fine control over the render order.</summary>
		public float CameraOffset;

		/// <summary>The cube map applied to the cloudsphere surface.</summary>
		public Cubemap MainTex;

		/// <summary>The look up table associating optical depth with cloud color. The left side is used when the depth is thin (e.g. edge of the cloudsphere when looking from space). The right side is used when the depth is thick (e.g. center of the cloudsphere when looking from space).</summary>
		public Texture2D DepthTex;

		/// <summary>Should the stars fade out if they're intersecting solid geometry?</summary>
		[Range(0.0f, 1000.0f)]
		public float Softness;

		/// <summary>If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.</summary>
		public bool Lit;

		/// <summary>The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture LightingTex;

		/// <summary>Enable this if you want the cloudsphere to fade out as the camera approaches.</summary>
		public bool Near;

		/// <summary>The lookup table used to calculate the fade opacity based on distance, where the left side is used when the camera is close, and the right side is used when the camera is far.</summary>
		public Texture NearTex;

		/// <summary>The distance the fading begins from in world space.</summary>
		public float NearDistance = 1.0f;

		/// <summary>Enable this if you want the cloud edges to be enhanced with more detail.</summary>
		public bool Detail;

		/// <summary>This allows you to set the detail map texture, the detail should be stored in the alpha channel.</summary>
		public Texture DetailTex;

		/// <summary>This allows you to set how many times the detail texture is repeating along the cloud surface.</summary>
		public float DetailScale = 8.0f;

		/// <summary>This allows you to set how many times the detail texture is repeating along the cloud surface.</summary>
		public float DetailTiling = 90.0f;

		/// <summary>This allows you to set the mesh used to render the cloudsphere. This should be a sphere.</summary>
		public Mesh Mesh;

		/// <summary>This allows you to set the radius of the Mesh. If this is incorrectly set then the cloudsphere will render incorrectly.</summary>
		public float MeshRadius = 1.0f;

		/// <summary>The model used to render the cloudsphere.</summary>
		[SerializeField]
		private SgtCloudsphereModel model;

		/// <summary>The material applied to the model.</summary>
		[System.NonSerialized]
		private Material material;

		/// <summary>This is used to optimize shader calculations.</summary>
		[System.NonSerialized]
		private bool renderedThisFrame;

		public void UpdateDepthTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._DepthTex, DepthTex);
			}
		}

		public void UpdateNearTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._NearTex, NearTex);
			}
		}

		public void UpdateLightingTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._LightingTex, LightingTex);
			}
		}

		[ContextMenu("Update Material")]
		public void UpdateMaterial()
		{
			renderedThisFrame = false;

			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Cloudsphere (Generated)", SgtHelper.ShaderNamePrefix + "Cloudsphere");

				if (model != null)
				{
					model.SetMaterial(material);
				}
			}

			var color = SgtHelper.Brighten(Color, Brightness);

			material.renderQueue = RenderQueue;

			material.SetColor(SgtShader._Color, color);
			material.SetTexture(SgtShader._MainTex, MainTex);
			material.SetTexture(SgtShader._DepthTex, DepthTex);
			material.SetTexture(SgtShader._LightingTex, LightingTex);

			if (Near == true)
			{
				SgtHelper.EnableKeyword("SGT_A", material); // Near

				material.SetTexture(SgtShader._NearTex, NearTex);
				material.SetFloat(SgtShader._NearScale, SgtHelper.Reciprocal(NearDistance));
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_A", material); // Near
			}

			if (Detail == true)
			{
				SgtHelper.EnableKeyword("SGT_B", material); // Detail

				material.SetTexture(SgtShader._DetailTex, DetailTex);
				material.SetFloat(SgtShader._DetailScale, DetailScale);
				material.SetFloat(SgtShader._DetailTiling, DetailTiling);
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_B", material); // Detail
			}

			if (Softness > 0.0f)
			{
				SgtHelper.EnableKeyword("SGT_C", material); // Softness

				material.SetFloat(SgtShader._SoftParticlesFactor, SgtHelper.Reciprocal(Softness));
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_C", material); // Softness
			}
		}

		[ContextMenu("Update Model")]
		public void UpdateModel()
		{
			if (model == null)
			{
				model = SgtCloudsphereModel.Create(this);
			}

			var scale = SgtHelper.Divide(Radius, MeshRadius);

			model.SetMesh(Mesh);
			model.SetMaterial(material);
			model.SetScale(scale);
		}

		public static SgtCloudsphere Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtCloudsphere Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject  = SgtHelper.CreateGameObject("Cloudsphere", layer, parent, localPosition, localRotation, localScale);
			var cloudsphere = gameObject.AddComponent<SgtCloudsphere>();

			return cloudsphere;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Cloudsphere", false, 10)]
		public static void CreateMenuItem()
		{
			var parent      = SgtHelper.GetSelectedParent();
			var cloudsphere = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(cloudsphere);
		}
#endif

		protected virtual void OnEnable()
		{
			Camera.onPreCull   += CameraPreCull;
			Camera.onPreRender += CameraPreRender;

			if (model != null)
			{
				model.gameObject.SetActive(true);
			}

			UpdateMaterial();
			UpdateModel();
		}

		protected virtual void Update()
		{
			renderedThisFrame = false;
		}

		protected virtual void OnDisable()
		{
			Camera.onPreCull   -= CameraPreCull;
			Camera.onPreRender -= CameraPreRender;

			if (model != null)
			{
				model.gameObject.SetActive(false);
			}
		}

		protected virtual void OnDestroy()
		{
			SgtCloudsphereModel.MarkForDestruction(model);
			SgtHelper.Destroy(material);
		}

		private void CameraPreCull(Camera camera)
		{
			if (CameraOffset != 0.0f)
			{
				if (model != null)
				{
					model.Revert();
					{
						if (CameraOffset != 0.0f)
						{
							var direction = camera.transform.position - transform.position;

							model.transform.position += direction.normalized * CameraOffset;
						}
					}
					model.Save(camera);
				}
			}
		}

		private void CameraPreRender(Camera camera)
		{
			if (model != null)
			{
				model.Restore(camera);
			}

			// Write these once to save CPU
			if (renderedThisFrame == false)
			{
				renderedThisFrame = true;

				// Write lights and shadows
				SgtHelper.SetTempMaterial(material);

				var mask   = 1 << gameObject.layer;
				var lights = SgtLight.Find(Lit, mask);

				SgtShadow.Find(Lit, mask, lights);
				SgtShadow.FilterOutSphere(transform.position);
				SgtShadow.Write(Lit, mask, 2);

				SgtLight.FilterOut(transform.position);
				SgtLight.Write(Lit, transform.position, null, null, 1.0f, 2);
			}
		}
	}
}