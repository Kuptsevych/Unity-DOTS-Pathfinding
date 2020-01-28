#if UNITY_EDITOR
using Pathfinding;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class DebugDrawNodesSystem : ComponentSystem
{
	private EntityQuery _entityQuery;

	protected override void OnCreate()
	{
		_entityQuery = GetEntityQuery(typeof(Node));
	}

	protected override void OnUpdate()
	{
		var nodes    = _entityQuery.ToComponentDataArray<Node>(Allocator.TempJob);
		var entities = _entityQuery.ToEntityArray(Allocator.TempJob);

		for (var i = 0; i < nodes.Length; i++)
		{
			var node = nodes[i];

			var x = node.Coord.x * 1.28f;
			var y = node.Coord.y * 1.28f;

			//Debug.DrawLine(new Vector3(x - 0.25f, y, -1), new Vector3(x + 0.25f, y, -1));

			//Debug.DrawLine(new Vector3(x, y - 0.25f, -1), new Vector3(x, y + 0.25f, -1));

			float width = 0.5f;
			
			Debug.DrawLine(new Vector3(x - width, y - width, -1), new Vector3(x + width, y - width, -1));
			Debug.DrawLine(new Vector3(x - width, y + width, -1), new Vector3(x + width, y + width, -1));
			
			Debug.DrawLine(new Vector3(x - width, y - width, -1), new Vector3(x - width, y + width, -1));
			Debug.DrawLine(new Vector3(x + width, y - width, -1), new Vector3(x + width, y + width, -1));

			DynamicBuffer<NodeLink> buffer = EntityManager.GetBuffer<NodeLink>(entities[i]);

			foreach (NodeLink nodeLink in buffer)
			{
				var lx = nodeLink.LinkedEntityCoord.x * 1.28f;
				var ly = nodeLink.LinkedEntityCoord.y * 1.28f;

				Debug.DrawLine(new Vector3(x, y, -1.1f), new Vector3(lx, ly, -1.1f), new Color(1f, 1f, 0, 0.1f));
			}
		}

		_entityQuery.CopyFromComponentDataArray(nodes);

		nodes.Dispose();
		entities.Dispose();
	}
}
#endif