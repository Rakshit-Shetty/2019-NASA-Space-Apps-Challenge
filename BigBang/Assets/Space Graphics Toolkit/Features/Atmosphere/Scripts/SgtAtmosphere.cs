using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAtmosphere))]
	public class SgtAtmosphere_Editor : SgtEditor<SgtAtmosphere>
	{
		protected override void OnInspector()
		{
			var updateMaterials = false;
			var updateModel     = false;

			DrawDefault("color", ref updateMaterials, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness < 0.0f));
				DrawDefault("brightness", ref updateMaterials, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("renderQueue", ref updateMaterials, "This allows you to adjust the render queue of the atmosphere materials. You can normally adjust the render queue in the material settings, but since these materials are procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(t => t.InnerDepthTex == null));
				DrawDefault("innerDepthTex", ref updateMaterials, "The look up table associating optical depth with atmospheric color for the planet surface. The left side is used when the atmosphere is thin (e.g. center of the planet when looking from space). The right side is used when the atmosphere is thick (e.g. the horizon).");
			EndError();
			BeginError(Any(t => t.InnerMeshRadius <= 0.0f));
				DrawDefault("innerMeshRadius", ref updateMaterials, ref updateModel, "The radius of the meshes set in the SgtSharedMaterial.");
			EndError();

			Separator();

			BeginError(Any(t => t.OuterDepthTex == null));
				DrawDefault("outerDepthTex", ref updateMaterials, "The look up table associating optical depth with atmospheric color for the planet sky. The left side is used when the atmosphere is thin (e.g. edge of the atmosphere when looking from space). The right side is used when the atmosphere is thick (e.g. the horizon).");
			EndError();
			BeginError(Any(t => t.OuterMesh == null));
				DrawDefault("outerMesh", ref updateModel, "This allows you to set the mesh used to render the atmosphere. This should be a sphere.");
			EndError();
			BeginError(Any(t => t.OuterMeshRadius <= 0.0f));
				DrawDefault("outerMeshRadius", ref updateModel, "This allows you to set the radius of the OuterMesh. If this is incorrectly set then the atmosphere will render incorrectly.");
			EndError();
			DrawDefault("outerSoftness", ref updateMaterials, "If you have a big object that is both inside and outside of the atmosphere, it can cause a sharp intersection line. Increasing this setting allows you to change the smoothness of this intersection.");

			if (Any(t => t.OuterSoftness > 0.0f))
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

			BeginError(Any(t => t.Height <= 0.0f));
				DrawDefault("height", ref updateMaterials, ref updateModel, "This allows you to set how high the atmosphere extends above the surface of the planet in local space.");
			EndError();
			BeginError(Any(t => t.InnerFog >= 1.0f));
				DrawDefault("innerFog", ref updateMaterials, "This allows you to adjust the fog level of the atmosphere on the surface.");
			EndError();
			BeginError(Any(t => t.OuterFog >= 1.0f));
				DrawDefault("outerFog", ref updateMaterials, "This allows you to adjust the fog level of the atmosphere in the sky.");
			EndError();
			BeginError(Any(t => t.Sky < 0.0f));
				DrawDefault("sky", "This allows you to control how thick the atmosphere is when the camera is inside its radius"); // Updated automatically
			EndError();
			DrawDefault("middle", "This allows you to set the altitude where atmospheric density reaches its maximum point. The lower you set this, the foggier the horizon will appear when approaching the surface."); // Updated automatically
			DrawDefault("cameraOffset", "This allows you to offset the camera distance in world space when rendering the atmosphere, giving you fine control over the render order."); // Updated automatically

			Separator();

			DrawDefault("lit", ref updateMaterials, "If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.");

			if (Any(t => t.Lit == true))
			{
				if (SgtLight.InstanceCount == 0)
				{
					EditorGUILayout.HelpBox("You need to add the SgtLight component to your scene lights for them to work with SGT.", MessageType.Warning);
				}

				BeginIndent();
					BeginError(Any(t => t.LightingTex == null));
						DrawDefault("lightingTex", ref updateMaterials, "The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
					EndError();
					DrawDefault("scattering", ref updateMaterials, "If you enable this then light will scatter through the atmosphere. This means light entering the eye will come from all angles, especially around the light point.");
					if (Any(t => t.Scattering == true))
					{
						BeginIndent();
							DrawDefault("groundScattering", ref updateMaterials, "If you enable this then atmospheric scattering will be applied to the surface material.");
							BeginError(Any(t => t.ScatteringTex == null));
								DrawDefault("scatteringTex", ref updateMaterials, "The look up table associating light angle with scattering color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
							EndError();
							DrawDefault("scatteringStrength", ref updateMaterials, "The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect.");
							DrawDefault("scatteringMie", ref updateMaterials, "The mie scattering term, allowing you to adjust the distribution of front scattered light.");
							DrawDefault("scatteringRayleigh", ref updateMaterials, "The mie rayleigh term, allowing you to adjust the distribution of front and back scattered light.");
						EndIndent();
					}
					DrawDefault("night", "Should the night side of the atmosphere have different sky values?"); // Updated automatically
					if (Any(t => t.Night == true))
					{
						BeginIndent();
							DrawDefault("nightSky", "The 'Sky' value of the night side."); // Updated automatically
							DrawDefault("nightEase", "The transition style between the day and night."); // Updated automatically
							BeginError(Any(t => t.NightStart >= t.NightEnd));
								DrawDefault("nightStart", "The start point of the day/sunset transition (0 = dark side, 1 = light side)."); // Updated automatically
								DrawDefault("nightEnd", "The end point of the day/sunset transition (0 = dark side, 1 = light side)."); // Updated automatically
							EndError();
							BeginError(Any(t => t.NightPower < 1.0f));
								DrawDefault("nightPower", "The power of the night transition."); // Updated automatically
							EndError();
						EndIndent();
					}
				EndIndent();
			}

			if (Any(t => (t.InnerDepthTex == null || t.OuterDepthTex == null) && t.GetComponent<SgtAtmosphereDepthTex>() == null))
			{
				Separator();

				if (Button("Add InnerDepthTex & OuterDepthTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtAtmosphereDepthTex>(t.gameObject));
				}
			}

			if (Any(t => t.Lit == true && t.LightingTex == null && t.GetComponent<SgtAtmosphereLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtAtmosphereLightingTex>(t.gameObject));
				}
			}

			if (Any(t => t.Lit == true && t.Scattering == true && t.ScatteringTex == null && t.GetComponent<SgtAtmosphereScatteringTex>() == null))
			{
				Separator();

				if (Button("Add ScatteringTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtAtmosphereScatteringTex>(t.gameObject));
				}
			}

			if (Any(t => SetOuterMeshAndOuterMeshRadius(t, false)))
			{
				Separator();

				if (Button("Set Outer Mesh & Outer Mesh Radius") == true)
				{
					Each(t => SetOuterMeshAndOuterMeshRadius(t, true));
				}
			}

			if (Any(t => AddInnerRendererAndSetInnerMeshRadius(t, false)))
			{
				Separator();

				if (Button("Add Inner Renderer & Set Inner Mesh Radius") == true)
				{
					Each(t => AddInnerRendererAndSetInnerMeshRadius(t, true));
				}
			}

			if (updateMaterials == true) DirtyEach(t => t.UpdateMaterials());
			if (updateModel     == true) DirtyEach(t => t.UpdateModel    ());
		}

		private bool SetOuterMeshAndOuterMeshRadius(SgtAtmosphere atmosphere, bool apply)
		{
			if (atmosphere.OuterMesh == null)
			{
				var mesh = SgtHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						atmosphere.OuterMesh       = mesh;
						atmosphere.OuterMeshRadius = SgtHelper.GetMeshRadius(mesh);

						atmosphere.UpdateMaterials();
						atmosphere.UpdateModel();
					}

					return true;
				}
			}

			return false;
		}

		private bool AddInnerRendererAndSetInnerMeshRadius(SgtAtmosphere atmosphere, bool apply)
		{
			if (atmosphere.CachedSharedMaterial.RendererCount == 0)
			{
				var meshRenderer = atmosphere.GetComponentInParent<MeshRenderer>();

				if (meshRenderer != null)
				{
					var meshFilter = meshRenderer.GetComponent<MeshFilter>();

					if (meshFilter != null)
					{
						var mesh = meshFilter.sharedMesh;

						if (mesh != null)
						{
							if (apply == true)
							{
								atmosphere.CachedSharedMaterial.AddRenderer(meshRenderer);
								atmosphere.InnerMeshRadius = SgtHelper.GetMeshRadius(mesh);
								atmosphere.UpdateModel();
							}

							return true;
						}
					}
				}
			}

			return false;
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to draw a volumetric atmosphere. The atmosphere is rendered using two materials, one for the surface (inner), and one for the sky (outer).
	/// The outer part of the atmosphere is automatically generated by this component using the OuterMesh you specify.
	/// The inner part of the atmosphere is provided by you (e.g. a normal sphere GameObject), and is specified in the SgtSharedMaterial component that this component automatically adds.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtSharedMaterial))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtAtmosphere")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Atmosphere")]
	public class SgtAtmosphere : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color { set { color = value; } get { return color; } } [FormerlySerializedAs("Color")] [SerializeField] private Color color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [FormerlySerializedAs("Brightness")] [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the atmosphere materials. You can normally adjust the render queue in the material settings, but since these materials are procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue { set { renderQueue = value; } get { return renderQueue; } } [FormerlySerializedAs("RenderQueue")] [SerializeField] private SgtRenderQueue renderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>The look up table associating optical depth with atmospheric color for the planet surface. The left side is used when the atmosphere is thin (e.g. center of the planet when looking from space). The right side is used when the atmosphere is thick (e.g. the horizon).</summary>
		public Texture InnerDepthTex { set { innerDepthTex = value; } get { return innerDepthTex; } } [FormerlySerializedAs("InnerDepthTex")] [SerializeField] private Texture innerDepthTex;

		/// <summary>The radius of the meshes set in the SgtSharedMaterial.</summary>
		public float InnerMeshRadius { set { innerMeshRadius = value; } get { return innerMeshRadius; } } [FormerlySerializedAs("InnerMeshRadius")] [SerializeField] private float innerMeshRadius;

		/// <summary>The look up table associating optical depth with atmospheric color for the planet sky. The left side is used when the atmosphere is thin (e.g. edge of the atmosphere when looking from space). The right side is used when the atmosphere is thick (e.g. the horizon).</summary>
		public Texture2D OuterDepthTex { set { outerDepthTex = value; } get { return outerDepthTex; } } [FormerlySerializedAs("OuterDepthTex")] [SerializeField] private Texture2D outerDepthTex;

		/// <summary>This allows you to set the mesh used to render the atmosphere. This should be a sphere.</summary>
		public Mesh OuterMesh { set { outerMesh = value; } get { return outerMesh; } } [FormerlySerializedAs("OuterMesh")] [SerializeField] private Mesh outerMesh;

		/// <summary>This allows you to set the radius of the OuterMesh. If this is incorrectly set then the atmosphere will render incorrectly.</summary>
		public float OuterMeshRadius { set { outerMeshRadius = value; } get { return outerMeshRadius; } } [FormerlySerializedAs("OuterMeshRadius")] [SerializeField] private float outerMeshRadius = 1.0f;

		/// <summary>If you have a big object that is both inside and outside of the atmosphere, it can cause a sharp intersection line. Increasing this setting allows you to change the smoothness of this intersection.</summary>
		public float OuterSoftness { set { outerSoftness = value; } get { return outerSoftness; } } [SerializeField] [Range(0.0f, 1000.0f)] private float outerSoftness;

		/// <summary>This allows you to set how high the atmosphere extends above the surface of the planet in local space.</summary>
		public float Height { set { height = value; } get { return height; } } [FormerlySerializedAs("Height")] [SerializeField] private float height = 0.1f;
		public void SetHeight(float value) { height = value; UpdateMaterials(); UpdateModel(); }

		/// <summary>This allows you to adjust the fog level of the atmosphere on the surface.</summary>
		public float InnerFog { set { innerFog = value; } get { return innerFog; } } [FormerlySerializedAs("InnerFog")] [SerializeField] private float innerFog;
		public void SetInnerFog(float value) { innerFog = value; UpdateMaterials(); }

		/// <summary>This allows you to adjust the fog level of the atmosphere in the sky.</summary>
		public float OuterFog { set { outerFog = value; } get { return outerFog; } } [FormerlySerializedAs("OuterFog")] [SerializeField] private float outerFog;
		public void SetOuterFog(float value) { outerFog = value; UpdateMaterials(); }

		/// <summary>This allows you to control how thick the atmosphere is when the camera is inside its radius.</summary>
		public float Sky { set { sky = value; } get { return sky; } } [FormerlySerializedAs("Sky")] [SerializeField] private float sky = 1.0f;

		/// <summary>This allows you to set the altitude where atmospheric density reaches its maximum point. The lower you set this, the foggier the horizon will appear when approaching the surface.</summary>
		public float Middle { set { middle = value; } get { return middle; } } [FormerlySerializedAs("Middle")] [SerializeField] [Range(0.0f, 1.0f)] private float middle = 0.5f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the atmosphere, giving you fine control over the render order.</summary>
		public float CameraOffset { set { cameraOffset = value; } get { return cameraOffset; } } [FormerlySerializedAs("CameraOffset")] [SerializeField] private float cameraOffset;

		/// <summary>If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.</summary>
		public bool Lit { set { lit = value; } get { return lit; } } [FormerlySerializedAs("Lit")] [SerializeField] private bool lit;

		/// <summary>The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture LightingTex { set { lightingTex = value; } get { return lightingTex; } } [FormerlySerializedAs("LightingTex")] [SerializeField] private Texture lightingTex;

		/// <summary>If you enable this then light will scatter through the atmosphere. This means light entering the eye will come from all angles, especially around the light point.</summary>
		public bool Scattering { set { scattering = value; } get { return scattering; } } [FormerlySerializedAs("Scattering")] [SerializeField] private bool scattering;

		/// <summary>If you enable this then atmospheric scattering will be applied to the surface material.</summary>
		public bool GroundScattering { set { groundScattering = value; } get { return groundScattering; } } [FormerlySerializedAs("GroundScattering")] [SerializeField] private bool groundScattering;

		/// <summary>The look up table associating light angle with scattering color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture ScatteringTex { set { scatteringTex = value; } get { return scatteringTex; } } [FormerlySerializedAs("ScatteringTex")] [SerializeField] private Texture scatteringTex;

		/// <summary>The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect.</summary>
		public float ScatteringStrength { set { scatteringStrength = value; } get { return scatteringStrength; } } [FormerlySerializedAs("ScatteringStrength")] [SerializeField] private float scatteringStrength = 3.0f;
		public void SetScatteringStrength(float value) { scatteringStrength = value; UpdateMaterials(); }

		/// <summary>The mie scattering term, allowing you to adjust the distribution of front scattered light.</summary>
		public float ScatteringMie { set { scatteringMie = value; } get { return scatteringMie; } } [FormerlySerializedAs("ScatteringMie")] [SerializeField] private float scatteringMie = 50.0f;
		public void SetScatteringMie(float value) { scatteringMie = value; UpdateMaterials(); }

		/// <summary>The mie rayleigh term, allowing you to adjust the distribution of front and back scattered light.</summary>
		public float ScatteringRayleigh { set { scatteringRayleigh = value; } get { return scatteringRayleigh; } } [FormerlySerializedAs("ScatteringRayleigh")] [SerializeField] private float scatteringRayleigh = 0.1f;
		public void SetScatteringRayleigh(float value) { scatteringRayleigh = value; UpdateMaterials(); }

		/// <summary>Should the night side of the atmosphere have different sky values?</summary>
		public bool Night { set { night = value; } get { return night; } } [FormerlySerializedAs("Night")] [SerializeField] private bool night;

		/// <summary>The 'Sky' value of the night side.</summary>
		public float NightSky { set { nightSky = value; } get { return nightSky; } } [FormerlySerializedAs("NightSky")] [SerializeField] private float nightSky = 0.25f;

		/// <summary>The transition style between the day and night.</summary>
		public SgtEase.Type NightEase { set { nightEase = value; } get { return nightEase; } } [FormerlySerializedAs("NightEase")] [SerializeField] private SgtEase.Type nightEase;

		/// <summary>The start point of the day/sunset transition (0 = dark side, 1 = light side).</summary>
		public float NightStart { set { nightStart = value; } get { return nightStart; } } [FormerlySerializedAs("NightStart")] [SerializeField] [Range(0.0f, 1.0f)] private float nightStart = 0.4f;

		/// <summary>The end point of the day/sunset transition (0 = dark side, 1 = light side).</summary>
		public float NightEnd { set { nightEnd = value; } get { return nightEnd; } } [FormerlySerializedAs("NightEnd")] [SerializeField] [Range(0.0f, 1.0f)] private float nightEnd = 0.6f;

		/// <summary>The power of the night transition.</summary>
		public float NightPower { set { nightPower = value; } get { return nightPower; } } [FormerlySerializedAs("NightPower")] [SerializeField] private float nightPower = 2.0f;

		// The GameObjects used to render the sky
		[SerializeField]
		private SgtAtmosphereModel model;

		// The material applied to the surface
		[System.NonSerialized]
		private Material innerMaterial;

		// The material applied to the sky
		[System.NonSerialized]
		private Material outerMaterial;

		[System.NonSerialized]
		private SgtSharedMaterial cachedSharedMaterial;

		[System.NonSerialized]
		private bool cachedSharedMaterialSet;

		[System.NonSerialized]
		private Transform cachedTransform;

		[System.NonSerialized]
		private bool cachedTransformSet;

		public float OuterRadius
		{
			get
			{
				return InnerMeshRadius + Height;
			}
		}

		public Material InnerMaterial
		{
			get
			{
				return innerMaterial;
			}
		}

		public SgtSharedMaterial CachedSharedMaterial
		{
			get
			{
				if (cachedSharedMaterialSet == false)
				{
					cachedSharedMaterial    = GetComponent<SgtSharedMaterial>();
					cachedSharedMaterialSet = true;
				}

				return cachedSharedMaterial;
			}
		}

		public void UpdateInnerDepthTex()
		{
			if (innerMaterial != null)
			{
				innerMaterial.SetTexture(SgtShader._DepthTex, InnerDepthTex);
			}
		}

		public void UpdateOuterDepthTex()
		{
			if (outerMaterial != null)
			{
				outerMaterial.SetTexture(SgtShader._DepthTex, OuterDepthTex);
			}
		}

		public void UpdateLightingTex()
		{
			if (innerMaterial != null)
			{
				innerMaterial.SetTexture(SgtShader._LightingTex, LightingTex);
			}

			if (outerMaterial != null)
			{
				outerMaterial.SetTexture(SgtShader._LightingTex, LightingTex);
			}
		}

		public void UpdateScatteringTex()
		{
			if (innerMaterial != null)
			{
				innerMaterial.SetTexture(SgtShader._ScatteringTex, ScatteringTex);
			}

			if (outerMaterial != null)
			{
				outerMaterial.SetTexture(SgtShader._ScatteringTex, ScatteringTex);
			}
		}

		[ContextMenu("Update Materials")]
		public void UpdateMaterials()
		{
			CacheTransform();

			if (innerMaterial == null)
			{
				innerMaterial = SgtHelper.CreateTempMaterial("Atmosphere Inner (Generated)", SgtHelper.ShaderNamePrefix + "AtmosphereInner");

				CachedSharedMaterial.Material = innerMaterial;
			}

			if (outerMaterial == null)
			{
				outerMaterial = SgtHelper.CreateTempMaterial("Atmosphere Outer (Generated)", SgtHelper.ShaderNamePrefix + "AtmosphereOuter");

				if (model != null)
				{
					model.SetMaterial(outerMaterial);
				}
			}

			var color      = SgtHelper.Brighten(Color, Brightness);
			var innerRatio = SgtHelper.Divide(InnerMeshRadius, OuterRadius);

			innerMaterial.renderQueue = outerMaterial.renderQueue = RenderQueue;

			innerMaterial.SetColor(SgtShader._Color, color);
			outerMaterial.SetColor(SgtShader._Color, color);

			innerMaterial.SetTexture(SgtShader._DepthTex, InnerDepthTex);
			outerMaterial.SetTexture(SgtShader._DepthTex, OuterDepthTex);

			innerMaterial.SetFloat(SgtShader._InnerRatio, innerRatio);
			innerMaterial.SetFloat(SgtShader._InnerScale, 1.0f / (1.0f - innerRatio));

			if (outerSoftness > 0.0f)
			{
				SgtHelper.EnableKeyword("SGT_A", outerMaterial); // Softness

				outerMaterial.SetFloat(SgtShader._SoftParticlesFactor, SgtHelper.Reciprocal(outerSoftness));
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_A", outerMaterial); // Softness
			}

			if (Lit == true)
			{
				innerMaterial.SetTexture(SgtShader._LightingTex, LightingTex);
				outerMaterial.SetTexture(SgtShader._LightingTex, LightingTex);

				if (Scattering == true)
				{
					outerMaterial.SetTexture(SgtShader._ScatteringTex, ScatteringTex);
					outerMaterial.SetFloat(SgtShader._ScatteringMie, ScatteringMie);
					outerMaterial.SetFloat(SgtShader._ScatteringRayleigh, ScatteringRayleigh);

					SgtHelper.EnableKeyword("SGT_B", outerMaterial); // Scattering

					if (GroundScattering == true)
					{
						innerMaterial.SetTexture(SgtShader._ScatteringTex, ScatteringTex);
						innerMaterial.SetFloat(SgtShader._ScatteringMie, ScatteringMie);
						innerMaterial.SetFloat(SgtShader._ScatteringRayleigh, ScatteringRayleigh);

						SgtHelper.EnableKeyword("SGT_B", innerMaterial); // Scattering
					}
					else
					{
						SgtHelper.DisableKeyword("SGT_B", innerMaterial); // Scattering
					}
				}
				else
				{
					SgtHelper.DisableKeyword("SGT_B", innerMaterial); // Scattering
					SgtHelper.DisableKeyword("SGT_B", outerMaterial); // Scattering
				}
			}
		}

		[ContextMenu("Update Model")]
		public void UpdateModel()
		{
			if (model == null)
			{
				model = SgtAtmosphereModel.Create(this);
			}

			var scale = SgtHelper.Divide(OuterRadius, OuterMeshRadius);

			model.SetMesh(OuterMesh);
			model.SetMaterial(outerMaterial);
			model.SetScale(scale);
		}

		public static SgtAtmosphere Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtAtmosphere Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Atmosphere", layer, parent, localPosition, localRotation, localScale);
			var atmosphere = gameObject.AddComponent<SgtAtmosphere>();

			return atmosphere;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Atmosphere", false, 10)]
		public static void CreateMenuItem()
		{
			var parent     = SgtHelper.GetSelectedParent();
			var atmosphere = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(atmosphere);
		}
#endif

		protected virtual void OnEnable()
		{
			Camera.onPreCull   += CameraPreCull;
			Camera.onPreRender += CameraPreRender;

			CacheTransform();

			CachedSharedMaterial.Material = innerMaterial;

			if (model != null)
			{
				model.gameObject.SetActive(true);
			}

			UpdateMaterials();
			UpdateModel();
		}

		protected virtual void OnDisable()
		{
			Camera.onPreCull   -= CameraPreCull;
			Camera.onPreRender -= CameraPreRender;

			cachedSharedMaterial.Material = null;

			if (model != null)
			{
				model.gameObject.SetActive(false);
			}
		}

		protected virtual void OnDestroy()
		{
			SgtAtmosphereModel.MarkForDestruction(model);
			SgtHelper.Destroy(outerMaterial);
			SgtHelper.Destroy(innerMaterial);
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			if (SgtHelper.Enabled(this) == true)
			{
				var r1 = InnerMeshRadius;
				var r2 = OuterRadius;

				SgtHelper.DrawSphere(transform.position, transform.right * transform.lossyScale.x * r1, transform.up * transform.lossyScale.y * r1, transform.forward * transform.lossyScale.z * r1);
				SgtHelper.DrawSphere(transform.position, transform.right * transform.lossyScale.x * r2, transform.up * transform.lossyScale.y * r2, transform.forward * transform.lossyScale.z * r2);
			}
		}
#endif

		private void CameraPreCull(Camera camera)
		{
			if (model != null)
			{
				model.Revert();
				{
					if (CameraOffset != 0.0f)
					{
						var direction = camera.transform.position - cachedTransform.position;

						model.transform.position += direction.normalized * CameraOffset;
					}
				}
				model.Save(camera);
			}
		}

		private void CameraPreRender(Camera camera)
		{
			if (model != null)
			{
				model.Restore(camera);
			}

			// Write camera-dependant shader values
			if (innerMaterial != null && outerMaterial != null)
			{
				var localPosition  = cachedTransform.InverseTransformPoint(camera.transform.position);
				var localDistance  = localPosition.magnitude;
				var height01       = Mathf.InverseLerp(OuterRadius, InnerMeshRadius, localDistance);
				var innerThickness = default(float);
				var outerThickness = default(float);
				var innerRatio     = SgtHelper.Divide(InnerMeshRadius, OuterRadius);
				var middleRatio    = Mathf.Lerp(innerRatio, 1.0f, middle);
				var distance       = SgtHelper.Divide(localDistance, OuterRadius);
				var innerDensity   = 1.0f - InnerFog;
				var outerDensity   = 1.0f - OuterFog;

				SgtHelper.CalculateHorizonThickness(innerRatio, middleRatio, distance, out innerThickness, out outerThickness);

				innerMaterial.SetFloat(SgtShader._HorizonLengthRecip, SgtHelper.Reciprocal(innerThickness * innerDensity));
				outerMaterial.SetFloat(SgtShader._HorizonLengthRecip, SgtHelper.Reciprocal(outerThickness * outerDensity));

				if (OuterDepthTex != null)
				{
#if UNITY_EDITOR
					SgtHelper.MakeTextureReadable(OuterDepthTex);
#endif
					outerMaterial.SetFloat(SgtShader._Sky, GetSky(camera) * OuterDepthTex.GetPixelBilinear(height01 / outerDensity, 0.0f).a);
				}

				var scale        = SgtHelper.Divide(OuterMeshRadius, OuterRadius);
				var worldToLocal = Matrix4x4.Scale(new Vector3(scale, scale, scale)) * cachedTransform.worldToLocalMatrix; // cachedTransform might not be set here, so use the property

				innerMaterial.SetMatrix(SgtShader._WorldToLocal, worldToLocal);
				outerMaterial.SetMatrix(SgtShader._WorldToLocal, worldToLocal);

				// Write lights and shadows
				SgtHelper.SetTempMaterial(innerMaterial, outerMaterial);

				var mask   = 1 << gameObject.layer;
				var lights = SgtLight.Find(Lit, mask);

				SgtShadow.Find(Lit, mask, lights);
				SgtShadow.FilterOutSphere(transform.position);
				SgtShadow.Write(Lit, mask, 2);

				SgtLight.FilterOut(transform.position);
				SgtLight.Write(Lit, transform.position, cachedTransform, null, ScatteringStrength, 2);
			}
		}

		private float GetSky(Camera camera)
		{
			if (Lit == true && Night == true)
			{
				var mask   = 1 << gameObject.layer;
				var lights = SgtLight.Find(Lit, mask);

				var lighting        = 0.0f;
				var cameraDirection = (camera.transform.position - transform.position).normalized;

				for (var i = 0; i < lights.Count && i < 2; i++)
				{
					var light     = lights[i];
					var position  = default(Vector3);
					var direction = default(Vector3);
					var color     = default(Color);

					SgtLight.Calculate(light, transform.position, null, null, ref position, ref direction, ref color);

					var dot     = Vector3.Dot(direction, cameraDirection) * 0.5f + 0.5f;
					var night01 = Mathf.InverseLerp(NightEnd, NightStart, dot);
					var night   = SgtEase.Evaluate(NightEase, 1.0f - Mathf.Pow(night01, NightPower));

					if (night > lighting)
					{
						lighting = night;
					}
				}

				return Mathf.Lerp(NightSky, Sky, lighting);
			}
		
			return Sky;
		}

		private void CacheTransform()
		{
			if (cachedTransformSet == false)
			{
				cachedTransform    = GetComponent<Transform>();
				cachedTransformSet = true;
			}
		}
	}
}