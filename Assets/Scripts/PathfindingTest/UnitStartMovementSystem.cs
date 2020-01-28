using Pathfinding;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class UnitStartMovementSystem : JobComponentSystem
{
	private EntityQuery _entityQuery;

	private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

	protected override void OnCreate()
	{
		_endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

		var queryDesc = new EntityQueryDesc()
		{
			None = new ComponentType[] {typeof(Path)},
			All  = new ComponentType[] {typeof(UnitController), ComponentType.ReadOnly<Unit>()}
		};

		_entityQuery = GetEntityQuery(queryDesc);

		base.OnCreate();
	}

	private struct UnitStartMovementJob : IJobChunk
	{
		[ReadOnly] public ArchetypeChunkEntityType                    EntityType;
		
		public ArchetypeChunkComponentType<UnitController> UnitControllerType;

		public EntityCommandBuffer.Concurrent EntityCommandBuffer;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			NativeArray<UnitController> unitControllers = chunk.GetNativeArray(UnitControllerType);
			NativeArray<Entity>         entities        = chunk.GetNativeArray(EntityType);
			
			for (var i = 0; i < unitControllers.Length; i++)
			{
				var unitController = unitControllers[i];

				if (unitController.NewTarget)
				{
					unitController.NewTarget = false;

					unitController.PathPos = 0;

					EntityCommandBuffer.AddComponent<Path>(chunkIndex, entities[i]);

					EntityCommandBuffer.SetComponent(chunkIndex, entities[i], new Path
					{
						StartCoord = unitController.CurrentCellCoord,
						GoalCoord  = unitController.TargetCellCoord,
						InProgress = true,
						Reachable  = false
					});

					EntityCommandBuffer.AddBuffer<PathNode>(chunkIndex, entities[i]);
					EntityCommandBuffer.AddBuffer<SearchNode>(chunkIndex, entities[i]);

#if UNITY_EDITOR
					EntityCommandBuffer.AddBuffer<DebugNode>(chunkIndex, entities[i]);
#endif
				}

				unitControllers[i] = unitController;
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var unitControllerType = GetArchetypeChunkComponentType<UnitController>(false);

		ArchetypeChunkEntityType entityType = GetArchetypeChunkEntityType();

		var job = new UnitStartMovementJob
		{
			EntityType          = entityType,
			UnitControllerType  = unitControllerType,
			EntityCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
		};

		JobHandle jobHandle = job.Schedule(_entityQuery, inputDeps);

		_endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

		return jobHandle;
	}
}