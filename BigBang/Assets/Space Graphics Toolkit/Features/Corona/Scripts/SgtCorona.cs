using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCorona))]
	public class SgtCorona_Editor : SgtEditor<SgtCorona>
	{
		protected override void OnInspector()
		{
			var updateMaterials = false;
			var updateModel     = false;

			DrawDefault("color", ref updateMaterials, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness < 0.0f));
				DrawDefault("brightness", ref updateMaterials, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("renderQueue", ref updateMaterials, "This allows you to adjust the render queue of the corona materials. You can normally adjust the render queue in the material settings, but since these materials are procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(t => t.InnerDepthTex == null));
				DrawDefault("innerDepthTex", ref updateMaterials, "The look up table associating optical depth with coronal color for the star surface. The left side is used when the corona is thin (e.g. center of the star when looking from space). The right side is used when the corona is thick (e.g. the horizon).");
			EndError();
			BeginError(Any(t => t.InnerMeshRadius <= 0.0f));
				DrawDefault("innerMeshRadius", ref updateMaterials, ref updateModel, "The radius of the inner renderers (surface) in local coordinates.");
			EndError();

			Separator();

			BeginError(Any(t => t.OuterDepthTex == null));
				DrawDefault("outerDepthTex", ref updateMaterials, "The look up table associating optical depth with coronal color for the star sky. The left side is used when the corona is thin (e.g. edge of the corona when looking from space). The right side is used when the corona is thick (e.g. the horizon).");
			EndError();
			BeginError(Any(t => t.OuterMesh == null));
				DrawDefault("outerMesh", ref updateModel, "This allows you to set the mesh used to render the atmosphere. This should be a sphere.");
			EndError();
			BeginError(Any(t => t.OuterMeshRadius <= 0.0f));
				DrawDefault("outerMeshRadius", ref updateModel, "This allows you to set the radius of the OuterMesh. If this is incorrectly set then the corona will render incorrectly.");
			EndError();
			DrawDefault("outerSoftness", ref updateMaterials, "Should the outer corona fade out against intersecting geometry?");

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
				DrawDefault("height", ref updateMaterials, ref updateModel, "This allows you to set how high the corona extends above the surface of the star in local space.");
			EndError();
			BeginError(Any(t => t.InnerFog >= 1.0f));
				DrawDefault("innerFog", ref updateMaterials, "If you want an extra-thin or extra-thick density, you can adjust that here (0 = default).");
			EndError();
			BeginError(Any(t => t.OuterFog >= 1.0f));
				DrawDefault("outerFog", ref updateMaterials, "If you want an extra-thin or extra-thick density, you can adjust that here (0 = default).");
			EndError();
			BeginError(Any(t => t.Sky < 0.0f));
				DrawDefault("sky", "This allows you to control how thick the corona is when the camera is inside its radius."); // Updated when rendering
			EndError();
			DrawDefault("middle", "This allows you to set the altitude where atmospheric density reaches its maximum point. The lower you set this, the foggier the horizon will appear when approaching the surface."); // Updated automatically
			DrawDefault("cameraOffset", "This allows you to offset the camera distance in world space when rendering the corona, giving you fine control over the render order."); // Updated automatically

			if (Any(t => (t.InnerDepthTex == null || t.OuterDepthTex == null) && t.GetComponent<SgtCoronaDepthTex>() == null))
			{
				Separator();

				if (Button("Add InnerDepthTex & OuterDepthTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtCoronaDepthTex>(t.gameObject));
				}
			}

			if (Any(t => SetOuterMeshAndOuterMeshRadius(t, false)))
			{
				Separator();

				if (Button("Set OuterMesh & OuterMeshRadius") == true)
				{
					Each(t => SetOuterMeshAndOuterMeshRadius(t, true));
				}
			}

			if (Any(t => AddInnerRendererAndSetInnerMeshRadius(t, false)))
			{
				Separator();

				if (Button("Add InnerRenderer & Set InnerMeshRadius") == true)
				{
					Each(t => AddInnerRendererAndSetInnerMeshRadius(t, true));
				}
			}

			if (updateMaterials == true) DirtyEach(t => t.UpdateMaterials());
			if (updateModel     == true) DirtyEach(t => t.UpdateModel    ());
		}

		private bool SetOuterMeshAndOuterMeshRadius(SgtCorona corona, bool apply)
		{
			if (corona.OuterMesh == null)
			{
				var mesh = SgtHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						corona.OuterMesh       = mesh;
						corona.OuterMeshRadius = SgtHelper.GetMeshRadius(mesh);

						corona.UpdateMaterials();
						corona.UpdateModel();
					}

					return true;
				}
			}

			return false;
		}

		private bool AddInnerRendererAndSetInnerMeshRadius(SgtCorona corona, bool apply)
		{
			if (corona.CachedSharedMaterial.RendererCount == 0)
			{
				var meshRenderer = corona.GetComponentInParent<MeshRenderer>();

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
								corona.CachedSharedMaterial.AddRenderer(meshRenderer);
								corona.InnerMeshRadius = SgtHelper.GetMeshRadius(mesh);
								corona.UpdateModel();
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
	/// <summary>This component allows you to draw a volumetric corona around a sphere.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtSharedMaterial))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCorona")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Corona")]
	public class SgtCorona : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color { set { color = value; } get { return color; } } [FormerlySerializedAs("Color")] [SerializeField] private Color color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [FormerlySerializedAs("Brightness")] [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the corona materials. You can normally adjust the render queue in the material settings, but since these materials are procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue { set { renderQueue = value; } get { return renderQueue; } } [FormerlySerializedAs("RenderQueue")] [SerializeField] private SgtRenderQueue renderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>This allows you to set how high the corona extends above the surface of the star in local space.</summary>
		public float Height { set { height = value; } get { return height; } } [FormerlySerializedAs("Height")] [SerializeField] private float height = 0.1f;

		/// <summary>If you want an extra-thin or extra-thick density, you can adjust that here (0 = default).</summary>
		public float InnerFog { set { innerFog = value; } get { return innerFog; } } [FormerlySerializedAs("InnerFog")] [SerializeField] private float innerFog;

		/// <summary>If you want an extra-thin or extra-thick density, you can adjust that here (0 = default).</summary>
		public float OuterFog { set { outerFog = value; } get { return outerFog; } } [FormerlySerializedAs("OuterFog")] [SerializeField] private float outerFog;

		/// <summary>This allows you to control how thick the corona is when the camera is inside its radius.</summary>
		public float Sky { set { sky = value; } get { return sky; } } [FormerlySerializedAs("Sky")] [SerializeField] private float sky = 1.0f;

		/// <summary>This allows you to set the altitude where atmospheric density reaches its maximum point. The lower you set this, the foggier the horizon will appear when approaching the surface.</summary>
		public float Middle { set { middle = value; } get { return middle; } } [FormerlySerializedAs("Middle")] [SerializeField] [Range(0.0f, 1.0f)] private float middle = 1.0f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the corona, giving you fine control over the render order.</summary>
		public float CameraOffset { set { cameraOffset = value; } get { return cameraOffset; } } [FormerlySerializedAs("CameraOffset")] [SerializeField] private float cameraOffset;

		/// <summary>The look up table associating optical depth with coronal color for the star surface. The left side is used when the corona is thin (e.g. center of the star when looking from space). The right side is used when the corona is thick (e.g. the horizon).</summary>
		public Texture InnerDepthTex { set { innerDepthTex = value; } get { return innerDepthTex; } } [FormerlySerializedAs("InnerDepthTex")] [SerializeField] private Texture innerDepthTex;

		/// <summary>The radius of the inner renderers (surface) in local coordinates.</summary>
		public float InnerMeshRadius { set { innerMeshRadius = value; } get { return innerMeshRadius; } } [FormerlySerializedAs("InnerMeshRadius")] [SerializeField] private float innerMeshRadius = 1.0f;

		/// <summary>The look up table associating optical depth with coronal color for the star sky. The left side is used when the corona is thin (e.g. edge of the corona when looking from space). The right side is used when the corona is thick (e.g. the horizon).</summary>
		public Texture2D OuterDepthTex { set { outerDepthTex = value; } get { return outerDepthTex; } } [FormerlySerializedAs("OuterDepthTex")] [SerializeField] private Texture2D outerDepthTex;

		/// <summary>This allows you to set the mesh used to render the atmosphere. This should be a sphere.</summary>
		public Mesh OuterMesh { set { outerMesh = value; } get { return outerMesh; } } [FormerlySerializedAs("OuterMesh")] [SerializeField] private Mesh outerMesh;

		/// <summary>This allows you to set the radius of the OuterMesh. If this is incorrectly set then the corona will render incorrectly.</summary>
		public float OuterMeshRadius { set { outerMeshRadius = value; } get { return outerMeshRadius; } } [FormerlySerializedAs("OuterMeshRadius")] [SerializeField] private float outerMeshRadius;

		/// <summary>Should the outer corona fade out against intersecting geometry?</summary>
		public float OuterSoftness { set { outerSoftness = value; } get { return outerSoftness; } } [SerializeField] [Range(0.0f, 1000.0f)] private float outerSoftness;

		/// <summary>Each model is used to render one segment of the disc.</summary>
		[SerializeField]
		private SgtCoronaModel model;

		/// <summary>The material applied to all inner renderers.</summary>
		[System.NonSerialized]
		private Material innerMaterial;

		/// <summary>The material applied to the outer model.</summary>
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

		[ContextMenu("Update Materials")]
		public void UpdateMaterials()
		{
			if (innerMaterial == null)
			{
				innerMaterial = SgtHelper.CreateTempMaterial("Corona Inner (Generated)", SgtHelper.ShaderNamePrefix + "CoronaInner");

				CachedSharedMaterial.Material = innerMaterial;
			}

			if (outerMaterial == null)
			{
				outerMaterial = SgtHelper.CreateTempMaterial("Corona Outer (Generated)", SgtHelper.ShaderNamePrefix + "CoronaOuter");

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

			UpdateMaterialNonSerialized();
		}

		[ContextMenu("Update Model")]
		public void UpdateModel()
		{
			if (model == null)
			{
				model = SgtCoronaModel.Create(this);
			}

			var scale = SgtHelper.Divide(OuterRadius, OuterMeshRadius);

			model.SetMesh(OuterMesh);
			model.SetMaterial(outerMaterial);
			model.SetScale(scale);
		}

		public static SgtCorona Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtCorona Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Corona", layer, parent, localPosition, localRotation, localScale);
			var corona     = gameObject.AddComponent<SgtCorona>();

			return corona;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Corona", false, 10)]
		public static void CreateMenuItem()
		{
			var parent = SgtHelper.GetSelectedParent();
			var corona = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(corona);
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
			SgtCoronaModel.MarkForDestruction(model);
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
					outerMaterial.SetFloat(SgtShader._Sky, Sky * OuterDepthTex.GetPixelBilinear(height01 / outerDensity, 0.0f).a);
				}

				UpdateMaterialNonSerialized();
			}
		}

		private void UpdateMaterialNonSerialized()
		{
			var scale        = SgtHelper.Divide(OuterMeshRadius, OuterRadius);
			var worldToLocal = Matrix4x4.Scale(new Vector3(scale, scale, scale)) * cachedTransform.worldToLocalMatrix;

			innerMaterial.SetMatrix(SgtShader._WorldToLocal, worldToLocal);
			outerMaterial.SetMatrix(SgtShader._WorldToLocal, worldToLocal);
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