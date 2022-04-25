using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{

    public int tileX;
    public int tileY;

    public TileMap map;

    public bool withinMovementSet = true;

    private void OnMouseOver()
    {
        if (withinMovementSet && map.state != TileMap.State.unitMoving)
        {
            map.GeneratePathTo(tileX, tileY);
        }
    }

    private void OnMouseUp()
    {
        if (map.state != TileMap.State.unitMoving)
        {
            map.MoveUnit();
        }
    }
}
