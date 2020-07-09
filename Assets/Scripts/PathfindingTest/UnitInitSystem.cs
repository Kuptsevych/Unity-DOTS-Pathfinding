using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class UnitInitSystem : ComponentSystem
{
	private EntityQuery _entityQuery;

	private MapDataSystem _mapDataSystem;

	protected override void OnCreate()
	{
		var queryDesc = new EntityQueryDesc
		{
			None = new[] {ComponentType.ReadOnly<Initialized>()},
			All  = new[] {ComponentType.ReadOnly<UnitMovement>(), ComponentType.ReadOnly<Unit>()}
		};

		_entityQuery = GetEntityQuery(queryDesc);
	}

	protected override void OnStartRunning()
	{
		_mapDataSystem = EntityManager.World.GetExistingSystem<MapDataSystem>();
	}

	protected override void OnUpdate()
	{
		var units       = _entityQuery.ToComponentDataArray<Unit>(Allocator.TempJob);
		var movements = _entityQuery.ToComponentDataArray<UnitMovement>(Allocator.TempJob);

		var entities = _entityQuery.ToEntityArray(Allocator.TempJob);

		for (var i = 0; i < units.Length; i++)
		{
			PostUpdateCommands.AddComponent<Initialized>(entities[i]);

			var unit       = units[i];
			var move = movements[i];

			if (_mapDataSystem.MapData.TryGetValue(move.CurrentCellCoord, out var cellData))
			{
				cellData.ContentType   = CellContentTypes.UNIT;
				cellData.Fraction      = unit.Fraction;
				cellData.ContentEntity = entities[i];

				_mapDataSystem.MapData[move.CurrentCellCoord] = cellData;
			}
			else
			{
				Debug.LogError("MapData for unit coord not found::" + move.CurrentCellCoord);
			}
		}

		_entityQuery.CopyFromComponentDataArray(units);
		_entityQuery.CopyFromComponentDataArray(movements);

		units.Dispose();
		movements.Dispose();
		entities.Dispose();
	}
}