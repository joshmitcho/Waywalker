using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class P : MonoBehaviour
{
    public TextMeshProUGUI display;

    public void rint(string line)
    {
        display.text += line + '\n';
        Debug.Log(line);
    }
}