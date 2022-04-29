using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    Vector3 aboutPoint;

    // Start is called before the first frame update
    void Awake()
    {
        aboutPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActiveAndEnabled)
        {

            transform.position = aboutPoint + new Vector3(0, waveGenerator(), 0);

        }
    }
    float waveGenerator()
    {
        return Mathf.Sin(Time.time*10f)/20f;
    }



}
