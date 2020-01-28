using Unity.Entities;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelInit : MonoBehaviour, IConvertGameObjectToEntity
{
	public Grid    WorldGrid;
	public Tilemap WorldTilemap;

	void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddComponentObject(entity, WorldGrid);
		dstManager.AddComponentObject(entity, WorldTilemap);
	}
}