using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableUnit : MonoBehaviour
{
    public string unitName;

    private void OnMouseUp()
    {
        Debug.Log("Click " + unitName);


    }
}
