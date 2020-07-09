using Pathfinding;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class UnitMovementPathSystem : JobComponentSystem
{
	private EntityQuery _entityQuery;

	private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

	protected override void OnCreate()
	{
		_endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

		var queryDesc = new EntityQueryDesc
		{
			All = new[] {typeof(UnitMovement), ComponentType.ReadOnly<Unit>(), ComponentType.ReadOnly<MoveTo>()}
		};

		_entityQuery = GetEntityQuery(queryDesc);

		base.OnCreate();
	}

	private struct UnitStartMovementJob : IJobChunk
	{
		[ReadOnly] public ArchetypeChunkEntityType EntityType;

		[ReadOnly] public ArchetypeChunkComponentType<MoveTo> UnitMoveToType;

		public ArchetypeChunkComponentType<UnitMovement> UnitMovementType;

		public EntityCommandBuffer.Concurrent EntityCommandBuffer;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			NativeArray<UnitMovement> movements = chunk.GetNativeArray(UnitMovementType);

			NativeArray<MoveTo> unitMoveTo = chunk.GetNativeArray(UnitMoveToType);

			NativeArray<Entity> entities = chunk.GetNativeArray(EntityType);

			for (var i = 0; i < movements.Length; i++)
			{
				var move   = movements[i];
				var moveTo = unitMoveTo[i];

				if (move.CurrentCellCoord.x != moveTo.Coord.x || move.CurrentCellCoord.y != moveTo.Coord.y)
				{
					EntityCommandBuffer.AddComponent<Path>(chunkIndex, entities[i]);

					EntityCommandBuffer.SetComponent(chunkIndex, entities[i], new Path
					{
						StartCoord = move.CurrentCellCoord,
						GoalCoord  = moveTo.Coord,
						InProgress = true,
						Reachable  = false
					});

					EntityCommandBuffer.AddBuffer<PathNode>(chunkIndex, entities[i]);
					EntityCommandBuffer.AddBuffer<SearchNode>(chunkIndex, entities[i]);

					movements[i] = UnitMovement.Reset(move);

#if UNITY_EDITOR
					EntityCommandBuffer.AddBuffer<DebugNode>(chunkIndex, entities[i]);
#endif
				}

				EntityCommandBuffer.RemoveComponent<MoveTo>(chunkIndex, entities[i]);
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var unitMovementType = GetArchetypeChunkComponentType<UnitMovement>();
		var unitMoveToType   = GetArchetypeChunkComponentType<MoveTo>(true);

		ArchetypeChunkEntityType entityType = GetArchetypeChunkEntityType();

		var job = new UnitStartMovementJob
		{
			EntityType          = entityType,
			UnitMovementType    = unitMovementType,
			UnitMoveToType      = unitMoveToType,
			EntityCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
		};

		JobHandle jobHandle = job.Schedule(_entityQuery, inputDeps);

		_endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

		return jobHandle;
	}
}