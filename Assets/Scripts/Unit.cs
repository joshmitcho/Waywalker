using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum State { zero, moving };

    public enum MovementType { walking, flying, swimming };

    public string unitName;

    public MovementType movementType = MovementType.walking;
    public int movementSpeed = 30;
    public int remainingMovement;

    public int attackRange = 5;
    public Dice attackPower = new Dice(2, 6, 3);

    public int initiativeBonus = 0;
    public int initiative;

    public int tileX;
    public int tileY;
    public ClickableTile occupyingTile;

    public TileMap map;

    public List<Node> currentPath = null;
    public State state = State.zero;

    private void Awake()
    {
        ResetTurnValues();
    }

    public void ResetTurnValues()
    {
        remainingMovement = movementSpeed;
    }
    
    public void MoveNextTile(int cost)
    {
        if (currentPath == null) return;

        state = State.moving;

        //remove first node from currentPath
        //we do this before moving because the first node is the one we're already on
        currentPath.RemoveAt(0);

        //now we move to the new first node in currentPath (visually)
        StartCoroutine(MoveToPosition(map.TileCoordToWorldCoord(currentPath[0].x, currentPath[0].y), 0.1f, cost));

        if (tileX > currentPath[0].x)
        {
            GetComponentInChildren<SpriteRenderer>().flipX = true;
        } else if (tileX < currentPath[0].x)
        {
            GetComponentInChildren<SpriteRenderer>().flipX = false;
        }

        //update unit's X and Y in the data
        tileX = currentPath[0].x;
        tileY = currentPath[0].y;

        if (currentPath.Count == 1)
        {
            //then we're standing on our final destination: we're done
            remainingMovement -= cost;
            currentPath = null;
            state = State.zero;
        }
    }

    IEnumerator MoveToPosition(Vector3 targetPos, float timeToMove, int cost)
    {
        Vector3 startPos = transform.position;
        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        if (currentPath == null) //if movement is complete...
        {
            map.ClearCurrentPath();
            map.state = TileMap.State.zero;
            map.Dijkstra(this);
            map.GenerateMovementSet();
            map.NewToolTip(occupyingTile);
        }
        else //if there's still movement left to do...
        {
            MoveNextTile(cost);
        }
    }

}
