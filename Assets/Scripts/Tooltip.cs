using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    bool isInRightCorner;
    RectTransform rect;
    bool isSliding;

    public TextMeshProUGUI tileType;
    public Image tileIcon;

    public TextMeshProUGUI terrainCost;
    public TextMeshProUGUI distanceToActiveUnit;

    public Sprite shadeFull;
    public Sprite shadeHalf;
    public Sprite frameFull;
    public Sprite frameHalf;
    Image shade;
    public Image frame;

    public GameObject unitTooltip;

    public TextMeshProUGUI unitName;
    public Image unitIcon;

    void Start()
    {
        gameObject.SetActive(false);
        isInRightCorner = true;
        isSliding = false;
        rect = GetComponent<RectTransform>();
        shade = GetComponent<Image>();
    }

    public void SwapSides()
    {
        if (!isSliding)
        {
            if (isInRightCorner)
            {
                StartCoroutine(Slide(new Vector2(0, 0), new Vector2(-40, 8), Vector2.zero, Vector2.zero, 0.5f));
            }
            else
            {
                StartCoroutine(Slide(new Vector2(1, 0), new Vector2(-8, 8), Vector2.zero, Vector2.zero, 0.5f));
            }
        }
        
    }

    IEnumerator Slide(Vector2 pivotEnd, Vector2 positionB, Vector2 positionC, Vector2 positonEnd, float timeToMove)
    {
        float t = 0f;

        rect.anchorMin = pivotEnd;
        rect.anchorMax = pivotEnd;
        rect.pivot = pivotEnd;

        while (t < 1)
        {
            isSliding = true;

            t += Time.deltaTime / timeToMove;
            rect.anchoredPosition = Vector3.Lerp(rect.anchoredPosition, positionB, t);

            yield return null;
        }
        rect.anchoredPosition = positionB;
        isInRightCorner = !isInRightCorner;
        isSliding = false;
    }

    public void LoadTile(ClickableTile tile, int dist, Unit unit)
    {
        frame.sprite = frameHalf;
        shade.sprite = shadeHalf;
        tileIcon.sprite = tile.GetComponent<SpriteRenderer>().sprite;
        tileIcon.color = tile.GetComponent<SpriteRenderer>().color;

        if (!isInRightCorner && !isSliding)
        {
            rect.anchoredPosition = new Vector2(-40, 8);
        }

        tileType.text = tile.GetTileType();
        //tileType.outlineWidth = 10.2f;
        //tileType.outlineColor = new Color32(255, 128, 255, 255);

        terrainCost.text = tile.movementCost.ToString();

        if (dist > unit.remainingMovement || dist < 0)
            distanceToActiveUnit.color = Color.red;
        else
            distanceToActiveUnit.color = Color.white;

        if (dist > 1000 || dist < 0)
            distanceToActiveUnit.text = "inf";
        else
            distanceToActiveUnit.text = dist.ToString();

        gameObject.SetActive(true);
        
        if (tile.occupyingUnit != null)
        {
            LoadUnit(tile.occupyingUnit); return;
        }
        unitTooltip.SetActive(false);
    }

    void LoadUnit(Unit unit)
    {
        frame.sprite = frameFull;
        shade.sprite = shadeFull;

        unitIcon.sprite = unit.GetComponentInChildren<SpriteRenderer>().sprite;
        unitIcon.color = unit.GetComponentInChildren<SpriteRenderer>().color;

        unitName.text = unit.unitName;

        if (!isInRightCorner && !isSliding)
        {
            rect.anchoredPosition = new Vector2(8,8);
        }

        unitTooltip.SetActive(true);
    }
    
    public void ClearItems()
    {
        gameObject.SetActive(false);
    }


}
