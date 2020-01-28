using Pathfinding;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateAfter(typeof(SelectSystem))]
public class UnitMovementSystem : ComponentSystem
{
	private EntityQuery _entityQuery;

	private BufferFromEntity<PathNode> _pathBuffer;

	private MapDataSystem _mapDataSystem;

	protected override void OnCreate()
	{
		var queryDesc = new EntityQueryDesc()
		{
			All = new [] {typeof(UnitController), ComponentType.ReadOnly<Unit>(), ComponentType.ReadOnly<Path>()}
		};

		_entityQuery = GetEntityQuery(queryDesc);
	}

	protected override void OnStartRunning()
	{
		_mapDataSystem = EntityManager.World.GetExistingSystem<MapDataSystem>();
	}

	protected override void OnUpdate()
	{
		var unitControllers = _entityQuery.ToComponentDataArray<UnitController>(Allocator.TempJob);
		var units           = _entityQuery.ToComponentDataArray<Unit>(Allocator.TempJob);
		var pathes          = _entityQuery.ToComponentDataArray<Path>(Allocator.TempJob);
		var entities        = _entityQuery.ToEntityArray(Allocator.TempJob);

		_pathBuffer = GetBufferFromEntity<PathNode>(true);

		float dt = Time.DeltaTime;

		for (int i = 0; i < unitControllers.Length; i++)
		{
			var unitController = unitControllers[i];
			var path           = pathes[i];

			if (unitController.IsMoving) continue;

			if (!path.InProgress)
			{
				if (path.Reachable)
				{
					DynamicBuffer<PathNode> pathNodeBuffer = _pathBuffer[entities[i]];

					int pathPosIndex = math.max(pathNodeBuffer.Length - 1 - unitController.PathPos, 0);

					PathNode pathPos = pathNodeBuffer[pathPosIndex];

					if (unitController.CurrentCellCoord.x != pathPos.Coord.x || unitController.CurrentCellCoord.y != pathPos.Coord.y)
					{
						var mapData = _mapDataSystem.MapData[pathPos.Coord];

						if (mapData.ContentEntity != Entity.Null)
						{
							unitController.WaitTimer += dt;

							if (unitController.WaitTimer > 1f)
							{
								PostUpdateCommands.RemoveComponent<Path>(entities[i]);
								unitController.RefreshClick = true;
							}

							unitControllers[i] = unitController;
							continue;
						}

						unitController.WaitTimer       =  0;
						unitController.IsMoving        =  true;
						unitController.TargetCellCoord =  pathPos.Coord;
						unitController.PathPos         += 1;

						SwitchMapData(unitController, units[i], entities[i], pathPos.Coord);
					}
					else
					{
						if (unitController.PathPos < pathNodeBuffer.Length - 1)
						{
							unitController.PathPos += 1;
						}
						else
						{
							PostUpdateCommands.RemoveComponent<Path>(entities[i]);
							unitController.RefreshClick = true;
						}
					}
				}
				else
				{
					PostUpdateCommands.RemoveComponent<Path>(entities[i]);
					unitController.RefreshClick = true;
				}
			}

			unitControllers[i] = unitController;
		}

		_entityQuery.CopyFromComponentDataArray(unitControllers);

		unitControllers.Dispose();
		pathes.Dispose();
		entities.Dispose();
		units.Dispose();
	}

	private void SwitchMapData(UnitController unitController, Unit unit, Entity unitEntity, int2 targetCoord)
	{
		var currentData = _mapDataSystem.MapData[unitController.CurrentCellCoord];

		currentData.Fraction      = Fractions.NEUTRAL;
		currentData.ContentType   = CellContentTypes.EMPTY;
		currentData.ContentEntity = Entity.Null;

		_mapDataSystem.MapData[unitController.CurrentCellCoord] = currentData;

		var nextData = _mapDataSystem.MapData[targetCoord];

		nextData.Fraction      = unit.Fraction;
		nextData.ContentEntity = unitEntity;
		nextData.ContentType   = CellContentTypes.UNIT;

		_mapDataSystem.MapData[targetCoord] = nextData;
	}
}