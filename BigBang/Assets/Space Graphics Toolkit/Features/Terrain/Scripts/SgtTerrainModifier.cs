using UnityEngine;

namespace SpaceGraphicsToolkit
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtTerrain))]
	public abstract class SgtTerrainModifier : MonoBehaviour
	{
		[System.NonSerialized]
		protected SgtTerrain terrain;

		[System.NonSerialized]
		protected bool terrainSet;

		public void Rebuild()
		{
			if (terrainSet == false)
			{
				terrain    = GetComponent<SgtTerrain>();
				terrainSet = true;
			}

			terrain.DelayRebuild = true;
		}

		public void UpdateRenderers()
		{
			if (terrainSet == false)
			{
				terrain    = GetComponent<SgtTerrain>();
				terrainSet = true;
			}

			terrain.DelayUpdateRenderers = true;
		}

		public void UpdateColliders()
		{
			if (terrainSet == false)
			{
				terrain    = GetComponent<SgtTerrain>();
				terrainSet = true;
			}

			terrain.DelayUpdateColliders = true;
		}
	}
}