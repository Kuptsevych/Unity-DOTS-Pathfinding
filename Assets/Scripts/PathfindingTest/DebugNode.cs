#if UNITY_EDITOR
using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding
{
	public struct DebugNode : IBufferElementData
	{
		public int2 Coord;
		public int2 PrevCoord;
	}
}
#endif