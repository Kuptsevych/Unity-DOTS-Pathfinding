using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitControllerSystem : ComponentSystem
{
	private EntityQuery _entityQuery;
	private EntityQuery _inputQuery;
	private EntityQuery _worldQuery;

	private Grid _grid;

	private float3 _shift;

	protected override void OnCreate()
	{
		_inputQuery = GetEntityQuery(typeof(InputClickComponent));

		_worldQuery = GetEntityQuery(typeof(Grid), typeof(Tilemap));

		var queryDesc = new EntityQueryDesc
		{
			//	None = new ComponentType[] {typeof(Path)},
			All = new ComponentType[]
				{typeof(UnitController), ComponentType.ReadOnly<Unit>(), ComponentType.ReadOnly<Initialized>(), ComponentType.ReadOnly<MoveTo>(),}
		};

		_entityQuery = GetEntityQuery(queryDesc);
	}

	protected override void OnStartRunning()
	{
		_grid = _worldQuery.ToComponentArray<Grid>()[0];
		var tilemap = _worldQuery.ToComponentArray<Tilemap>()[0];

		Vector3 cellSize   = tilemap.cellSize;
		Vector3 cellAnchor = tilemap.tileAnchor;

		_shift = new float3(cellSize.x * cellAnchor.x, cellSize.y * cellAnchor.y, cellSize.z * cellAnchor.z);
	}

	protected override void OnUpdate()
	{
		var input = _inputQuery.GetSingleton<InputClickComponent>();

		var unitControllers = _entityQuery.ToComponentDataArray<UnitController>(Allocator.TempJob);
		var movetos         = _entityQuery.ToComponentDataArray<MoveTo>(Allocator.TempJob);
		var entities        = _entityQuery.ToEntityArray(Allocator.TempJob);

		for (int i = 0; i < unitControllers.Length; i++)
		{
			var unitController = unitControllers[i];
			var moveto         = movetos[i];

			if (unitController.TargetCellCoord.x != moveto.Coord.x || unitController.TargetCellCoord.y != moveto.Coord.y)
			{
				unitController.TargetCellCoord = new int2(moveto.Coord.x, moveto.Coord.y);
				unitController.NewTarget       = true;
			}

			PostUpdateCommands.RemoveComponent<MoveTo>(entities[i]);

			unitControllers[i] = unitController;
		}

		_entityQuery.CopyFromComponentDataArray(unitControllers);

		unitControllers.Dispose();
		movetos.Dispose();
		entities.Dispose();
	}
}