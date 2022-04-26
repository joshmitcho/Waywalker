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

    public int tileX;
    public int tileY;

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

    private void OnMouseUp()
    {
        Debug.Log("Click " + unitName);


    }

    private void Update()
    {
        if (currentPath != null)
        {
            int i = 0;

            //Debug.Log("currentPath count: " + currentPath.Count);

            while (i < currentPath.Count - 1)
            {
                Vector3 start = map.TileCoordToWorldCoord(currentPath[i].x, currentPath[i].y);
                Vector3 end = map.TileCoordToWorldCoord(currentPath[i + 1].x, currentPath[i + 1].y);
                i++;

                Debug.DrawLine(start, end);
            }
        }
    }

    public void MoveNextTile(int cost)
    {
        if (currentPath == null) return;

        state = State.moving;

        //remove first node from currentPath
        //we do this before moving because the first node is the one we're already on
        currentPath.RemoveAt(0);

        //now we move to the new first node in currentPath (visually)
        StartCoroutine(MoveToPosition(transform, map.TileCoordToWorldCoord(currentPath[0].x, currentPath[0].y), 0.1f, cost));

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

    public IEnumerator MoveToPosition(Transform transform, Vector3 position, float timeToMove, int cost)
    {
        var currentPos = transform.position;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;
            transform.position = Vector3.Lerp(currentPos, position, t);
            yield return null;
        }

        if (currentPath == null) //if movement is complete...
        {
            map.state = TileMap.State.zero;
            map.GenerateMovementSet(this);
        }
        else //if there's still movement left to do...
        {
            MoveNextTile(cost);
        }
    }

}
