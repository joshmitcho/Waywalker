using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    bool isInRightCorner;
    RectTransform rect;
    bool isSliding;

    public TextMeshProUGUI tileType;

    List<TooltipItem> items;
    public enum ItemType { tileType, movementCost, distToUnit };

    Dictionary<ItemType, int> itemHierarchy = new Dictionary<ItemType, int>
    {
        { ItemType.tileType, 0 },
        { ItemType.movementCost, 1 },
        { ItemType.distToUnit, 2 }
    };

    void Start()
    {
        enabled = false;
        isInRightCorner = true;
        isSliding = false;
        rect = GetComponent<RectTransform>();
    }

    public void SwapSides()
    {
        if (!isSliding)
        {
            if (isInRightCorner)
            {
                StartCoroutine(Slide(new Vector2(0, 0), new Vector2(8, 8), Vector2.zero, Vector2.zero, 0.5f));
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
        isInRightCorner = !isInRightCorner;
        isSliding = false;
    }

    public void LoadTile(ClickableTile tile)
    {
        //ClearItems();
        //LoadItem(new TooltipItem(ItemType.tileType, tile.GetTileType()));
        tileType.text = tile.GetTileType();
        //tileType.outlineWidth = 10.2f;
        //tileType.outlineColor = new Color32(255, 128, 255, 255);

        enabled = true;
    }

    public void LoadUnit(Unit unit)
    {
        enabled = true;
    }
    /*
    void LoadItem(TooltipItem item)
    {
        
        //if there's no items yet, add item
        if (items.Count == 0)
        {
            items.Add(item);
            return;
        }
        
        int i = 0;
        while (i < items.Count)
        {
            //if item should come before next item in list, insert it there
            if (itemHierarchy[item.type] < itemHierarchy[items[i].type])
            {
                items.Insert(i, item);
                return;
            }
            i++;
        } 
        //if it hasn't been added yet, it must go at the end. Add it
        items.Add(item);
    }
    */
    public void ClearItems()
    {
        enabled = false;
    }


}
class TooltipItem
{
    public Tooltip.ItemType type;
    public Sprite icon;
    public string value;

    public override string ToString()
    {
        return type + ": " + value;
    }

    public TooltipItem(Tooltip.ItemType type, string value)
    {
        this.type = type;
        this.value = value;
        if (type != Tooltip.ItemType.tileType)
            icon = (Sprite)Resources.Load(iconLookup[type]);

    }

    Dictionary<Tooltip.ItemType, string> iconLookup = new Dictionary<Tooltip.ItemType, string>
    {
        { Tooltip.ItemType.tileType, "0" },
        { Tooltip.ItemType.movementCost, "0" },
        { Tooltip.ItemType.distToUnit, "0" }
    };

}
