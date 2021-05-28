using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtStarfieldInfinite))]
	public class SgtStarfieldInfinite_Editor : SgtStarfield_Editor<SgtStarfieldInfinite>
	{
		protected override void OnInspector()
		{
			var updateMaterial        = false;
			var updateMeshesAndModels = false;

			DrawMaterial(ref updateMaterial);

			Separator();

			DrawMainTex(ref updateMaterial, ref updateMeshesAndModels);
			DrawLayout(ref updateMaterial, ref updateMeshesAndModels);

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

			DrawPointMaterial(ref updateMaterial);

			DrawDefault("Far", ref updateMaterial, "Should the stars fade out when the camera gets too far away?");

			if (Any(t => t.Far == true))
			{
				BeginIndent();
					BeginError(Any(t => t.FarTex == null));
						DrawDefault("FarTex", ref updateMaterial, "The lookup table used to calculate the fading amount based on the distance.");
					EndError();
					BeginError(Any(t => t.FarRadius < 0.0f));
						DrawDefault("FarRadius", ref updateMaterial, "The radius of the fading effect in world space.");
					EndError();
					BeginError(Any(t => t.FarThickness <= 0.0f));
						DrawDefault("FarThickness", ref updateMaterial, "The thickness of the fading effect in world space.");
					EndError();
				EndIndent();
			}

			Separator();

			DrawDefault("Seed", ref updateMeshesAndModels, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(t => t.Size.x <= 0.0f || t.Size.y <= 0.0f || t.Size.z <= 0.0f));
				DrawDefault("Size", ref updateMeshesAndModels, ref updateMaterial, "The radius of the starfield.");
			EndError();

			Separator();

			BeginError(Any(t => t.StarCount < 0));
				DrawDefault("StarCount", ref updateMeshesAndModels, "The amount of stars that will be generated in the starfield.");
			EndError();
			DrawDefault("StarColors", ref updateMeshesAndModels, "Each star is given a random color from this gradient.");
			BeginError(Any(t => t.StarRadiusMin < 0.0f || t.StarRadiusMin > t.StarRadiusMax));
				DrawDefault("StarRadiusMin", ref updateMeshesAndModels, "The minimum radius of stars in the starfield.");
			EndError();
			BeginError(Any(t => t.StarRadiusMax < 0.0f || t.StarRadiusMin > t.StarRadiusMax));
				DrawDefault("StarRadiusMax", ref updateMeshesAndModels, "The maximum radius of stars in the starfield.");
			EndError();
			BeginError(Any(t => t.StarRadiusBias < 1.0f));
				DrawDefault("StarRadiusBias", ref updateMeshesAndModels, "How likely the size picking will pick smaller stars over larger ones (1 = default/linear).");
			EndError();
			DrawDefault("StarPulseMax", ref updateMeshesAndModels, "The maximum amount a star's size can pulse over time. A value of 1 means the star can potentially pulse between its maximum size, and 0.");
		
			RequireCamera();

			serializedObject.ApplyModifiedProperties();

			if (updateMaterial        == true) DirtyEach(t => t.UpdateMaterial       ());
			if (updateMeshesAndModels == true) DirtyEach(t => t.UpdateMeshesAndModels());

			if (Any(t => t.Far == true && t.FarTex == null && t.GetComponent<SgtStarfieldInfiniteFarTex>() == null))
			{
				Separator();

				if (Button("Add FarTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtStarfieldInfiniteFarTex>(t.gameObject));
				}
			}

			if (Any(t => t.GetComponentInParent<SgtFloatingObject>()))
			{
				EditorGUILayout.HelpBox("SgtStarfieldInfinite automatically snaps with the floating origin system, using SgtFloatingObject may cause issues with this GameObject.", MessageType.Warning);
			}

			if (SgtFloatingCamera.InstanceCount > 0 && Any(t => t.transform.rotation != Quaternion.identity))
			{
				EditorGUILayout.HelpBox("This transform is rotated, this may cause issues with the floating origin system.", MessageType.Warning);
			}

			if (SgtFloatingCamera.InstanceCount > 0 && Any(t => IsUniform(t.transform) == false))
			{
				EditorGUILayout.HelpBox("This transform is non-uniformly scaled, this may cause issues with the floating origin system.", MessageType.Warning);
			}
		}

		private bool IsUniform(Transform t)
		{
			var scale = t.localScale;

			if (scale.x != scale.y || scale.x != scale.z)
			{
				return false;
			}

			if (t.parent != null)
			{
				return IsUniform(t.parent);
			}

			return true;
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a starfield that repeats forever.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtStarfieldInfinite")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Starfield Infinite")]
	public class SgtStarfieldInfinite : SgtStarfield
	{
		/// <summary>Should the stars fade out if they're intersecting solid geometry?</summary>
		[Range(0.0f, 1000.0f)]
		public float Softness;

		/// <summary>Should the stars fade out when the camera gets too far away?</summary>
		public bool Far;

		/// <summary>The lookup table used to calculate the fading amount based on the distance.</summary>
		public Texture FarTex;

		/// <summary>The radius of the fading effect in world coordinates.</summary>
		public float FarRadius = 2.0f;

		/// <summary>The thickness of the fading effect in world coordinates.</summary>
		public float FarThickness = 2.0f;

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public SgtSeed Seed;

		/// <summary>The size of the starfield in local space.</summary>
		public Vector3 Size = Vector3.one;

		/// <summary>The amount of stars that will be generated in the starfield.</summary>
		public int StarCount = 1000;

		/// <summary>Each star is given a random color from this gradient.</summary>
		public Gradient StarColors;

		/// <summary>The minimum radius of stars in the starfield.</summary>
		public float StarRadiusMin = 0.01f;

		/// <summary>The maximum radius of stars in the starfield.</summary>
		public float StarRadiusMax = 0.05f;

		/// <summary>How likely the size picking will pick smaller stars over larger ones (1 = default/linear).</summary>
		public float StarRadiusBias = 1.0f;

		/// <summary>The maximum amount a star's size can pulse over time. A value of 1 means the star can potentially pulse between its maximum size, and 0.</summary>
		[Range(0.0f, 1.0f)]
		public float StarPulseMax = 1.0f;

		protected override string ShaderName
		{
			get
			{
				return SgtHelper.ShaderNamePrefix + "StarfieldInfinite";
			}
		}

		public void UpdateFarTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._FarTex, FarTex);
			}
		}

		public static SgtStarfieldInfinite Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtStarfieldInfinite Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject        = SgtHelper.CreateGameObject("Starfield Infinite", layer, parent, localPosition, localRotation, localScale);
			var starfieldInfinite = gameObject.AddComponent<SgtStarfieldInfinite>();

			return starfieldInfinite;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Starfield Infinite", false, 10)]
		private static void CreateMenuItem()
		{
			var parent            = SgtHelper.GetSelectedParent();
			var starfieldInfinite = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(starfieldInfinite);
		}
