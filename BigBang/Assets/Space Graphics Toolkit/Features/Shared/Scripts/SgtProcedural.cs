using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	public class SgtProcedural_Editor<T> : SgtEditor<T>
		where T : SgtProcedural
	{
		protected override void OnInspector()
		{
			DrawDefault("Generate", "This allows you to control when this component will be generated.");

			if (Any(t => t.Generate == SgtProcedural.GenerateType.WithFixedSeed))
			{
				DrawDefault("Seed", "The seed used for automatic generation.");
			}

			Separator();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component is the basis for all procedural components in SGT.</summary>
	public abstract class SgtProcedural : MonoBehaviour
	{
		public enum GenerateType
		{
			Automatically,
			WithRandomSeed,
			WithFixedSeed,
			Manually
		}

		/// <summary>This allows you to control when this component will be generated.</summary>
		public GenerateType Generate;

		/// <summary>The seed used for automatic generation.</summary>
		public SgtSeed Seed;

		/// <summary>This method allows you to manually generate this component with the specified seed.</summary>
		public void GenerateWith(int seed)
		{
			SgtHelper.BeginRandomSeed(seed);
			{
				DoGenerate();
			}
			SgtHelper.EndRandomSeed();
		}

		[ContextMenu("Generate Now")]
		public void GenerateNow()
		{
			switch (Generate)
			{
				case GenerateType.Automatically:
				{
					DoGenerate();
				}
				break;

				case GenerateType.WithRandomSeed:
				{
					var randomSeed = Random.Range(int.MinValue, int.MaxValue);

					GenerateWith(randomSeed);
				}
				break;

				case GenerateType.WithFixedSeed:
				{
					GenerateWith(Seed);
				}
				break;

				case GenerateType.Manually:
				{
					DoGenerate();
				}
				break;
			}
		}

		protected abstract void DoGenerate();

		protected virtual void Awake()
		{
			switch (Generate)
			{
				case GenerateType.Automatically:
				{
					DoGenerate();
				}
				break;

				case GenerateType.WithRandomSeed:
				{
					var randomSeed = Random.Range(int.MinValue, int.MaxValue);

					GenerateWith(randomSeed);
				}
				break;

				case GenerateType.WithFixedSeed:
				{
					GenerateWith(Seed);
				}
				break;
			}
		}
	}
}