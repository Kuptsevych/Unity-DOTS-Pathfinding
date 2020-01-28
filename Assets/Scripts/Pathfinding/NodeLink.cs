using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding
{
	public struct NodeLink : IBufferElementData
	{
		public int2 LinkedEntityCoord;
	}
}