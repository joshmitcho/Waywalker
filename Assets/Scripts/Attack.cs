using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack
{
    public int range;
    public Dice attackDice;
    public Dice damageDice;

    public Attack(int rng, Dice att, Dice dmg)
    {
        range = rng;
        attackDice = att;
        damageDice = dmg;
    }
}
