using Pathfinding;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapDataSystem : ComponentSystem
{
	public NativeHashMap<int2, CellData> MapData;

	private Grid _grid;

	public struct CellData
	{
		public int2   Coord;
		public int    NodeIndex;
		public int    CellType;
		public int    ContentType;
		public Entity ContentEntity;
		public int    Fraction;
	}

	private EntityQuery _entityQuery;
	private EntityQuery _worldQuery;

	protected override void OnCreate()
	{
		_entityQuery = GetEntityQuery(typeof(Node));
		_worldQuery  = GetEntityQuery(typeof(Grid), typeof(Tilemap));
	}

	protected override void OnStartRunning()
	{
		_grid = _worldQuery.ToComponentArray<Grid>()[0];
		var tilemap = _worldQuery.ToComponentArray<Tilemap>()[0];

		EntityArchetype archetypeNode = EntityManager.CreateArchetype(typeof(Node));

		BoundsInt  bounds   = tilemap.cellBounds;
		TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

		var cellsCount = bounds.size.x * bounds.size.y;

		MapData = new NativeHashMap<int2, CellData>(cellsCount, Allocator.Persistent);

		int nodeIndex = 0;

		for (int x = 0; x < bounds.size.x; x++)
		{
			for (int y = 0; y < bounds.size.y; y++)
			{
				TileBase tile;
				TilemapHelper.TryGetTile(x, y, bounds.size.x, bounds.size.y, ref allTiles, out tile);

				int2 coord = new int2(x, y);

				if (tile == null)
				{
					var entity = EntityManager.CreateEntity(archetypeNode);

					EntityManager.SetComponentData(entity, new Node
					{
						Coord    = coord,
						Walkable = true
					});

					EntityManager.AddBuffer<NodeLink>(entity);

					DynamicBuffer<NodeLink> buffer = EntityManager.GetBuffer<NodeLink>(entity);

					var neighbouringTiles = TilemapHelper.GetNeighbouringTiles(x, y, bounds.size.x, bounds.size.y, ref allTiles, true);

					foreach (var neighbouringTile in neighbouringTiles)
					{
						buffer.Add(new NodeLink {LinkedEntityCoord = neighbouringTile});
					}

					MapData.TryAdd(coord, new CellData
					{
						Coord       = coord,
						NodeIndex   = nodeIndex,
						CellType    = CellTypes.GROUND,
						ContentType = CellContentTypes.EMPTY,
						Fraction    = Fractions.NEUTRAL
					});

					nodeIndex++;
				}
				else
				{
					MapData.TryAdd(coord, new CellData
					{
						Coord       = coord,
						NodeIndex   = -1,
						CellType    = CellTypes.WALL,
						ContentType = CellContentTypes.EMPTY,
						Fraction    = Fractions.NEUTRAL
					});
				}
			}
		}
	}

	protected override void OnUpdate()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		MapData.Dispose();
	}
}