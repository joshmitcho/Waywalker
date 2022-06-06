using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceHandler : MonoBehaviour
{
    public TileMap map;

    Object[] sprites;
    Image rend;

    public GameObject diePrefab;

    List<GameObject> dice = new List<GameObject>();
    
    public void GenerateDieBlanks(int numDice)
    {
        sprites = Resources.LoadAll("Dice", typeof(Sprite));

        GameObject go;

        for (int i = 0; i < numDice; i++)
        {
            go = Instantiate(diePrefab, transform.position + new Vector3(i*32, 0, 0), Quaternion.identity, transform);
            if (go == null)
            {
                throw new System.Exception("instatiate failed");
            }
            else
            {
                rend = go.GetComponent<Image>();

                rend.sprite = (Sprite)sprites[i % 3];
                dice.Add(go);
                go.SetActive(false);
            }

        }
    }

    public void DrawDice(int[] values, Unit unit)
    {
        Vector3 anchor = map.cam.WorldToScreenPoint(unit.transform.position);

        //clear all dice first
        foreach (GameObject die in dice)
        {
            die.SetActive(false);
        }

        //then show total result die
        dice[0].SetActive(true);
        dice[0].transform.position = anchor + new Vector3(0, 32, 0);
        if (values[0] >= 0)
        {
            dice[0].GetComponentInChildren<TextMeshProUGUI>().text = "+" + values[0];
        }
        else
        {
            dice[0].GetComponentInChildren<TextMeshProUGUI>().text = "-" + values[0];
        }


        //the rest of the dice
        for (int i = 1; i < values.Length; i++)
        {
            dice[i].SetActive(true);

            dice[i].transform.position = anchor + new Vector3(i*32, 0, 0);
            dice[i].GetComponentInChildren<TextMeshProUGUI>().text = values[i].ToString();

        }

        map.state = TileMap.State.zero;
        map.OpenActionMenu();

    }
    
}
