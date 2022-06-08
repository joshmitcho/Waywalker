using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceHandler : MonoBehaviour
{
    public TileMap map;

    Object[] sprites;

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
                dice.Add(go);
                go.SetActive(false);
            }

        }
    }

    public void DrawDice(int[] values, Unit unit)
    {
        Vector3 anchor = map.cam.WorldToScreenPoint(unit.transform.position) + new Vector3(0, 32, 0);

        //clear all dice first
        foreach (GameObject die in dice)
        {
            die.SetActive(false);
        }

        //then show the relevant dice
        for (int i = 0; i < values.Length; i++)
        {
            dice[i].SetActive(true);
            dice[i].GetComponent<Image>().color = unit.diceColour;
            dice[i].GetComponentInChildren<TextMeshProUGUI>().color = unit.numColour;

            dice[i].transform.position = anchor + new Vector3(i*32, 0, 0);
            dice[i].GetComponentInChildren<TextMeshProUGUI>().text = values[i].ToString();
        }

        //then the modifier
        if (values[^1] >= 0)
        {
            dice[values.Length-1].GetComponentInChildren<TextMeshProUGUI>().text = "+" + values[^1];
        }
        else
        {
            dice[values.Length - 1].GetComponentInChildren<TextMeshProUGUI>().text = "-" + values[^1];
        }

        //modifier isn't a die, so it's a grey circle with white text
        dice[values.Length - 1].GetComponent<Image>().color = Color.grey;
        dice[values.Length - 1].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        dice[values.Length - 1].GetComponent<Image>().sprite = (Sprite)sprites[2];


        //put the result in the correct position
        dice[0].transform.position = anchor + new Vector3(0, 32, 0);


        map.state = TileMap.State.zero;
        map.OpenActionMenu();

    }
    
}
