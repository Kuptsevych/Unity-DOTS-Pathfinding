#if UNITY_EDITOR
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Pathfinding
{
	public class DebugDrawPathSystem : ComponentSystem
	{
		private EntityQuery _entityQuery;

		protected override void OnCreate()
		{
			_entityQuery = GetEntityQuery(typeof(Path));
		}

		protected override void OnUpdate()
		{
			var pathes   = _entityQuery.ToComponentDataArray<Path>(Allocator.TempJob);
			var entities = _entityQuery.ToEntityArray(Allocator.TempJob);

			for (var i = 0; i < pathes.Length; i++)
			{
				var path = pathes[i];

				if (!path.InProgress && path.Reachable)
				{
					DynamicBuffer<PathNode> buffer = EntityManager.GetBuffer<PathNode>(entities[i]);

					foreach (PathNode nodeLink in buffer)
					{
						var x = nodeLink.Coord.x * 1.28f;
						var y = nodeLink.Coord.y * 1.28f;

						Debug.DrawLine(new Vector3(x - 0.15f, y + 0.15f, -1.1f), new Vector3(x + 0.15f, y - 0.15f, -1.1f), Color.cyan);

						Debug.DrawLine(new Vector3(x + 0.15f, y + 0.15f, -1.1f), new Vector3(x - 0.15f, y - 0.15f, -1.1f), Color.cyan);
					}

					DynamicBuffer<DebugNode> debugBuffer = EntityManager.GetBuffer<DebugNode>(entities[i]);

					for (var index = 0; index < Mathf.Clamp(PathfindingTest.DebugNodeIndexValue, 0, debugBuffer.Length); index++)
					{
						DebugNode debugNode = debugBuffer[index];
						var       x         = debugNode.Coord.x * 1.28f;
						var       y         = debugNode.Coord.y * 1.28f;

						var px = debugNode.PrevCoord.x * 1.28f;
						var py = debugNode.PrevCoord.y * 1.28f;

						Debug.DrawLine(new Vector3(x - 0.1f, y + 0.1f, -1.1f), new Vector3(x + 0.1f, y - 0.1f, -1.1f), Color.magenta);

						Debug.DrawLine(new Vector3(x + 0.1f, y + 0.1f, -1.1f), new Vector3(x - 0.1f, y - 0.1f, -1.1f), Color.magenta);

						Debug.DrawLine(new Vector3(x + 0.1f, y + 0.1f, -1.1f), new Vector3(px - 0.1f, py - 0.1f, -1.1f), Color.magenta);
					}
				}

				var sx = path.StartCoord.x * 1.28f;
				var sy = path.StartCoord.y * 1.28f;

				var gx = path.GoalCoord.x * 1.28f;
				var gy = path.GoalCoord.y * 1.28f;

				Debug.DrawLine(new Vector3(sx - 0.15f, sy, -1.3f), new Vector3(sx + 0.15f, sy, -1.3f), Color.red);

				Debug.DrawLine(new Vector3(sx, sy - 0.15f, -1.3f), new Vector3(sx, sy + 0.15f, -1.3f), Color.red);

				Debug.DrawLine(new Vector3(gx - 0.15f, gy, -1.3f), new Vector3(gx + 0.15f, gy, -1.3f), Color.blue);

				Debug.DrawLine(new Vector3(gx, gy - 0.15f, -1.3f), new Vector3(gx, gy + 0.15f, -1.3f),
					Color.blue);
			}

			_entityQuery.CopyFromComponentDataArray(pathes);

			pathes.Dispose();
			entities.Dispose();
		}
	}
}
#endif