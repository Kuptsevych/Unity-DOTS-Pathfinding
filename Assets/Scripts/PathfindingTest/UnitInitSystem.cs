using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class UnitInitSystem : ComponentSystem
{
	private EntityQuery _entityQuery;

	private MapDataSystem _mapDataSystem;

	protected override void OnCreate()
	{
		var queryDesc = new EntityQueryDesc()
		{
			None = new ComponentType[] {typeof(Initialized)},
			All  = new ComponentType[] {typeof(UnitController), ComponentType.ReadOnly<Unit>()}
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
		var controllers = _entityQuery.ToComponentDataArray<UnitController>(Allocator.TempJob);

		var entities = _entityQuery.ToEntityArray(Allocator.TempJob);

		for (var i = 0; i < units.Length; i++)
		{
			PostUpdateCommands.AddComponent<Initialized>(entities[i]);

			var unit       = units[i];
			var controller = controllers[i];

			if (_mapDataSystem.MapData.TryGetValue(controller.CurrentCellCoord, out var cellData))
			{
				cellData.ContentType   = CellContentTypes.UNIT;
				cellData.Fraction      = unit.Fraction;
				cellData.ContentEntity = entities[i];

				_mapDataSystem.MapData[controller.CurrentCellCoord] = cellData;
			}
			else
			{
				Debug.LogError("MapData for unit coord not found::" + controller.CurrentCellCoord);
			}
		}

		_entityQuery.CopyFromComponentDataArray(units);
		_entityQuery.CopyFromComponentDataArray(controllers);

		units.Dispose();
		controllers.Dispose();
		entities.Dispose();
	}
}