#endif

		protected override void OnEnable()
		{
			base.OnEnable();

			SgtFloatingCamera.OnSnap += FloatingCameraSnap;
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			SgtFloatingCamera.OnSnap -= FloatingCameraSnap;
		}

		private void FloatingCameraSnap(SgtFloatingCamera floatingCamera, Vector3 delta)
		{
			// Make sure the right camera snapped
			if (floatingCamera.IsRendering(gameObject.layer) == true)
			{
				var position = transform.position + delta;
				var extent   = Size * SgtHelper.UniformScale(transform.lossyScale);

				position.x = position.x % extent.x;
				position.y = position.y % extent.y;
				position.z = position.z % extent.z;

				transform.position = position;
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireCube(Vector3.zero, Size);
		}
#endif

		protected override void BuildMaterial()
		{
			base.BuildMaterial();

			if (Softness > 0.0f)
			{
				SgtHelper.EnableKeyword("LIGHT_2", material); // Softness

				material.SetFloat(SgtShader._SoftParticlesFactor, SgtHelper.Reciprocal(Softness));
			}
			else
			{
				SgtHelper.DisableKeyword("LIGHT_2", material); // Softness
			}

			if (Far == true)
			{
				SgtHelper.EnableKeyword("SGT_E", material); // Far

				material.SetTexture(SgtShader._FarTex, FarTex);
				material.SetFloat(SgtShader._FarRadius, FarRadius);
				material.SetFloat(SgtShader._FarScale, SgtHelper.Reciprocal(FarThickness));
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_E", material); // Far
			}

			material.SetVector(SgtShader._WrapSize, Size);
			material.SetVector(SgtShader._WrapScale, SgtHelper.Reciprocal3(Size));
		}

		protected override int BeginQuads()
		{
			SgtHelper.BeginRandomSeed(Seed);

			if (StarColors == null)
			{
				StarColors = SgtHelper.CreateGradient(Color.white);
			}

			return StarCount;
		}

		protected override void NextQuad(ref SgtStarfieldStar star, int starIndex)
		{
			var x = Random.Range(Size.x * -0.5f, Size.x * 0.5f);
			var y = Random.Range(Size.y * -0.5f, Size.y * 0.5f);
			var z = Random.Range(Size.z * -0.5f, Size.z * 0.5f);

			star.Variant     = Random.Range(int.MinValue, int.MaxValue);
			star.Color       = StarColors.Evaluate(Random.value);
			star.Radius      = Mathf.Lerp(StarRadiusMin, StarRadiusMax, Mathf.Pow(Random.value, StarRadiusBias));
			star.Angle       = Random.Range(-180.0f, 180.0f);
			star.Position    = new Vector3(x, y, z);
			star.PulseRange  = Random.value * StarPulseMax;
			star.PulseSpeed  = Random.value;
			star.PulseOffset = Random.value;
		}

		protected override void BuildMesh(Mesh mesh, int starIndex, int starCount)
		{
			base.BuildMesh(mesh, starIndex, starCount);

			mesh.bounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue);
		}

		protected override void EndQuads()
		{
			SgtHelper.EndRandomSeed();
		}

		protected override void CameraPreCull(Camera camera)
		{
			base.CameraPreCull(camera);

			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var model = models[i];

					if (model != null)
					{
						model.transform.position = camera.transform.position;
					}
				}
			}
		}

		protected override void CameraPreRender(Camera camera)
		{
			base.CameraPreRender(camera);

			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var model = models[i];

					if (model != null)
					{
						model.transform.localPosition = Vector3.zero;
					}
				}
			}
		}
	}
}