using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{

    public int tileX;
    public int tileY;

    public int costToFinishHere;

    public TileMap map;

    bool withinMovementSet;

    public bool isWalkable = true;
    public int movementCost = 5;
    public Unit occupyingUnit;

    Color highlightColour = new Color(58, 162, 190, 134);

    private void Awake()
    {
        occupyingUnit = null;
        withinMovementSet = false;
    }

    private void OnMouseEnter()
    {
        if (map.state != TileMap.State.unitMoving)
        {
            map.GeneratePathTo(tileX, tileY);
        }
    }

    private void OnMouseExit()
    {
        map.ClearCurrentPath();
    }

    public void AddToMovementSet()
    {
        highlightColour.a = 0.3f;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = highlightColour;
        withinMovementSet = true;
    }

    public void RemoveFromAllSets()
    {
        highlightColour.a = 0f;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = highlightColour;
        withinMovementSet = false;
    }

    private void OnMouseUp()
    {
        if (!withinMovementSet)
        {
            Debug.Log("can't move here");
        }
        else if (map.state != TileMap.State.unitMoving)
        {
            map.MoveUnit(tileX, tileY, costToFinishHere);
        }
    }
}
