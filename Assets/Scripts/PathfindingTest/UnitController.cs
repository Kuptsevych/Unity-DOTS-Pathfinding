using Unity.Entities;

public struct UnitController : IComponentData
{
	public bool   SelectionEnabled;
	public Entity SelectionEntity;
}