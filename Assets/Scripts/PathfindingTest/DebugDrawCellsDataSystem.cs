using Pathfinding;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

#if UNITY_EDITOR
public class DebugDrawCellsDataSystem : ComponentSystem
{
	private EntityQuery _entityQuery;

	private MapDataSystem _mapDataSystem;

	protected override void OnCreate()
	{
		_entityQuery = GetEntityQuery(typeof(Node));
	}

	protected override void OnStartRunning()
	{
		_mapDataSystem = EntityManager.World.GetExistingSystem<MapDataSystem>();
	}

	protected override void OnUpdate()
	{
		return;
		var nodes = _entityQuery.ToComponentDataArray<Node>(Allocator.TempJob);

		for (var i = 0; i < nodes.Length; i++)
		{
			var node = nodes[i];

			var mapData = _mapDataSystem.MapData[node.Coord];

			float x = node.Coord.x * 1.28f;
			float y = node.Coord.y * 1.28f;

			if (mapData.ContentEntity == Entity.Null)
			{
				Debug.DrawLine(new Vector3(x + 0.25f, y - 0.25f, -10), new Vector3(x + 0.25f, y + 0.25f, -1), Color.cyan);
			}
			else
			{
				Debug.DrawLine(new Vector3(x + 0.25f, y - 0.25f, -10), new Vector3(x + 0.25f, y + 0.25f, -1), Color.red);
			}
		}

		nodes.Dispose();
	}
}
#endif