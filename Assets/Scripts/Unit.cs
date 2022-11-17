using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum State { zero, moving };

    public enum MovementType { walking, flying, swimming };

    public string unitName;

    public Color diceColour;
    public Color numColour;

    public MovementType movementType = MovementType.walking;
    public int movementSpeed = 30;
    public int currentMovement;

    public int HP;
    public int currentHP;

    public int AC;
    public int currentAC;

    public int STR;
    public int DEX;
    public int CON;
    public int INT;
    public int WIS;
    public int CHA;

    public Weapon mainHand;
    public int numAttackActions = 1;
    public int remainingAttackActions;

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
        InitiateUnitValues();
        mainHand = new Weapon("Greataxe", "Two-handed Axe", new Attack(5, new Dice(1, 20, 0), new Dice(2, 6, -1)));
        
    }

    public void InitiateUnitValues()
    {
        currentAC = AC;
        currentHP = HP;
        ResetTurnValues();
    }

    public int GetModifier(int score)
    {
        float output = (score - 10) / 2f;

        if (output >= 0)
            return (int)output;
        else
            return (int)(output - 0.5f);
    }

    public int GetAttackRange()
    {
        return mainHand.primaryAttack.range;
    }

    public Tuple<int, int[]> Attack()
    {
        Tuple<int, int[]> rolls = mainHand.primaryAttack.damageDice.Roll();

        print("Attack: " + mainHand + ", " + rolls.Item2[0]);
        remainingAttackActions--;

        return rolls;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
    }

    public void ResetTurnValues()
    {
        currentMovement = movementSpeed;
        remainingAttackActions = numAttackActions;
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
            currentMovement -= cost;
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
            map.Dijkstra(this, true, false);
            map.OpenActionMenu();
            map.NewToolTip(occupyingTile);
        }
        else //if there's still movement left to do...
        {
            MoveNextTile(cost);
        }
    }

}

[Serializable]
public class UnitSpec
{
    public string name;
    public string team;
    public string alignment;
    public int AC;
    public int HP;
    public int walkingSpeed;
    public int swimmingSpeed;
    public int STR;
    public int DEX;
    public int CON;
    public int INT;
    public int WIS;
    public int CHA;
    public string savingThrows;
    public string skills;
    public string senses;
    public string languages;
    public string traits;
    public string spritesheet;
    public int numSprites;
}