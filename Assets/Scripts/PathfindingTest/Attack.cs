using Unity.Entities;
using Unity.Mathematics;

public struct Attack : IComponentData
{
	public int2   DetectedCoord;
	public Entity EntityToAttack;
	//todo attack type
}