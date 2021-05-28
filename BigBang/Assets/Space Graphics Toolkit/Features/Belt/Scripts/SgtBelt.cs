using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	public abstract class SgtBelt_Editor<T> : SgtQuads_Editor<T>
		where T : SgtBelt
	{
		protected override void DrawMaterial(ref bool updateMaterial)
		{
			DrawDefault("color", ref updateMaterial, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness < 0.0f));
				DrawDefault("brightness", ref updateMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("BlendMode", ref updateMaterial, "The blend mode used to render the material.");
			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the belt material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");
			DrawDefault("OrbitOffset", "The amount of seconds this belt has been animating for."); // Updated automatically
			DrawDefault("OrbitSpeed", "The animation speed of this belt."); // Updated automatically
		}

		protected void DrawLighting(ref bool updateMaterial)
		{
			DrawDefault("PowerRgb", ref updateMaterial, "Instead of just tinting the asteroids with the colors, should the RGB values be raised to the power of the color?");
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

			if (Any(t => t.Lit == true && t.LightingTex == null && t.GetComponent<SgtBeltLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtBeltLightingTex>(t.gameObject));
				}
			}
		}

		protected override void DrawMainTex(ref bool updateMaterial, ref bool updateMeshesAndModels)
		{
			BeginError(Any(t => t.MainTex == null));
				DrawDefault("MainTex", ref updateMaterial, "The main texture of this material.");
			EndError();
			DrawLayout(ref updateMaterial, ref updateMeshesAndModels);
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This base class contains the functionality to render an asteroid belt.</summary>
	public abstract class SgtBelt : SgtQuads
	{
		/// <summary>The amount of seconds this belt has been animating for.</summary>
		public float OrbitOffset;

		/// <summary>The animation speed of this belt.</summary>
		public float OrbitSpeed = 1.0f;

		/// <summary>If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.</summary>
		public bool Lit;

		/// <summary>Instead of just tinting the asteroids with the colors, should the RGB values be raised to the power of the color?</summary>
		public bool PowerRgb;

		/// <summary>The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture LightingTex;

		/// <summary>This is used to optimize shader calculations.</summary>
		[System.NonSerialized]
		private bool renderedThisFrame;

		protected override string ShaderName
		{
			get
			{
				return SgtHelper.ShaderNamePrefix + "Belt";
			}
		}

		public SgtBeltCustom MakeEditableCopy(int layer = 0, Transform parent = null)
		{
			return MakeEditableCopy(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public SgtBeltCustom MakeEditableCopy(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
#if UNITY_EDITOR
			SgtHelper.BeginUndo("Create Editable Belt Copy");
#endif
			var gameObject = SgtHelper.CreateGameObject("Editable Belt Copy", layer, parent, localPosition, localRotation, localScale);
			var customBelt = SgtHelper.AddComponent<SgtBeltCustom>(gameObject, false);
			var quads      = new List<SgtBeltAsteroid>();
			var quadCount  = BeginQuads();

			for (var i = 0; i < quadCount; i++)
			{
				var asteroid = SgtPoolClass<SgtBeltAsteroid>.Pop() ?? new SgtBeltAsteroid();

				NextQuad(ref asteroid, i);

				quads.Add(asteroid);
			}

			EndQuads();

			// Copy common settings
			customBelt.Color         = Color;
			customBelt.Brightness    = Brightness;
			customBelt.MainTex       = MainTex;
			customBelt.Layout        = Layout;
			customBelt.LayoutColumns = LayoutColumns;
			customBelt.LayoutRows    = LayoutRows;
			customBelt.LayoutRects   = new List<Rect>(LayoutRects);
			customBelt.BlendMode     = BlendMode;
			customBelt.RenderQueue   = RenderQueue;
			customBelt.OrbitOffset   = OrbitOffset;
			customBelt.OrbitSpeed    = OrbitSpeed;
			customBelt.Lit           = Lit;
			customBelt.LightingTex   = LightingTex;
			customBelt.PowerRgb      = PowerRgb;

			// Copy custom settings
			customBelt.Asteroids = quads;

			// Update
			customBelt.UpdateMaterial();
			customBelt.UpdateMeshesAndModels();

			return customBelt;
		}

		public virtual void UpdateLightingTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._LightingTex, LightingTex);
			}
		}

#if UNITY_EDITOR
		[ContextMenu("Make Editable Copy")]
		public void MakeEditableCopyContext()
		{
			var customBelt = MakeEditableCopy(gameObject.layer, transform.parent, transform.localPosition, transform.localRotation, transform.localScale);

			SgtHelper.SelectAndPing(customBelt);
		}
#endif

		protected override void OnEnable()
		{
			Camera.onPreRender += CameraPreRender;

			base.OnEnable();
		}

		protected virtual void LateUpdate()
		{
			renderedThisFrame = false;

			if (Application.isPlaying == true)
			{
				OrbitOffset += Time.deltaTime * OrbitSpeed;
			}

			if (material != null)
			{
				material.SetFloat(SgtShader._Age, OrbitOffset);
			}
		}

		protected override void OnDisable()
		{
			Camera.onPreRender -= CameraPreRender;

			base.OnDisable();
		}

		protected override void BuildMaterial()
		{
			base.BuildMaterial();

			if (BlendMode == BlendModeType.Default)
			{
				BuildAlphaTest();
			}

			material.SetFloat(SgtShader._Age, OrbitOffset);

			if (Lit == true)
			{
				material.SetTexture(SgtShader._LightingTex, LightingTex);
			}

			if (PowerRgb == true)
			{
				SgtHelper.EnableKeyword("SGT_B", material); // PowerRgb
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_B", material); // PowerRgb
			}
		}

		protected abstract void NextQuad(ref SgtBeltAsteroid quad, int starIndex);

		protected override void BuildMesh(Mesh mesh, int asteroidIndex, int asteroidCount)
		{
			var positions = new Vector3[asteroidCount * 4];
			var colors    = new Color[asteroidCount * 4];
			var normals   = new Vector3[asteroidCount * 4];
			var tangents  = new Vector4[asteroidCount * 4];
			var coords1   = new Vector2[asteroidCount * 4];
			var coords2   = new Vector2[asteroidCount * 4];
			var indices   = new int[asteroidCount * 6];
			var maxWidth  = 0.0f;
			var maxHeight = 0.0f;

			for (var i = 0; i < asteroidCount; i++)
			{
				NextQuad(ref SgtBeltAsteroid.Temp, asteroidIndex + i);

				var offV     = i * 4;
				var offI     = i * 6;
				var radius   = SgtBeltAsteroid.Temp.Radius;
				var distance = SgtBeltAsteroid.Temp.OrbitDistance;
				var height   = SgtBeltAsteroid.Temp.Height;
				var uv       = tempCoords[SgtHelper.Mod(SgtBeltAsteroid.Temp.Variant, tempCoords.Count)];

				maxWidth  = Mathf.Max(maxWidth , distance + radius);
				maxHeight = Mathf.Max(maxHeight, height   + radius);

				positions[offV + 0] =
				positions[offV + 1] =
				positions[offV + 2] =
				positions[offV + 3] = new Vector3(SgtBeltAsteroid.Temp.OrbitAngle, distance, SgtBeltAsteroid.Temp.OrbitSpeed);

				colors[offV + 0] =
				colors[offV + 1] =
				colors[offV + 2] =
				colors[offV + 3] = SgtBeltAsteroid.Temp.Color;

				normals[offV + 0] = new Vector3(-1.0f,  1.0f, 0.0f);
				normals[offV + 1] = new Vector3( 1.0f,  1.0f, 0.0f);
				normals[offV + 2] = new Vector3(-1.0f, -1.0f, 0.0f);
				normals[offV + 3] = new Vector3( 1.0f, -1.0f, 0.0f);

				tangents[offV + 0] =
				tangents[offV + 1] =
				tangents[offV + 2] =
				tangents[offV + 3] = new Vector4(SgtBeltAsteroid.Temp.Angle / Mathf.PI, SgtBeltAsteroid.Temp.Spin / Mathf.PI, 0.0f, 0.0f);

				coords1[offV + 0] = new Vector2(uv.x, uv.y);
				coords1[offV + 1] = new Vector2(uv.z, uv.y);
				coords1[offV + 2] = new Vector2(uv.x, uv.w);
				coords1[offV + 3] = new Vector2(uv.z, uv.w);

				coords2[offV + 0] =
				coords2[offV + 1] =
				coords2[offV + 2] =
				coords2[offV + 3] = new Vector2(radius, height);

				indices[offI + 0] = offV + 0;
				indices[offI + 1] = offV + 1;
				indices[offI + 2] = offV + 2;
				indices[offI + 3] = offV + 3;
				indices[offI + 4] = offV + 2;
				indices[offI + 5] = offV + 1;
			}

			mesh.vertices  = positions;
			mesh.colors    = colors;
			mesh.normals   = normals;
			mesh.tangents  = tangents;
			mesh.uv        = coords1;
			mesh.uv2       = coords2;
			mesh.triangles = indices;
			mesh.bounds    = new Bounds(Vector3.zero, new Vector3(maxWidth * 2.0f, maxHeight * 2.0f, maxWidth * 2.0f));
		}

		private void ObserverPreRender(SgtCamera observer)
		{
			if (material != null)
			{
				material.SetFloat(SgtShader._CameraRollAngle, observer.RollAngle * Mathf.Deg2Rad);
			}
		}

		protected void CameraPreRender(Camera camera)
		{
			if (material != null)
			{
				var observer = default(SgtCamera);

				if (SgtCamera.TryFind(camera, ref observer) == true)
				{
					material.SetFloat(SgtShader._CameraRollAngle, observer.RollAngle * Mathf.Deg2Rad);
				}
				else
				{
					material.SetFloat(SgtShader._CameraRollAngle, 0.0f);
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
					SgtShadow.FilterOutRing(transform.position);
					SgtShadow.Write(Lit, mask, 2);

					SgtLight.Write(Lit, transform.position, transform, null, 1.0f, 2);
				}
			}
		}
	}
}