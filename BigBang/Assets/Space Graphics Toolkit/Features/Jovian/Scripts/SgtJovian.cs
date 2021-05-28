using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtJovian))]
	public class SgtJovian_Editor : SgtEditor<SgtJovian>
	{
		protected override void OnInspector()
		{
			var updateMaterial = false;
			var updateModel   = false;

			DrawDefault("Color", ref updateMaterial, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness < 0.0f));
				DrawDefault("Brightness", ref updateMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the jovian material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(t => t.MainTex == null));
				DrawDefault("MainTex", ref updateMaterial, "The cube map used as the base texture for the jovian.");
			EndError();
			BeginError(Any(t => t.DepthTex == null));
				DrawDefault("DepthTex", ref updateMaterial, "The look up table associating optical depth with atmosphere color. The left side is used when the atmosphere is thin (e.g. edge of the jovian when looking from space). The right side is used when the atmosphere is thick (e.g. the center of the jovian when looking from space).");
			EndError();

			Separator();

			BeginError(Any(t => t.Sky < 0.0f));
				DrawDefault("Sky", "This allows you to control how thick the atmosphere is when the camera is inside its radius"); // Updated when rendering
			EndError();

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
					DrawDefault("Scattering", ref updateMaterial, "If you enable this then light will scatter through the jovian atmosphere. This means light entering the eye will come from all angles, especially around the light point.");

					if (Any(t => t.Scattering == true))
					{
						BeginIndent();
							BeginError(Any(t => t.ScatteringTex == null));
								DrawDefault("ScatteringTex", ref updateMaterial, "The look up table associating light angle with scattering color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
							EndError();
							DrawDefault("ScatteringStrength", ref updateMaterial, "The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect.");
						EndIndent();
					}
				EndIndent();
			}

			Separator();

			BeginError(Any(t => t.Mesh == null));
				DrawDefault("Mesh", ref updateModel, "This allows you to set the mesh used to render the jovian. This should be a sphere.");
			EndError();
			BeginError(Any(t => t.MeshRadius <= 0.0f));
				DrawDefault("MeshRadius", ref updateModel, "This allows you to set the radius of the Mesh. If this is incorrectly set then the jovian will render incorrectly.");
			EndError();
			BeginError(Any(t => t.Radius <= 0.0f));
				DrawDefault("Radius", ref updateModel, "This allows you to set the radius of the jovian in local space.");
			EndError();

			if (Any(t => t.DepthTex == null && t.GetComponent<SgtJovianDepthTex>() == null))
			{
				Separator();

				if (Button("Add DepthTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtJovianDepthTex>(t.gameObject));
				}
			}

			if (Any(t => t.Lit == true && t.LightingTex == null && t.GetComponent<SgtJovianLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtJovianLightingTex>(t.gameObject));
				}
			}

			if (Any(t => t.Lit == true && t.Scattering == true && t.ScatteringTex == null && t.GetComponent<SgtJovianScatteringTex>() == null))
			{
				Separator();

				if (Button("Add ScatteringTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtJovianScatteringTex>(t.gameObject));
				}
			}

			if (Any(t => SetMeshAndMeshRadius(t, false)))
			{
				Separator();

				if (Button("Set Mesh & MeshRadius") == true)
				{
					Each(t => SetMeshAndMeshRadius(t, true));
				}
			}

			serializedObject.ApplyModifiedProperties();

			if (updateMaterial == true) DirtyEach(t => t.UpdateMaterial());
			if (updateModel    == true) DirtyEach(t => t.UpdateModel   ());
		}

		private bool SetMeshAndMeshRadius(SgtJovian jovian, bool apply)
		{
			if (jovian.Mesh == null)
			{
				var mesh = SgtHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						jovian.Mesh       = mesh;
						jovian.MeshRadius = SgtHelper.GetMeshRadius(mesh);

						jovian.UpdateMaterial();
						jovian.UpdateModel();
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
	/// <summary>This component allows you to render volumetric jovian (gas giant) planets.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtJovian")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Jovian")]
	public class SgtJovian : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the jovian material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>The cube map used as the base texture for the jovian.</summary>
		public Cubemap MainTex;

		/// <summary>The look up table associating optical depth with atmosphere color. The left side is used when the atmosphere is thin (e.g. edge of the jovian when looking from space). The right side is used when the atmosphere is thick (e.g. the center of the jovian when looking from space).</summary>
		public Texture2D DepthTex;

		/// <summary>This allows you to control how thick the atmosphere is when the camera is inside its radius.</summary>
		public float Sky = 1.0f;

		/// <summary>If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.</summary>
		public bool Lit;

		/// <summary>The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture LightingTex;

		/// <summary>If you enable this then light will scatter through the jovian atmosphere. This means light entering the eye will come from all angles, especially around the light point.</summary>
		public bool Scattering;

		/// <summary>The look up table associating light angle with scattering color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture ScatteringTex;

		/// <summary>The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect.</summary>
		public float ScatteringStrength = 3.0f;

		/// <summary>This allows you to set the mesh used to render the jovian. This should be a sphere.</summary>
		public Mesh Mesh;

		/// <summary>This allows you to set the radius of the Mesh. If this is incorrectly set then the jovian will render incorrectly.</summary>
		public float MeshRadius = 1.0f;

		/// <summary>This allows you to set the radius of the jovian in local space.</summary>
		public float Radius = 1.0f;

		/// <summary>This child model rendering the jovian.</summary>
		[SerializeField]
		private SgtJovianModel model;

		/// <summary>The temporary material rendering the jovian.</summary>
		[System.NonSerialized]
		private Material material;

		/// <summary>This is used to optimize shader calculations.</summary>
		[System.NonSerialized]
		private bool renderedThisFrame;

		public virtual void UpdateDepthTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._DepthTex, DepthTex);
			}
		}

		public virtual void UpdateLightingTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._LightingTex, LightingTex);
			}
		}

		public virtual void UpdateScatteringTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._ScatteringTex, ScatteringTex);
			}
		}

		[ContextMenu("Update Material")]
		public void UpdateMaterial()
		{
			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Jovian Material (Generated)", SgtHelper.ShaderNamePrefix + "Jovian");

				if (model != null)
				{
					model.SetMaterial(material);
				}
			}

			material.renderQueue = RenderQueue;

			material.SetTexture(SgtShader._MainTex, MainTex);
			material.SetTexture(SgtShader._DepthTex, DepthTex);
			material.SetColor(SgtShader._Color, SgtHelper.Brighten(Color, Brightness));

			if (Lit == true)
			{
				material.SetTexture(SgtShader._LightingTex, LightingTex);
			}

			SgtHelper.SetTempMaterial(material);

			if (Scattering == true)
			{
				material.SetTexture(SgtShader._ScatteringTex, ScatteringTex);

				SgtHelper.EnableKeyword("SGT_B"); // Scattering
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_B"); // Scattering
			}
		}

		[ContextMenu("Update Model")]
		public void UpdateModel()
		{
			if (model == null)
			{
				model = SgtJovianModel.Create(this);
			}

			var scale = SgtHelper.Divide(Radius, MeshRadius);

			model.SetMesh(Mesh);
			model.SetMaterial(material);
			model.SetScale(scale);
		}

		public static SgtJovian Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtJovian Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Jovian", layer, parent, localPosition, localRotation, localScale);
			var jovian     = gameObject.AddComponent<SgtJovian>();

			return jovian;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Jovian", false, 10)]
		public static void CreateMenuItem()
		{
			var parent = SgtHelper.GetSelectedParent();
			var jovian = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(jovian);
		}
