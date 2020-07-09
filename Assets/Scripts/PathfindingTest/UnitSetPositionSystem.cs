using Pathfinding;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class UnitSetPositionSystem : SystemBase
{
	protected override void OnUpdate()
	{
		Entities.WithoutBurst().WithAll<Initialized, Unit, Path>().ForEach((Entity e, ref Translation translation, ref UnitMovement movement) =>
		{
			if (movement.IsMoving)
			{
				translation.Value = math.lerp(movement.PrevTransformPosition, movement.TargetTransformPosition, movement.StepProgress);
			}
			else
			{
				movement.PrevTransformPosition = translation.Value;
			}
		}).Run();
	}
}