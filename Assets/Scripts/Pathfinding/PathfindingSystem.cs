using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Pathfinding
{
	public class PathfindingSystem : JobComponentSystem
	{
		private EntityQuery _entityQuery;

		private NativeHashMap<int2, NodeWithEntity> NodesMap;

		private NativeHashMap<int, int2> _closeSet;
		private NativeHashMap<int, int2> _openSet;

		private JobHandle _jobHandle;

		private struct NodeWithEntity
		{
			public Node   Node;
			public Entity Entity;
		}

		private const int StraightDist = 10;
		private const int DiagonalDist = 14;

		protected override void OnCreate()
		{
			var queryDesc = new EntityQueryDesc
			{
				All = new[] {ComponentType.ReadOnly<Node>()}
			};

			_entityQuery = GetEntityQuery(queryDesc);
		}

		protected override void OnStartRunning()
		{
			var nodes    = _entityQuery.ToComponentDataArray<Node>(Allocator.TempJob);
			var entities = _entityQuery.ToEntityArray(Allocator.TempJob);

			NodesMap = new NativeHashMap<int2, NodeWithEntity>(nodes.Length, Allocator.Persistent);

			for (var i = 0; i < nodes.Length; i++)
			{
				NodesMap.TryAdd(nodes[i].Coord, new NodeWithEntity
				{
					Node = nodes[i], Entity = entities[i]
				});
			}

			nodes.Dispose();
			entities.Dispose();
		}

#if !UNITY_EDITOR
		[BurstCompile(FloatPrecision.Low, FloatMode.Fast)]
#endif
		private struct PathfindingJob : IJobForEachWithEntity<Path>
		{
			[NativeDisableParallelForRestriction] public BufferFromEntity<SearchNode> SearchNodeBuffer;
			[NativeDisableParallelForRestriction] public BufferFromEntity<NodeLink>   NodeLinkBuffer;
			[NativeDisableParallelForRestriction] public BufferFromEntity<PathNode>   PathNodeBuffer;

#if UNITY_EDITOR
			[NativeDisableParallelForRestriction] public BufferFromEntity<DebugNode> DebugNodeBuffer;
#endif

			[NativeDisableParallelForRestriction] public NativeHashMap<int, int2> CloseSet;
			[NativeDisableParallelForRestriction] public NativeHashMap<int, int2> OpenSet;

			[ReadOnly] public NativeHashMap<int2, NodeWithEntity> Nodes;

			public void Execute(Entity entity, int index, ref Path path)
			{
				if (!path.InProgress) return;

				CloseSet.Clear();
				OpenSet.Clear();

				//setp 1
				DynamicBuffer<SearchNode> searchNodeBuffer = SearchNodeBuffer[entity];
#if UNITY_EDITOR
				DynamicBuffer<DebugNode> debugNodeBuffer = DebugNodeBuffer[entity];
#endif
				//step2
				searchNodeBuffer.Add(new SearchNode
				{
					Coord             = path.StartCoord,
					HeapIndex         = 0,
					PrevNode          = path.StartCoord,
					HeuristicDistance = GetHeuristicPathLength(path.StartCoord, path.GoalCoord),
					DistanceFromStart = 0
				});
				OpenSet.TryAdd(path.StartCoord.GetHashCode(), path.StartCoord);

				while (searchNodeBuffer.Length > 0)
				{
					//step 3
					int2 currentNode = RemoveFirst(ref searchNodeBuffer, out int2 prevCoord, out int prevDistanceFromStart);
#if UNITY_EDITOR
					debugNodeBuffer.Add(new DebugNode
					{
						Coord     = currentNode,
						PrevCoord = prevCoord
					});
#endif

					//step 4
					if (currentNode.Equals(path.GoalCoord))
					{
						DynamicBuffer<PathNode> pathNodeBuffer = PathNodeBuffer[entity];

						pathNodeBuffer.Add(new PathNode {Coord = currentNode});

						int2 pathNode = currentNode;

						while (!path.StartCoord.Equals(pathNode))
						{
							if (CloseSet.TryGetValue(pathNode.GetHashCode(), out int2 prevNode))
							{
								pathNodeBuffer.Add(new PathNode {Coord = prevNode});
								pathNode = prevNode;
							}
							else
							{
								path.InProgress = false;
								return;
							}
						}

						path.Reachable  = true;
						path.InProgress = false;

						searchNodeBuffer.Clear();
						OpenSet.Clear();
						CloseSet.Clear();
						return;
					}

					//step 5
					if (Nodes.TryGetValue(currentNode, out NodeWithEntity nodeWithEntity))
					{
						DynamicBuffer<NodeLink> nodeLinkBuffer = NodeLinkBuffer[nodeWithEntity.Entity];

						// calculate heuristic distance for neighbour nodes
						// add neighbour nodes to open set buffer (binary heap)
						for (var i = 0; i < nodeLinkBuffer.Length; i++)
						{
							var nodeLink = nodeLinkBuffer[i];

							if (Nodes.TryGetValue(nodeLink.LinkedEntityCoord, out NodeWithEntity neighbourNodeWithEntity))
							{
								if (!neighbourNodeWithEntity.Node.Walkable ||
								    CloseSet.ContainsKey(neighbourNodeWithEntity.Node.Coord.GetHashCode()))
								{
									continue;
								}
							}
							else
							{
								continue;
							}

							// if open set not contain node, add it
							if (!OpenSet.ContainsKey(nodeLink.LinkedEntityCoord.GetHashCode()))
							{
								OpenSet.TryAdd(nodeLink.LinkedEntityCoord.GetHashCode(), nodeLink.LinkedEntityCoord);

								int heapIndex = searchNodeBuffer.Length;
								searchNodeBuffer.Add(new SearchNode
								{
									HeapIndex         = heapIndex,
									Coord             = nodeLink.LinkedEntityCoord,
									HeuristicDistance = GetHeuristicPathLength(nodeLink.LinkedEntityCoord, path.GoalCoord),
									PrevNode          = currentNode,
									DistanceFromStart = prevDistanceFromStart + GetHeuristicPathLength(nodeLink.LinkedEntityCoord, currentNode)
								});

								SortUp(ref searchNodeBuffer, heapIndex);
							}
						}
					}
				}

				path.InProgress = false;
			}

			private void SortUp(ref DynamicBuffer<SearchNode> searchNodeBuffer, int nodeIndex)
			{
				int index = nodeIndex;

				while (index > 0)
				{
					int parentIndex = (index - 1) / 2;

					SearchNode node       = searchNodeBuffer[index];
					SearchNode parentItem = searchNodeBuffer[parentIndex];

					while (true)
					{
						if (node.HeuristicDistance + node.DistanceFromStart < parentItem.HeuristicDistance + parentItem.DistanceFromStart)
						{
							Swap(ref searchNodeBuffer, node, parentItem);
							node        = searchNodeBuffer[parentIndex];
							parentIndex = (parentIndex - 1) / 2;
							parentItem  = searchNodeBuffer[parentIndex];
						}
						else
						{
							index = (index - 1) / 2;
							break;
						}
					}
				}
			}

			private void Swap(ref DynamicBuffer<SearchNode> searchNodeBuffer, SearchNode itemA, SearchNode itemB)
			{
				int indexA = itemA.HeapIndex;
				int indexB = itemB.HeapIndex;

				itemA.HeapIndex = indexB;
				itemB.HeapIndex = indexA;

				searchNodeBuffer[indexA] = itemB;
				searchNodeBuffer[indexB] = itemA;
			}

			private void SortDown(ref DynamicBuffer<SearchNode> searchNodeBuffer, int index = 0)
			{
				if (searchNodeBuffer.Length == 0) return;

				SearchNode node = searchNodeBuffer[index];

				int firstChild = -1;

				while (true)
				{
					int childIndexLeft  = node.HeapIndex * 2 + 1;
					int childIndexRight = node.HeapIndex * 2 + 2;
					int swapIndex       = 0;

					if (childIndexLeft < searchNodeBuffer.Length &&
					    searchNodeBuffer[childIndexLeft].HeuristicDistance + searchNodeBuffer[childIndexLeft].DistanceFromStart <
					    node.HeuristicDistance                             + node.DistanceFromStart)
					{
						swapIndex = childIndexLeft;
					}

					if (childIndexRight < searchNodeBuffer.Length &&
					    searchNodeBuffer[childIndexRight].HeuristicDistance + searchNodeBuffer[childIndexRight].DistanceFromStart <
					    node.HeuristicDistance                              + node.DistanceFromStart)
					{
						swapIndex = childIndexRight;
					}

					if (swapIndex != 0)
					{
						if (firstChild < 0) firstChild = swapIndex;
						Swap(ref searchNodeBuffer, node, searchNodeBuffer[swapIndex]);
						node = searchNodeBuffer[swapIndex];
					}
					else
					{
						if (firstChild >= 0) SortDown(ref searchNodeBuffer, firstChild);
						return;
					}
				}
			}

			private int2 RemoveFirst(ref DynamicBuffer<SearchNode> searchNodeBuffer, out int2 prevCoord, out int prevDistanceFromStart)
			{
				int2 firstItem = searchNodeBuffer[0].Coord;

				prevCoord = searchNodeBuffer[0].PrevNode;

				prevDistanceFromStart = searchNodeBuffer[0].DistanceFromStart;

				CloseSet.TryAdd(firstItem.GetHashCode(), searchNodeBuffer[0].PrevNode);

				int lastIndex = searchNodeBuffer.Length - 1;

				var lastNode = searchNodeBuffer[lastIndex];

				lastNode.HeapIndex = 0;

				searchNodeBuffer[0] = lastNode;

				searchNodeBuffer.RemoveAt(lastIndex);

				SortDown(ref searchNodeBuffer);

				return firstItem;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (!_jobHandle.IsCompleted) return default;

			if (_closeSet.IsCreated) _closeSet.Dispose();
			if (_openSet.IsCreated) _openSet.Dispose();

			_closeSet = new NativeHashMap<int, int2>(NodesMap.Count(), Allocator.TempJob);
			_openSet  = new NativeHashMap<int, int2>(NodesMap.Count(), Allocator.TempJob);

			var pathfindingJob = new PathfindingJob
			{
				SearchNodeBuffer = GetBufferFromEntity<SearchNode>(false),
				NodeLinkBuffer   = GetBufferFromEntity<NodeLink>(true),
				PathNodeBuffer   = GetBufferFromEntity<PathNode>(false),
#if UNITY_EDITOR
				DebugNodeBuffer = GetBufferFromEntity<DebugNode>(false),
#endif
				Nodes    = NodesMap,
				CloseSet = _closeSet,
				OpenSet  = _openSet
			};

			_jobHandle = pathfindingJob.Schedule(this, inputDeps);

			return _jobHandle;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (_closeSet.IsCreated) _closeSet.Dispose();
			if (_openSet.IsCreated) _openSet.Dispose();
			NodesMap.Dispose();
		}

		private static int GetHeuristicPathLength(int2 from, int2 to)
		{
			int dx = math.abs(from.x - to.x);
			int dy = math.abs(from.y - to.y);

			int delta = math.abs(dx - dy);

			return DiagonalDist * math.min(dx, dy) + delta * StraightDist;
		}
	}
}