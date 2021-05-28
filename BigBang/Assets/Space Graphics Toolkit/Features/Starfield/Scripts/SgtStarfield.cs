using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	public class SgtStarfield_Editor<T> : SgtQuads_Editor<T>
		where T : SgtStarfield
	{
		protected void DrawPointMaterial(ref bool updateMaterial)
		{
			Separator();

			DrawDefault("PowerRgb", ref updateMaterial, "Instead of just tinting the stars with the colors, should the RGB values be raised to the power of the color?");

			DrawDefault("Stretch", ref updateMaterial, "Should the stars stretch if an observer moves?");

			if (Any(t => t.Stretch == true))
			{
				BeginIndent();
					DrawDefault("StretchVector", ref updateMaterial, "The vector of the stretching.");
					BeginError(Any(t => t.StretchScale < 0.0f));
						DrawDefault("StretchScale", ref updateMaterial, "The scale of the stretching relative to the velocity.");
					EndError();
					BeginError(Any(t => t.StretchLimit <= 0.0f));
						DrawDefault("StretchLimit", "When warping with the floating origin system the camera velocity can get too large, this allows you to limit it.");
					EndError();
				EndIndent();
			}

			DrawDefault("Pulse", ref updateMaterial, "Should the stars pulse in size over time?");

			if (Any(t => t.Pulse == true))
			{
				BeginIndent();
					DrawDefault("PulseOffset", "The amount of seconds this starfield has been animating.");
					BeginError(Any(t => t.PulseSpeed == 0.0f));
						DrawDefault("PulseSpeed", "The animation speed of this starfield.");
					EndError();
				EndIndent();
			}

			DrawDefault("Near", ref updateMaterial, "Should the stars fade out when the camera gets near?");

			if (Any(t => t.Near == true))
			{
				BeginIndent();
					BeginError(Any(t => t.NearTex == null));
						DrawDefault("NearTex", ref updateMaterial, "The lookup table used to calculate the fading amount based on the distance.");
					EndError();
					BeginError(Any(t => t.NearThickness < 0.0f));
						DrawDefault("NearThickness", ref updateMaterial, "The thickness of the fading effect in world space.");
					EndError();
				EndIndent();
			}

			if (Any(t => t.Near == true && t.NearTex == null && t.GetComponent<SgtStarfieldNearTex>() == null))
			{
				Separator();

				if (Button("Add NearTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtStarfieldNearTex>(t.gameObject));
				}
			}
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This is the base class for all starfields that store star corner vertices the same point/location and are stretched out in the vertex shader, allowing billboarding in view space, and dynamic resizing.</summary>
	public abstract class SgtStarfield : SgtQuads
	{
		/// <summary>Instead of just tinting the stars with the colors, should the RGB values be raised to the power of the color?</summary>
		public bool PowerRgb;

		/// <summary>Should the stars stretch if an observer moves?</summary>
		public bool Stretch;

		/// <summary>The vector of the stretching.</summary>
		public Vector3 StretchVector;

		/// <summary>The scale of the stretching relative to the velocity.</summary>
		public float StretchScale = 1.0f;

		/// <summary>When warping with the floating origin system the camera velocity can get too large, this allows you to limit it.</summary>
		public float StretchLimit = 10000.0f;

		/// <summary>Should the stars fade out when the camera gets near?</summary>
		public bool Near;

		/// <summary>The lookup table used to calculate the fading amount based on the distance.</summary>
		public Texture NearTex;

		/// <summary>The thickness of the fading effect in world space.</summary>
		public float NearThickness = 2.0f;

		/// <summary>Should the stars pulse in size over time?</summary>
		public bool Pulse;

		/// <summary>The amount of seconds this starfield has been animating.</summary>
		public float PulseOffset;

		/// <summary>The animation speed of this starfield.</summary>
		public float PulseSpeed = 1.0f;

		protected override string ShaderName
		{
			get
			{
				return SgtHelper.ShaderNamePrefix + "Starfield";
			}
		}

		public void UpdateNearTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._NearTex, NearTex);
			}
		}

		public SgtStarfieldCustom MakeEditableCopy(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
#if UNITY_EDITOR
			SgtHelper.BeginUndo("Create Editable Starfield Copy");
#endif
			var gameObject      = SgtHelper.CreateGameObject("Editable Starfield Copy", layer, parent, localPosition, localRotation, localScale);
			var customStarfield = SgtHelper.AddComponent<SgtStarfieldCustom>(gameObject, false);
			var quads           = new List<SgtStarfieldStar>();
			var starCount       = BeginQuads();

			for (var i = 0; i < starCount; i++)
			{
				var quad = SgtPoolClass<SgtStarfieldStar>.Pop() ?? new SgtStarfieldStar();

				NextQuad(ref quad, i);

				quads.Add(quad);
			}

			EndQuads();

			// Copy common settings
			customStarfield.Color             = Color;
			customStarfield.Brightness        = Brightness;
			customStarfield.MainTex           = MainTex;
			customStarfield.Layout            = Layout;
			customStarfield.LayoutColumns     = LayoutColumns;
			customStarfield.LayoutRows        = LayoutRows;
			customStarfield.LayoutRects       = new List<Rect>(LayoutRects);
			customStarfield.BlendMode         = BlendMode;
			customStarfield.RenderQueue       = RenderQueue;
			customStarfield.PowerRgb          = PowerRgb;
			customStarfield.Stretch           = Stretch;
			customStarfield.StretchVector     = StretchVector;
			customStarfield.StretchScale      = StretchScale;
			customStarfield.StretchLimit      = StretchLimit;
			customStarfield.Near              = Near;
			customStarfield.NearTex           = NearTex;
			customStarfield.NearThickness     = NearThickness;
			customStarfield.Pulse             = Pulse;
			customStarfield.PulseOffset       = PulseOffset;
			customStarfield.PulseSpeed        = PulseSpeed;

			// Copy custom settings
			customStarfield.Stars = quads;

			// Update
			customStarfield.UpdateMaterial();
			customStarfield.UpdateMeshesAndModels();

			return customStarfield;
		}

		public SgtStarfieldCustom MakeEditableCopy(int layer = 0, Transform parent = null)
		{
			return MakeEditableCopy(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

#if UNITY_EDITOR
		[ContextMenu("Make Editable Copy")]
		public void MakeEditableCopyContext()
		{
			var customStarfield = MakeEditableCopy(gameObject.layer, transform.parent, transform.localPosition, transform.localRotation, transform.localScale);

			SgtHelper.SelectAndPing(customStarfield);
		}
#endif

		protected override void OnEnable()
		{
			Camera.onPreCull   += CameraPreCull;
			Camera.onPreRender += CameraPreRender;

			base.OnEnable();
		}

		protected virtual void LateUpdate()
		{
			if (material != null)
			{
				if (Pulse == true)
				{
					material.SetFloat(SgtShader._PulseOffset, PulseOffset);
				}
			}

			UpdatePulse();
		}

		protected override void OnDisable()
		{
			Camera.onPreCull   -= CameraPreCull;
			Camera.onPreRender -= CameraPreRender;

			base.OnDisable();
		}

		protected override void BuildMaterial()
		{
			base.BuildMaterial();

			if (BlendMode == SgtQuads.BlendModeType.Default)
			{
				BuildAdditive();
			}

			if (PowerRgb == true)
			{
				SgtHelper.EnableKeyword("SGT_B", material); // PowerRgb
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_B", material); // PowerRgb
			}

			if (Stretch == true)
			{
				SgtHelper.EnableKeyword("SGT_C", material); // Stretch
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_C", material); // Stretch
			}

			if (Near == true)
			{
				SgtHelper.EnableKeyword("SGT_D", material); // Near

				material.SetTexture(SgtShader._NearTex, NearTex);
				material.SetFloat(SgtShader._NearScale, SgtHelper.Reciprocal(NearThickness));
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_D", material); // Near
			}

			if (Pulse == true)
			{
				SgtHelper.EnableKeyword("LIGHT_1", material); // Pulse

				// This is also set in Update
				material.SetFloat(SgtShader._PulseOffset, PulseOffset);
			}
			else
			{
				SgtHelper.DisableKeyword("LIGHT_1", material); // Pulse
			}
		}

		protected abstract void NextQuad(ref SgtStarfieldStar quad, int starIndex);

		protected override void BuildMesh(Mesh mesh, int starIndex, int starCount)
		{
			var positions = new Vector3[starCount * 4];
			var colors    = new Color[starCount * 4];
			var normals   = new Vector3[starCount * 4];
			var tangents  = new Vector4[starCount * 4];
			var coords1   = new Vector2[starCount * 4];
			var coords2   = new Vector2[starCount * 4];
			var indices   = new int[starCount * 6];
			var minMaxSet = false;
			var min       = default(Vector3);
			var max       = default(Vector3);

			for (var i = 0; i < starCount; i++)
			{
				NextQuad(ref SgtStarfieldStar.Temp, starIndex + i);

				var offV     = i * 4;
				var offI     = i * 6;
				var position = SgtStarfieldStar.Temp.Position;
				var radius   = SgtStarfieldStar.Temp.Radius;
				var angle    = Mathf.Repeat(SgtStarfieldStar.Temp.Angle / 180.0f, 2.0f) - 1.0f;
				var uv       = tempCoords[SgtHelper.Mod(SgtStarfieldStar.Temp.Variant, tempCoords.Count)];

				ExpandBounds(ref minMaxSet, ref min, ref max, position, radius);

				positions[offV + 0] =
				positions[offV + 1] =
				positions[offV + 2] =
				positions[offV + 3] = position;

				colors[offV + 0] =
				colors[offV + 1] =
				colors[offV + 2] =
				colors[offV + 3] = SgtStarfieldStar.Temp.Color;

				normals[offV + 0] = new Vector3(-1.0f,  1.0f, angle);
				normals[offV + 1] = new Vector3( 1.0f,  1.0f, angle);
				normals[offV + 2] = new Vector3(-1.0f, -1.0f, angle);
				normals[offV + 3] = new Vector3( 1.0f, -1.0f, angle);

				tangents[offV + 0] =
				tangents[offV + 1] =
				tangents[offV + 2] =
				tangents[offV + 3] = new Vector4(SgtStarfieldStar.Temp.PulseOffset, SgtStarfieldStar.Temp.PulseSpeed, SgtStarfieldStar.Temp.PulseRange, 0.0f);

				coords1[offV + 0] = new Vector2(uv.x, uv.y);
				coords1[offV + 1] = new Vector2(uv.z, uv.y);
				coords1[offV + 2] = new Vector2(uv.x, uv.w);
				coords1[offV + 3] = new Vector2(uv.z, uv.w);

				coords2[offV + 0] = new Vector2(radius,  0.5f);
				coords2[offV + 1] = new Vector2(radius, -0.5f);
				coords2[offV + 2] = new Vector2(radius,  0.5f);
				coords2[offV + 3] = new Vector2(radius, -0.5f);

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
			mesh.bounds    = SgtHelper.NewBoundsFromMinMax(min, max);
		}

		protected virtual void CameraPreCull(Camera camera)
		{
		}

		protected virtual void CameraPreRender(Camera camera)
		{
			if (material != null)
			{
				var velocity  = StretchVector;
				var sgtCamera = default(SgtCamera);

				if (SgtCamera.TryFind(camera, ref sgtCamera) == true)
				{
					material.SetFloat(SgtShader._CameraRollAngle, sgtCamera.RollAngle * Mathf.Deg2Rad);

					var cameraVelocity = sgtCamera.Velocity;
					var cameraSpeed    = cameraVelocity.magnitude;

					if (cameraSpeed > StretchLimit)
					{
						cameraVelocity = cameraVelocity.normalized * StretchLimit;
					}

					velocity += cameraVelocity * StretchScale;
				}
				else
				{
					material.SetFloat(SgtShader._CameraRollAngle, 0.0f);
				}

				if (Stretch == true)
				{
					material.SetVector(SgtShader._StretchVector, velocity);
					material.SetVector(SgtShader._StretchDirection, velocity.normalized);
					material.SetFloat(SgtShader._StretchLength, velocity.magnitude);
				}
				else
				{
					material.SetVector(SgtShader._StretchVector, Vector3.zero);
					material.SetVector(SgtShader._StretchDirection, Vector3.zero);
					material.SetFloat(SgtShader._StretchLength, 0.0f);
				}
			}
		}

		private void UpdatePulse()
		{
#if UNITY_EDITOR
			if (Application.isPlaying == false)
			{
				return;
			}
#endif
			if (Pulse == true)
			{
				PulseOffset += Time.deltaTime * PulseSpeed;
			}
		}
	}
}