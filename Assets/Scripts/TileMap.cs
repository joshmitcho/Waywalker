using System.Collections;
using UnityEngine;

public class TileMap : MonoBehaviour
{

    public GameObject selectedUnit;
    public TileType[] tileTypes;

    int[,] tiles;

    int mapSizeX = 10;
    int mapSizeY = 10;

    private void Start()
    {
        GenerateMapData();
        GenerateMapVisual();
    }

    void GenerateMapData()
    {
        //Allocate our map tiles
        tiles = new int[mapSizeX, mapSizeY];

        int x, y;

        //Initialize map tiles to be grass (0 = grass)
        for (x = 0; x < mapSizeX; x++)
        {
            for (y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = 0;
            }
        }

        //swamp area _1 = swamp)
        for (x = 3; x <= 5; x++)
        {
            for (y = 0; y < 4; y++)
            {
                tiles[x, y] = 1;
            }
        }

        //u-shaped mountain range (2 = mountain)
        tiles[4, 4] = 2;
        tiles[5, 4] = 2;
        tiles[6, 4] = 2;
        tiles[7, 4] = 2;
        tiles[8, 4] = 2;

        tiles[4, 5] = 2;
        tiles[4, 6] = 2;
        tiles[8, 5] = 2;
        tiles[8, 6] = 2;
    }

    void GenerateMapVisual()
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                //spawn visual prefabs
                TileType tt = tileTypes[tiles[x, y]];
                GameObject go = Instantiate(tt.tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity);

                ClickableTile ct = go.GetComponent<ClickableTile>();
                ct.tileX = x;
                ct.tileY = y;
            }
        }
    }

    public void MoveUnitTo(int x, int y)
    {

    }

}
