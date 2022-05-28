using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item
{
    public Attack primaryAttack;

    public Weapon(string nm, string desc, Attack primary) : base(nm, desc)
    {
        primaryAttack = primary;
    }
}
