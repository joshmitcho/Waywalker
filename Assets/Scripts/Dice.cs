using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice
{
    public int[] values = new int[3]; // XdY+Z  e.g. 2d6+3

    public Dice(int x, int y, int z)
    {
        values[0] = x;
        values[1] = y;
        values[2] = z;
    }

    public int Roll()
    {
        int rollTotal = 0;

        for (int i = 0; i < values[0]; i++)
        {
            int rollVal = Random.Range(1, values[1]);
            rollTotal += rollVal;
        }
        rollTotal += values[2];

        return rollTotal;
    }

    public override string ToString()
    {
        if (values[2] > 0)
            return values[0] + "d" + values[1] + "+" + values[2];
        else if (values[2] < 0)
            return values[0] + "d" + values[1] + "-" + values[2];
        else
            return values[0] + "d" + values[1];

    }
}
