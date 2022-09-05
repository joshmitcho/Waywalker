using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Dice
{
    public int[] values = new int[3]; // XdY+Z  e.g. 2d6+3

    public Dice(int x, int y, int z)
    {
        values[0] = x;
        values[1] = y;
        values[2] = z;
    }

    public Tuple<int, int[]> Roll() //[0] is the total result, [1]..[n] is the individual dice values, last is the modifier
    {
        int[] rolls = new int[values[0] + 2]; //first is the total result, last is the modifier
        int rollTotal = 0;

        for (int i = 1; i < values[0]+1; i++)
        {
            int rollVal = Random.Range(1, values[1]);

            rolls[i] = rollVal;

            rollTotal += rollVal;
        }
        rollTotal += values[2];

        rolls[0] = rollTotal; //first is the total result
        rolls[values[0]+1] = values[2]; //last is the modifier

        return Tuple.Create(values[1], rolls);
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
