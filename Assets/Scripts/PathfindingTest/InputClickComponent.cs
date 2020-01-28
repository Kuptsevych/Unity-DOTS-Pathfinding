using Unity.Entities;
using Unity.Mathematics;

public struct InputClickComponent : IComponentData
{
	public int    Index;
	public int    Index2;
	public int2   ClickCoord;
	public float3 ClickWorldCoord;
}