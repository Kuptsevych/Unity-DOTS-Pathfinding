using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding
{
	public struct SearchNode : IBufferElementData
	{
		public int2 Coord;
		public int  HeapIndex;
		public int2 PrevNode;
		public int  HeuristicDistance;
		public int  DistanceFromStart;
	}
}