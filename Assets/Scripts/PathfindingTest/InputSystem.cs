using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class InputSystem : ComponentSystem
{
	private EntityQuery _entityQuery;

	private Camera _mainCamera;

	private int _clickIndex1;
	private int _clickIndex2;

	protected override void OnCreate()
	{
		EntityArchetype archetypeNode = EntityManager.CreateArchetype(typeof(InputClickComponent));

		EntityManager.CreateEntity(archetypeNode);

		_entityQuery = GetEntityQuery(typeof(InputClickComponent));

		_entityQuery.SetSingleton(new InputClickComponent
		{
			Index           = 0,
			ClickCoord      = new int2(0, 0),
			ClickWorldCoord = new float3(0, 0, 0)
		});
	}

	protected override void OnStartRunning()
	{
		_mainCamera = Camera.main;
	}

	protected override void OnUpdate()
	{
		if (_mainCamera == null) return;

		if (Input.GetMouseButtonDown(0))
		{
			_clickIndex1++;
			Vector2 mousePosition = Input.mousePosition;

			_entityQuery.SetSingleton(new InputClickComponent
			{
				Index           = _clickIndex1,
				Index2          = _clickIndex2,
				ClickCoord      = new int2((int) mousePosition.x, (int) mousePosition.y),
				ClickWorldCoord = _mainCamera.ScreenToWorldPoint(mousePosition)
			});
		}

		if (Input.GetMouseButtonDown(1))
		{
			_clickIndex2++;
			Vector2 mousePosition = Input.mousePosition;

			_entityQuery.SetSingleton(new InputClickComponent
			{
				Index           = _clickIndex1,
				Index2          = _clickIndex2,
				ClickCoord      = new int2((int) mousePosition.x, (int) mousePosition.y),
				ClickWorldCoord = _mainCamera.ScreenToWorldPoint(mousePosition)
			});
		}
	}
}