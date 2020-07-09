using Pathfinding;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectSystem : ComponentSystem
{
	private EntityQuery _entityQuery;
	private EntityQuery _inputQuery;
	private EntityQuery _worldQuery;
	private EntityQuery _selectedQuery;

	private Grid _grid;

	private float3 _shift;

	private MapDataSystem _mapDataSystem;

	private int _selectedFraction;

	protected override void OnCreate()
	{
		EntityArchetype archetypeNode = EntityManager.CreateArchetype(typeof(SelectSingletoneComponent));

		EntityManager.CreateEntity(archetypeNode);

		_entityQuery = GetEntityQuery(typeof(SelectSingletoneComponent));

		_entityQuery.SetSingleton(new SelectSingletoneComponent
		{
			InputIndex = 0,
		});

		_inputQuery = GetEntityQuery(typeof(InputClickComponent));

		_worldQuery = GetEntityQuery(typeof(Grid), typeof(Tilemap));

		_selectedQuery = GetEntityQuery(typeof(Selected));
	}

	protected override void OnStartRunning()
	{
		var grids = _worldQuery.ToComponentArray<Grid>();

		if (grids.Length == 0) return;
		
		_grid = grids[0];
		var tilemap = _worldQuery.ToComponentArray<Tilemap>()[0];

		Vector3 cellSize   = tilemap.cellSize;
		Vector3 cellAnchor = tilemap.tileAnchor;

		_shift = new float3(cellSize.x * cellAnchor.x, cellSize.y * cellAnchor.y, cellSize.z * cellAnchor.z);

		_mapDataSystem = EntityManager.World.GetExistingSystem<MapDataSystem>();
	}

	protected override void OnUpdate()
	{
		var input = _inputQuery.GetSingleton<InputClickComponent>();

		var select = _entityQuery.GetSingleton<SelectSingletoneComponent>();

		//todo OnHoverUICheck

		if (select.InputIndex != input.Index)
		{
			select.InputIndex = input.Index;
			_entityQuery.SetSingleton(select);

			var selectedCount = _selectedQuery.CalculateEntityCount();

			var selectedCellCoord = _grid.WorldToCell(input.ClickWorldCoord + _shift);

			int2 coord = new int2(selectedCellCoord.x, selectedCellCoord.y);

			if (_mapDataSystem.MapData.TryGetValue(coord, out var cellData))
			{
				if (selectedCount == 0)
				{
					if (IsSelectable(cellData.ContentType))
					{
						PostUpdateCommands.AddComponent<Selected>(cellData.ContentEntity);
					}
				}
				else
				{
					if (IsSelectable(cellData.ContentType))
					{
						//todo checks
						// check if selected buildins
						// check if can attack
						// check if can pickup or any other actions by default
						// check if click again

						//check fraction if non equal attack else select instead

						if (cellData.Fraction != Fractions.NEUTRAL && cellData.Fraction != _selectedFraction)
						{
							AttackSelected(cellData, EntityManager);
						}
						else
						{
							//TODO unselect on second click
							//ReleaseSelected();
							PostUpdateCommands.AddComponent<Selected>(cellData.ContentEntity);
						}
					}
					else
					{
						if (IsWalkable(cellData.CellType))
						{
							MoveSelected(cellData.Coord, EntityManager);
						}
						else
						{
							ReleaseSelected();
						}
					}
				}
			}
		}

		if (select.InputIndex2 != input.Index2)
		{
			select.InputIndex2 = input.Index2;
			_entityQuery.SetSingleton(select);

			ReleaseSelected();
		}
	}

	private void AttackSelected(MapDataSystem.CellData target, EntityManager entityManager)
	{
		var entities = _selectedQuery.ToEntityArray(Allocator.TempJob);

		for (var index = 0; index < entities.Length; index++)
		{
			Entity entity = entities[index];

			if (!entityManager.HasComponent<Attack>(entity))
			{
				PostUpdateCommands.AddComponent<Attack>(entity);
			}

			PostUpdateCommands.SetComponent(entity, new Attack
			{
				DetectedCoord  = target.Coord,
				EntityToAttack = target.ContentEntity
			});
		}

		entities.Dispose();
	}

	private void MoveSelected(int2 coord, EntityManager entityManager)
	{
		var entities = _selectedQuery.ToEntityArray(Allocator.TempJob);

		for (var index = 0; index < entities.Length; index++)
		{
			Entity entity = entities[index];

			if (entityManager.HasComponent<Path>(entity))
			{
				PostUpdateCommands.RemoveComponent<Path>(entity);
			}

			if (!entityManager.HasComponent<MoveTo>(entity))
			{
				PostUpdateCommands.AddComponent<MoveTo>(entity);
			}

			PostUpdateCommands.SetComponent(entity, new MoveTo
			{
				Coord = coord
			});
		}

		entities.Dispose();
	}

	private void ReleaseSelected()
	{
		_selectedFraction = Fractions.NEUTRAL;

		var entities = _selectedQuery.ToEntityArray(Allocator.TempJob);

		for (var index = 0; index < entities.Length; index++)
		{
			Entity entity = entities[index];
			PostUpdateCommands.RemoveComponent<Selected>(entity);
		}

		entities.Dispose();
	}

	private bool IsSelectable(int typeId)
	{
		return typeId == CellContentTypes.UNIT || typeId == CellContentTypes.BUILDING;
	}

	private bool IsWalkable(int typeId)
	{
		return typeId == CellTypes.GROUND;
	}
}