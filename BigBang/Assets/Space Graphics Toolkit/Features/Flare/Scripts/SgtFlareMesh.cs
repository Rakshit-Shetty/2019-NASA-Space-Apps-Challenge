using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFlareMesh))]
	public class SgtFlareMesh_Editor : SgtEditor<SgtFlareMesh>
	{
		protected override void OnInspector()
		{
			var updateMesh  = false;
			var updateApply = false;

			BeginError(Any(t => t.Detail <= 2));
				DrawDefault("Detail", ref updateMesh, "The amount of points used to make the flare mesh.");
			EndError();
			BeginError(Any(t => t.Radius <= 0.0f));
				DrawDefault("Radius", ref updateMesh, "The base radius of the flare in local space.");
			EndError();

			Separator();

			DrawDefault("Wave", ref updateMesh, "Deform the flare based on cosine wave?");

			if (Any(t => t.Wave == true))
			{
				BeginIndent();
					DrawDefault("WaveStrength", ref updateMesh, "The strength of the wave in local space.");
					BeginError(Any(t => t.WavePoints < 0));
						DrawDefault("WavePoints", ref updateMesh, "The amount of wave peaks.");
					EndError();
					BeginError(Any(t => t.WavePower < 1.0f));
						DrawDefault("WavePower", ref updateMesh, "The sharpness of the waves.");
					EndError();
					DrawDefault("WavePhase", ref updateMesh, "The angle offset of the waves.");
				EndIndent();
			}

			Separator();
		
			DrawDefault("Noise", ref updateMesh, "Deform the flare based on noise?");

			if (Any(t => t.Noise == true))
			{
				BeginIndent();
					BeginError(Any(t => t.NoiseStrength < 0.0f));
						DrawDefault("NoiseStrength", ref updateMesh, "The strength of the noise in local space.");
					EndError();
					BeginError(Any(t => t.NoisePoints <= 0));
						DrawDefault("NoisePoints", ref updateMesh, "The amount of noise points.");
					EndError();
					DrawDefault("NoisePhase", ref updateMesh, "The angle offset of the noise.");
					DrawDefault("NoiseSeed", ref updateMesh, "The random seed used for the random noise.");
				EndIndent();
			}

			if (updateMesh  == true) DirtyEach(t => t.UpdateMesh ());
			if (updateApply == true) DirtyEach(t => t.ApplyMesh());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtFlare.Mesh field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtFlare))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFlareMesh")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Flare Mesh")]
	public class SgtFlareMesh : MonoBehaviour
	{
		/// <summary>The amount of points used to make the flare mesh.</summary>
		public int Detail = 512;

		/// <summary>The base radius of the flare in local space.</summary>
		public float Radius = 2.0f;

		/// <summary>Deform the flare based on cosine wave?</summary>
		public bool Wave;

		/// <summary>The strength of the wave in local space.</summary>
		public float WaveStrength = 5.0f;

		/// <summary>The amount of wave peaks.</summary>
		public int WavePoints = 4;

		/// <summary>The sharpness of the waves.</summary>
		public float WavePower = 5.0f;

		/// <summary>The angle offset of the waves.</summary>
		public float WavePhase;

		/// <summary>Deform the flare based on noise?</summary>
		public bool Noise;

		/// <summary>The strength of the noise in local space.</summary>
		public float NoiseStrength = 5.0f;

		/// <summary>The amount of noise points.</summary>
		public int NoisePoints = 50;

		/// <summary>The angle offset of the noise.</summary>
		public float NoisePhase;

		/// <summary>The random seed used for the random noise.</summary>
		public SgtSeed NoiseSeed;

		[System.NonSerialized]
		private Mesh generatedMesh;

		[System.NonSerialized]
		private SgtFlare cachedFlare;

		[System.NonSerialized]
		private bool cachedFlareSet;

		private static List<float> points = new List<float>();

		public Mesh GeneratedMesh
		{
			get
			{
				return generatedMesh;
			}
		}

#if UNITY_EDITOR
		[ContextMenu("Export Mesh")]
		public void ExportMesh()
		{
			SgtHelper.ExportAssetDialog(generatedMesh, "Flare Mesh");
		}
#endif

		[ContextMenu("Update Mesh")]
		public void UpdateMesh()
		{
			if (Detail > 2)
			{
				if (generatedMesh == null)
				{
					generatedMesh = SgtHelper.CreateTempMesh("Mesh (Generated)");

					ApplyMesh();
				}

				var total     = Detail + 1;
				var positions = new Vector3[total];
				var coords1   = new Vector2[total];
				var indices   = new int[Detail * 3];
				var angleStep = (Mathf.PI * 2.0f) / Detail;
				var noiseStep = 0.0f;

				if (Noise == true && NoisePoints > 0)
				{
					SgtHelper.BeginRandomSeed(NoiseSeed);
					{
						points.Clear();

						for (var i = 0; i < NoisePoints; i++)
						{
							points.Add(Random.value);
						}

						noiseStep = NoisePoints / (float)Detail;
					}
					SgtHelper.EndRandomSeed();
				}

				// Write center vertices
				positions[0] = Vector3.zero;
				coords1[0] = Vector2.zero;

				// Write outer vertices
				for (var point = 0; point < Detail; point++)
				{
					var angle = angleStep * point;
					var x     = Mathf.Sin(angle);
					var y     = Mathf.Cos(angle);
					var r     = Radius;

					if (Wave == true)
					{
						var waveAngle = (angle + WavePhase * Mathf.Deg2Rad) * WavePoints;

						r += Mathf.Pow(Mathf.Cos(waveAngle) * 0.5f + 0.5f, WavePower * WavePower) * WaveStrength;
					}

					if (Noise == true && NoisePoints > 0)
					{
						var noise  = Mathf.Repeat(noiseStep * point + NoisePhase, NoisePoints);
						//var noise = point * noiseStep + NoisePhase;
						var index  = (int)noise;
						var frac   = noise % 1.0f;
						var pointA = points[(index + 0) % NoisePoints];
						var pointB = points[(index + 1) % NoisePoints];
						var pointC = points[(index + 2) % NoisePoints];
						var pointD = points[(index + 3) % NoisePoints];

						r += SgtHelper.CubicInterpolate(pointA, pointB, pointC, pointD, frac) * NoiseStrength;
					}

					// Write outer vertices
					var v = point + 1;

					positions[v] = new Vector3(x * r, y * r, 0.0f);
					coords1[v] = new Vector2(1.0f, 0.0f);
				}

				for (var tri = 0; tri < Detail; tri++)
				{
					var i  = tri * 3;
					var v0 = tri + 1;
					var v1 = tri + 2;

					if (v1 >= total)
					{
						v1 = 1;
					}

					indices[i + 0] = 0;
					indices[i + 1] = v0;
					indices[i + 2] = v1;
				}

				generatedMesh.Clear(false);
				generatedMesh.vertices  = positions;
				generatedMesh.uv        = coords1;
				generatedMesh.triangles = indices;
				generatedMesh.RecalculateNormals();
				generatedMesh.RecalculateBounds();
			}
		}

		[ContextMenu("Apply Mesh")]
		public void ApplyMesh()
		{
			if (cachedFlareSet == false)
			{
				cachedFlare    = GetComponent<SgtFlare>();
				cachedFlareSet = true;
			}

			if (cachedFlare.Mesh != generatedMesh)
			{
				cachedFlare.Mesh = generatedMesh;

				cachedFlare.UpdateMesh();
			}
		}

		[ContextMenu("Remove Mesh")]
		public void RemoveMesh()
		{
			if (cachedFlareSet == false)
			{
				cachedFlare    = GetComponent<SgtFlare>();
				cachedFlareSet = true;
			}

			if (cachedFlare.Mesh == generatedMesh)
			{
				cachedFlare.Mesh = null;

				cachedFlare.UpdateMesh();
			}
		}

		protected virtual void OnEnable()
		{
			UpdateMesh();
			ApplyMesh();
		}

		protected virtual void OnDisable()
		{
			RemoveMesh();
		}

		protected virtual void OnDestroy()
		{
			if (generatedMesh != null)
			{
				generatedMesh.Clear(false);

				SgtObjectPool<Mesh>.Add(generatedMesh);
			}
		}
	}
}