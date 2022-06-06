using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{
    public string tileType;

    Vector3 scale;

    public int tileX;
    public int tileY;

    public int costToFinishHere;

    public TileMap map;

    bool withinMovementSet;
    SpriteRenderer movementSetHighlight;
    bool withinAttackSet;
    SpriteRenderer attackSetHighlight;
    SpriteRenderer hoverHighlight;

    public bool isWalkable = true;
    public int movementCost = 5;
    public Unit occupyingUnit;

    private void Awake()
    {
        occupyingUnit = null;
        withinMovementSet = false;
        movementSetHighlight = GetComponentsInChildren<SpriteRenderer>()[1];
        attackSetHighlight = GetComponentsInChildren<SpriteRenderer>()[2];
        hoverHighlight = GetComponentsInChildren<SpriteRenderer>()[3];
        scale = hoverHighlight.transform.localScale;

        RemoveFromAllSets();
    }

    void Update()
    {
        if (hoverHighlight.enabled)
        {
            float wave = Mathf.Abs(Mathf.Sin(Time.time * 4f) / 5f);
            hoverHighlight.transform.localScale = scale + new Vector3(wave, wave, 0);

        }
    }

    private void OnMouseEnter()
    {
        HoverHighlightOn();

        if (map.state != TileMap.State.unitMoving)
        {
            map.GeneratePathTo(tileX, tileY, withinMovementSet);
            map.NewToolTip(this);
        }
    }

    void HoverHighlightOn()
    {
        hoverHighlight.enabled = true;
    }

    void HoverHighlightOff()
    {
        hoverHighlight.enabled = false;
    }

    private void OnMouseExit()
    {
        HoverHighlightOff();
        if (map.state != TileMap.State.unitMoving)
        {
            map.ClearCurrentPath();
        }
    }

    public void AddToMovementSet()
    {
        movementSetHighlight.enabled = true;
        withinMovementSet = true;
        StartCoroutine(animateIn(movementSetHighlight, 0.25f));
    }

    public void AddToAttackSet()
    {
        attackSetHighlight.enabled = true;
        withinAttackSet = true;
        StartCoroutine(animateIn(attackSetHighlight, 0.25f));
    }

    IEnumerator animateIn(SpriteRenderer rend, float timeToMove)
    {
        var goalScale = rend.transform.localScale;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;
            rend.transform.localScale = Vector3.Lerp(Vector3.zero, goalScale, t);
            yield return null;
        }
    }
    
    public void RemoveFromAllSets()
    {
        movementSetHighlight.enabled = false;
        attackSetHighlight.enabled = false;
        hoverHighlight.enabled = false;

        withinMovementSet = false;
        withinAttackSet = false;
    }

    private void OnMouseUp()
    {
        if (withinMovementSet && map.state == TileMap.State.choosingMovement && occupyingUnit == null)
        {
            map.MoveUnit(tileX, tileY, costToFinishHere);
        }

        if (withinAttackSet && map.state == TileMap.State.choosingAttack && occupyingUnit != null)
        {
            map.Attack();
        }

    }
}
