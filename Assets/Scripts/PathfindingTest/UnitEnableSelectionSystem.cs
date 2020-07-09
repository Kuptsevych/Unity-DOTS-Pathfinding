using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class UnitEnableSelectionSystem : ComponentSystem
{
	private EntityQuery _entityQuery;

	protected override void OnCreate()
	{
		var queryDesc = new EntityQueryDesc
		{
			All = new[]
			{
				typeof(UnitController),
				ComponentType.ReadOnly<Unit>(),
				ComponentType.ReadOnly<Initialized>(),
				ComponentType.ReadOnly<Selected>()
			}
		};

		_entityQuery = GetEntityQuery(queryDesc);
	}

	protected override void OnUpdate()
	{
		//todo system to process player selection changes (player selected visual changes)
		
		var unitControllers = _entityQuery.ToComponentDataArray<UnitController>(Allocator.TempJob);

		for (int i = 0; i < unitControllers.Length; i++)
		{
			var unitController = unitControllers[i];

			if (!unitController.SelectionEnabled)
			{
				unitController.SelectionEnabled = true;

				var selection = EntityManager.GetComponentObject<SpriteRenderer>(unitController.SelectionEntity);
				selection.enabled = true;
			}

			unitControllers[i] = unitController;
		}

		_entityQuery.CopyFromComponentDataArray(unitControllers);

		unitControllers.Dispose();
	}
}