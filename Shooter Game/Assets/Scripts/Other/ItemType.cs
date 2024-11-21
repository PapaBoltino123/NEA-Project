using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ItemStructures;
using System;
using System.Linq;

public class ItemType : MonoBehaviour
{
    public dynamic item;

    public void AssignItemType()
    {
        string[] rangedItemNames = { "Pistol", "SMG", "RocketLauncher", "Rifle"};
        string[] healthItemNames = { "Medkit", "Medicine", "Bandage" };
        string[] meleeItemNames = { "Sword", "Spear", "Axe"};

        if (rangedItemNames.Contains(gameObject.name))
        {
            item = new RangedWeapon();
            item.AssignVariables(gameObject.name);
        }
        else if (meleeItemNames.Contains(gameObject.name))
        {
            item = new MeleeWeapon();
            item.AssignVariables(gameObject.name);
        }
        else if (healthItemNames.Contains(gameObject.name))
        {
            item = new HealthPack();
            item.AssignVariables(gameObject.name);
        }
    }
}
