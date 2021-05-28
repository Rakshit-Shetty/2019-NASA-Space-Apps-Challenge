namespace SpaceGraphicsToolkit
{
	[System.Serializable]
	public struct SgtTerrainNeighbour
	{
		public int            A;
		public int            B;
		public int            C;
		public int            D;
		public int            I;
		public int            O;
		public int            Z;
		public SgtTerrainFace Face;

		public void Set(SgtTerrainFace newFace, int newI, int newA, int newB, int newO, int newC, int newD, int newZ)
		{
			Face = newFace;
			I    = newI;
			A    = newA;
			B    = newB;
			O    = newO;
			C    = newC;
			D    = newD;
			Z    = newZ;
		}
	}
}