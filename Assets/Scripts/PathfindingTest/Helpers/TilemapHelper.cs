using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Tilemaps;

public static class TilemapHelper
{
	public static bool TryGetTile(int x, int y, int xSize, int ySize, ref TileBase[] allTiles, out TileBase tile)
	{
		if (!IsCoordValid(x, y, xSize, ySize))
		{
			tile = null;
			return false;
		}

		tile = allTiles[GetTileIndex(x, y, xSize)];
		return true;
	}

	public static bool IsCoordValid(int x, int y, int xSize, int ySize)
	{
		if (x < 0 || y < 0 || x >= xSize || y >= ySize) return false;

		return true;
	}

	public static int GetTileIndex(int x, int y, int xSize)
	{
		int index = x + y * xSize;
		return index;
	}

	public static List<int2> GetNeighbouringTiles(int x, int y, int xSize, int ySize, ref TileBase[] allTiles, bool eight = false)
	{
		var result = new List<int2>();

		int2 topCoord  = new int2(x, y + 1);
		int2 downCoord = new int2(x, y - 1);

		int2 leftCoord  = new int2(x - 1, y);
		int2 rightCoord = new int2(x + 1, y);

		if (TryGetTile(topCoord.x,   topCoord.y,   xSize, ySize, ref allTiles, out var topTile)   && topTile   == null) result.Add(topCoord);
		if (TryGetTile(downCoord.x,  downCoord.y,  xSize, ySize, ref allTiles, out var downTile)  && downTile  == null) result.Add(downCoord);
		if (TryGetTile(leftCoord.x,  leftCoord.y,  xSize, ySize, ref allTiles, out var leftTile)  && leftTile  == null) result.Add(leftCoord);
		if (TryGetTile(rightCoord.x, rightCoord.y, xSize, ySize, ref allTiles, out var rightTile) && rightTile == null) result.Add(rightCoord);

		if (eight)
		{
			int2 topLeftCoord  = new int2(x - 1, y + 1);
			int2 topRightCoord = new int2(x + 1, y + 1);

			int2 downLeftCoord  = new int2(x - 1, y - 1);
			int2 downRightCoord = new int2(x + 1, y - 1);

			if (TryGetTile(topLeftCoord.x, topLeftCoord.y, xSize, ySize, ref allTiles, out var topLeftTile) && topLeftTile == null)
				result.Add(topLeftCoord);
			if (TryGetTile(topRightCoord.x, topRightCoord.y, xSize, ySize, ref allTiles, out var topRightTile) && topRightTile == null)
				result.Add(topRightCoord);
			if (TryGetTile(downLeftCoord.x, downLeftCoord.y, xSize, ySize, ref allTiles, out var downLeftTile) && downLeftTile == null)
				result.Add(downLeftCoord);
			if (TryGetTile(downRightCoord.x, downRightCoord.y, xSize, ySize, ref allTiles, out var downRightTile) && downRightTile == null)
				result.Add(downRightCoord);
		}

		return result;
	}
}