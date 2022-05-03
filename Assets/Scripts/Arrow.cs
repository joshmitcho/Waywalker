using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    Object[] sprites;
    SpriteRenderer rend;

    public int arrowLength;
    public GameObject arrowPrefab;

    List<GameObject> arrowSegments = new List<GameObject>();

    public void GenerateArrowSegments(int mapSizeX, int mapSizeY)
    {
        sprites = Resources.LoadAll("Arrow", typeof(Sprite));

        GameObject go;

        for (int i = 0; i < mapSizeX + mapSizeY - 1; i++)
        {
            go = Instantiate(arrowPrefab, transform.position + new Vector3(i, 0, 0), Quaternion.identity, transform);
            if (go == null)
            {
                throw new System.Exception("instatiate failed");
            }
            else
            {
                rend = go.GetComponent<SpriteRenderer>();

                rend.sprite = (Sprite)sprites[i % 3];
                arrowSegments.Add(go);
                go.SetActive(false);
            }

        }
    }

    public void DrawArrow(List<Node> nodes)
    {
        foreach (GameObject segment in arrowSegments)
        {
            segment.SetActive(false);
        }

        if (nodes == null) return;        

        int i;
        float angle = 0;
        int spriteIndex = 2;
        //iterate through the nodes to construct the arrow, segment by segment
        for (i = 1; i < nodes.Count; i++)
        {
            arrowSegments[i].SetActive(true);
            arrowSegments[i].transform.position = new Vector3(nodes[i].x, nodes[i].y, -1);

            //set sprite to straight path if...
            if (nodes[i].x == nodes[i - 1].x || nodes[i].y == nodes[i - 1].y) //same x value or y value means straight path
            {
                spriteIndex = 2;
            }

            //set rotation of straight path based on the position of previous arrow segment
            if (nodes[i].x > nodes[i - 1].x) angle = 270;
            else if (nodes[i].x < nodes[i - 1].x) angle = 90;
            if (nodes[i].y > nodes[i - 1].y) angle = 0;
            else if (nodes[i].y < nodes[i - 1].y) angle = 180;

            //set sprite to bend if...
            if (i < nodes.Count-1)//if it's not the end of the arrow (the head)
            {
                if (nodes[i].y > nodes[i - 1].y && nodes[i + 1].x > nodes[i].x
                    || nodes[i].x < nodes[i - 1].x && nodes[i + 1].y < nodes[i].y) //bottom to right bend
                {
                    spriteIndex = 1;
                    angle = 0;
                }else if (nodes[i].y < nodes[i - 1].y && nodes[i + 1].x > nodes[i].x
                    || nodes[i].x < nodes[i - 1].x && nodes[i + 1].y > nodes[i].y) //top to right bend
                {
                    spriteIndex = 1;
                    angle = 90;
                }
                else if(nodes[i].y < nodes[i - 1].y && nodes[i + 1].x < nodes[i].x
                    || nodes[i].x > nodes[i - 1].x && nodes[i + 1].y > nodes[i].y) //left to top bend
                {
                    spriteIndex = 1;
                    angle = 180;
                }
                else if(nodes[i].y > nodes[i - 1].y && nodes[i + 1].x < nodes[i].x
                    || nodes[i].x > nodes[i - 1].x && nodes[i + 1].y < nodes[i].y) //left to bottom bend
                {
                    spriteIndex = 1;
                    angle = 270;
                }
            }

            //last segment is always the arrowhead
            if (i == nodes.Count - 1) spriteIndex = 0;

            //apply rotation and sprite
            arrowSegments[i].transform.rotation = Quaternion.Euler(0, 0, angle);
            arrowSegments[i].GetComponent<SpriteRenderer>().sprite = (Sprite)sprites[spriteIndex];
            
        }

    }
}
