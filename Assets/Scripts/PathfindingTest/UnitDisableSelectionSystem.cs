using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class UnitDisableSelectionSystem : ComponentSystem
{
	private EntityQuery _entityQuery;

	protected override void OnCreate()
	{
		var queryDesc = new EntityQueryDesc
		{
			None = new[]
			{
				ComponentType.ReadOnly<Selected>()
			},
			All = new[]
			{
				typeof(UnitController),
				ComponentType.ReadOnly<Unit>(),
				ComponentType.ReadOnly<Initialized>(),
			}
		};

		_entityQuery = GetEntityQuery(queryDesc);
	}

	protected override void OnUpdate()
	{
		var unitControllers = _entityQuery.ToComponentDataArray<UnitController>(Allocator.TempJob);

		for (int i = 0; i < unitControllers.Length; i++)
		{
			var unitController = unitControllers[i];

			if (unitController.SelectionEnabled)
			{
				unitController.SelectionEnabled = false;

				var selection = EntityManager.GetComponentObject<SpriteRenderer>(unitController.SelectionEntity);
				selection.enabled = false;
			}

			unitControllers[i] = unitController;
		}

		_entityQuery.CopyFromComponentDataArray(unitControllers);

		unitControllers.Dispose();
	}
}