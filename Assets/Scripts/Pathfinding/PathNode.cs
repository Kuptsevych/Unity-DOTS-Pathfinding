using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding
{
	public struct PathNode : IBufferElementData
	{
		public int2 Coord;
	}
}