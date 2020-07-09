using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Pathfinding
{
	public class UnitAuthoring : MonoBehaviour, IConvertGameObjectToEntity
	{
		public int   Id;
		public float Speed = 1f;
		public Grid  WorldGrid;
		public int   Fraction;

		public SpriteRenderer _selectionSprite;


		void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			var cellCoord = WorldGrid.WorldToCell(transform.position);

			Debug.Log("Unit coord::" + cellCoord);

			int2 currentCellCoord = new int2(cellCoord.x, cellCoord.y);

			dstManager.AddComponentData(entity, new Unit
			{
				Id       = Id,
				Fraction = Fraction
			});

			dstManager.AddComponentData(entity, new CopyTransformToGameObject());

			var selectionArchetype = dstManager.CreateArchetype(typeof(SpriteRenderer), typeof(Selection));

			var selectionEntity = dstManager.CreateEntity(selectionArchetype);

			dstManager.AddComponentData(selectionEntity, new Selection
			{
				LinkedUnit = entity
			});

			dstManager.AddComponentObject(selectionEntity, _selectionSprite);
			dstManager.AddComponentObject(selectionEntity, _selectionSprite.transform);

			dstManager.AddComponentData(entity, new UnitController
			{
				SelectionEntity = selectionEntity
			});

			dstManager.AddComponentData(entity, new UnitMovement
			{
				Speed            = Speed,
				CurrentCellCoord = currentCellCoord,
			});
		}
	}
}