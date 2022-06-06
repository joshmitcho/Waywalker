using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item
{
    public Attack primaryAttack;

    public Weapon(string name, string desc, Attack primary) : base(name, desc)
    {
        primaryAttack = primary;
    }

    public override string ToString()
    {
        return name;
    }

}
