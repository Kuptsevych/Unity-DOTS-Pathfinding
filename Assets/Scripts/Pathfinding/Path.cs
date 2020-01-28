using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding
{
	public struct Path : IComponentData
	{
		public int2 StartCoord;
		public int2 GoalCoord;
		public bool InProgress;
		public bool Reachable;
	}
}