#endif

		protected virtual void OnEnable()
		{
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
			Camera.onPreRender -= CameraPreRender;

			if (model != null)
			{
				model.gameObject.SetActive(false);
			}
		}

		protected virtual void OnDestroy()
		{
			SgtJovianModel.MarkForDestruction(model);

			SgtHelper.Destroy(material);
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			if (SgtHelper.Enabled(this) == true)
			{
				var r0 = transform.lossyScale;

				SgtHelper.DrawSphere(transform.position, transform.right * r0.x, transform.up * r0.y, transform.forward * r0.z);
			}
		}
#endif

		private void CameraPreRender(Camera camera)
		{
			if (material != null)
			{
				var cameraPosition      = camera.transform.position;
				var localCameraPosition = transform.InverseTransformPoint(cameraPosition);
				var localDistance       = localCameraPosition.magnitude;
				var scaleDistance       = SgtHelper.Divide(localDistance, Radius);

				if (DepthTex != null)
				{
#if UNITY_EDITOR
					SgtHelper.MakeTextureReadable(DepthTex);
#endif
					material.SetFloat(SgtShader._Sky, Sky * DepthTex.GetPixelBilinear(1.0f - scaleDistance, 0.0f).a);
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
					SgtLight.Write(Lit, transform.position, transform, null, ScatteringStrength, 2);

					// Write matrices
					var scale        = Radius;
					var localToWorld = transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(scale, scale, scale)); // Double mesh radius so the max thickness caps at 1.0

					material.SetMatrix(SgtShader._WorldToLocal, localToWorld.inverse);
					material.SetMatrix(SgtShader._LocalToWorld, localToWorld);
				}
			}
		}
	}
}