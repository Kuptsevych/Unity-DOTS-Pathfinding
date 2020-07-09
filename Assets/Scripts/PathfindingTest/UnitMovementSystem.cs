using Pathfinding;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

[UpdateAfter(typeof(SelectSystem))]
public class UnitMovementSystem : ComponentSystem
{
	private EntityQuery _entityQuery;
	private EntityQuery _worldQuery;

	private BufferFromEntity<PathNode> _pathBuffer;

	private MapDataSystem _mapDataSystem;
	private Grid          _grid;

	protected override void OnCreate()
	{
		var queryDesc = new EntityQueryDesc
		{
			All = new[]
			{
				typeof(UnitMovement),
				ComponentType.ReadOnly<Unit>(),
				ComponentType.ReadOnly<Path>()
			}
		};

		_entityQuery = GetEntityQuery(queryDesc);

		_worldQuery = GetEntityQuery(typeof(Grid), typeof(Tilemap));
	}

	protected override void OnStartRunning()
	{
		_mapDataSystem = EntityManager.World.GetExistingSystem<MapDataSystem>();
		_grid          = _worldQuery.ToComponentArray<Grid>()[0];
	}

	protected override void OnUpdate()
	{
		var units    = _entityQuery.ToComponentDataArray<Unit>(Allocator.TempJob);
		var pathes   = _entityQuery.ToComponentDataArray<Path>(Allocator.TempJob);
		var entities = _entityQuery.ToEntityArray(Allocator.TempJob);
		var movement = _entityQuery.ToComponentDataArray<UnitMovement>(Allocator.TempJob);

		_pathBuffer = GetBufferFromEntity<PathNode>(true);

		float dt = Time.DeltaTime;

		for (int i = 0; i < entities.Length; i++)
		{
			var path = pathes[i];
			var move = movement[i];

			if (!path.InProgress)
			{
				if (path.Reachable)
				{
					if (move.IsMoving)
					{
						move.StepProgress += dt * move.Speed;

						if (move.StepProgress >= 1f)
						{
							SwitchMapData(move, units[i], entities[i], move.TargetCellCoord);
							move.StepProgress     = 1f;
							move.IsMoving         = false;
							move.CurrentCellCoord = move.TargetCellCoord;
						}
					}
					else
					{
						DynamicBuffer<PathNode> pathNodeBuffer = _pathBuffer[entities[i]];

						int pathPosIndex = math.max(pathNodeBuffer.Length - 1 - move.PathPositionIndex, 0);

						PathNode pathPos = pathNodeBuffer[pathPosIndex];

						if (move.CurrentCellCoord.x != pathPos.Coord.x || move.CurrentCellCoord.y != pathPos.Coord.y)
						{
							var mapData = _mapDataSystem.MapData[pathPos.Coord];

							if (mapData.ContentEntity != Entity.Null)
							{
								move.WaitTimer += dt;

								if (move.WaitTimer > 1f)
								{
									PostUpdateCommands.RemoveComponent<Path>(entities[i]);
								}

								movement[i] = move;
								continue;
							}

							move.WaitTimer               =  0;
							move.IsMoving                =  true;
							move.StepProgress            =  0f;
							move.TargetCellCoord         =  pathPos.Coord;
							move.TargetTransformPosition =  _grid.CellToWorld(new Vector3Int(move.TargetCellCoord.x, move.TargetCellCoord.y, 0));
							move.PathPositionIndex       += 1;
						}
						else
						{
							if (move.PathPositionIndex < pathNodeBuffer.Length - 1)
							{
								move.PathPositionIndex += 1;
							}
							else
							{
								PostUpdateCommands.RemoveComponent<Path>(entities[i]);
							}
						}
					}
				}
				else
				{
					PostUpdateCommands.RemoveComponent<Path>(entities[i]);
				}
			}
			else
			{
				move = UnitMovement.Reset(move);
			}

			movement[i] = move;
		}

		_entityQuery.CopyFromComponentDataArray(movement);

		pathes.Dispose();
		entities.Dispose();
		units.Dispose();
		movement.Dispose();
	}

	private void SwitchMapData(UnitMovement unitMovement, Unit unit, Entity unitEntity, int2 targetCoord)
	{
		var currentData = _mapDataSystem.MapData[unitMovement.CurrentCellCoord];

		currentData.Fraction      = Fractions.NEUTRAL;
		currentData.ContentType   = CellContentTypes.EMPTY;
		currentData.ContentEntity = Entity.Null;

		_mapDataSystem.MapData[unitMovement.CurrentCellCoord] = currentData;

		var nextData = _mapDataSystem.MapData[targetCoord];

		nextData.Fraction      = unit.Fraction;
		nextData.ContentEntity = unitEntity;
		nextData.ContentType   = CellContentTypes.UNIT;

		_mapDataSystem.MapData[targetCoord] = nextData;
	}
}