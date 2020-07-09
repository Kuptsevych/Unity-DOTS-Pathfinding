using Unity.Entities;
using Unity.Mathematics;

public struct UnitMovement : IComponentData
{
	public float  Speed;
	public int    PathPositionIndex;
	public int2   TargetCellCoord;
	public int2   CurrentCellCoord;
	public float  StepProgress;
	public float  WaitTimer;
	public float  MoveTime;
	public bool   IsMoving;
	public float3 TargetTransformPosition;
	public float3 PrevTransformPosition;

	public static UnitMovement Reset(UnitMovement source)
	{
		return new UnitMovement
		{
			Speed                   = source.Speed,
			PathPositionIndex       = 0,
			TargetCellCoord         = source.TargetCellCoord,
			CurrentCellCoord        = source.CurrentCellCoord,
			StepProgress            = 0f,
			WaitTimer               = 0f,
			MoveTime                = 0f,
			IsMoving                = false,
			TargetTransformPosition = source.TargetTransformPosition,
			PrevTransformPosition   = source.PrevTransformPosition,
		};
	}
}