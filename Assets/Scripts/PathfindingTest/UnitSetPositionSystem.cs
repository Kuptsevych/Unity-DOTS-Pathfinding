using Pathfinding;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitSetPositionSystem : ComponentSystem
{
	private EntityQuery _entityQuery;
	private EntityQuery _worldQuery;

	private Grid _grid;

	protected override void OnCreate()
	{
		var queryDesc = new EntityQueryDesc()
		{
			All = new ComponentType[] {typeof(Translation), typeof(UnitController), typeof(Unit), typeof(Path)}
		};

		_entityQuery = GetEntityQuery(queryDesc);

		_worldQuery = GetEntityQuery(typeof(Grid), typeof(Tilemap));
	}

	protected override void OnStartRunning()
	{
		_grid = _worldQuery.ToComponentArray<Grid>()[0];

		var unitControllers = _entityQuery.ToComponentDataArray<UnitController>(Allocator.TempJob);

		for (int i = 0; i < unitControllers.Length; i++)
		{
			var unitController = unitControllers[i];

			unitController.MoveTime = 0;

			unitControllers[i] = unitController;
		}

		_entityQuery.CopyFromComponentDataArray(unitControllers);

		unitControllers.Dispose();
	}

	protected override void OnUpdate()
	{
		var unitControllers = _entityQuery.ToComponentDataArray<UnitController>(Allocator.TempJob);

		var entities = _entityQuery.ToEntityArray(Allocator.TempJob);

		var transforms = _entityQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

		float dt = Time.DeltaTime;

		for (int i = 0; i < unitControllers.Length; i++)
		{
			var unitController = unitControllers[i];

			if (!unitController.IsMoving) continue;

			var transform = transforms[i];

			if (unitController.IsTransformSync)
			{
				unitController.MoveTime += dt * unitController.Speed;

				transform.Value = math.lerp(unitController.PrevTransformPosition, unitController.TargetTransformPosition, unitController.MoveTime);

				if (unitController.MoveTime >= 1f)
				{
					unitController.MoveTime        = 0;
					transform.Value                = new float3(unitController.TargetTransformPosition.x, unitController.TargetTransformPosition.y, -1);
					unitController.IsTransformSync = false;
					unitController.IsMoving        = false;

					Vector3Int transformCellCoord = _grid.WorldToCell(transform.Value);

					unitController.CurrentCellCoord = new int2(transformCellCoord.x, transformCellCoord.y);
				}
			}
			else
			{
				Vector3Int transformCellCoord = _grid.WorldToCell(transform.Value);

				if (transformCellCoord.x != unitController.TargetCellCoord.x || transformCellCoord.y != unitController.TargetCellCoord.y)
				{
					unitController.TargetTransformPosition =
						_grid.CellToWorld(new Vector3Int(unitController.TargetCellCoord.x, unitController.TargetCellCoord.y, 0));
					unitController.PrevTransformPosition = transform.Value;
					unitController.IsTransformSync       = true;
				}
				else
				{
					unitController.IsMoving = false;
				}
			}

			unitControllers[i] = unitController;
			transforms[i]      = transform;
		}

		_entityQuery.CopyFromComponentDataArray(unitControllers);
		_entityQuery.CopyFromComponentDataArray(transforms);

		transforms.Dispose();
		unitControllers.Dispose();
		entities.Dispose();
	}
}