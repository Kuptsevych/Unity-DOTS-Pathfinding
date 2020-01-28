using Unity.Entities;
using Unity.Mathematics;

public struct UnitController : IComponentData
{
	public int    LastClickIndex;
	public int2   TargetCellCoord;
	public int2   CurrentCellCoord;
	public bool   IsMoving;
	public float3 TargetTransformPosition;
	public float3 PrevTransformPosition;
	public bool   IsTransformSync;
	public bool   NewTarget;
	public int    PathPos;
	public float  Speed;
	public bool   RefreshClick;
	public float  MoveTime;
	public bool   SelectionEnabled;
	public Entity SelectionEntity;
	public float  WaitTimer;
}