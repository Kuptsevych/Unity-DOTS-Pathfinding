using Unity.Entities;
using Unity.Mathematics;

public struct MoveTo : IComponentData
{
	public int2 Coord;
}