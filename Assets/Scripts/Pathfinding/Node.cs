using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding
{
	public struct Node : IComponentData
	{
		public int2 Coord;
		public bool Walkable;
	}
}