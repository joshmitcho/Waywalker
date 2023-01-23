using System;

[Serializable]
public class WWMap
{
    public string spritesheet;
    public int pixelsWide;
    public int pixelsHigh;
    public int width;
    public int height;
    public int tileSize;
    public string[] baseLayer;
    public string[] topLayer;
    public TileType[] tileTypes;
}

[Serializable]
public class TileType
{
    public int[] numbers;
    public string tileType;
    public bool isWalkable;
    public int movementCost;
    public string spritesheet;
    public int numSprites;
    public int rotation;

}
