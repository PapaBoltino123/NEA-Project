using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ItemStructures;
using System;
using System.Linq;
using Unity.VisualScripting;

public class ItemType : MonoBehaviour
{
    private (HealthPack pack, MeleeWeapon melee, RangedWeapon ranged) itemVariable;

    public (HealthPack pack, MeleeWeapon melee, RangedWeapon ranged) ItemVariable
    {
        get
        {
            AssignItemType();
            return itemVariable;
        }
    }

    public void AssignItemType()
    {
        string[] rangedItemNames = { "Pistol", "SMG", "RocketLauncher", "Rifle"};
        string[] healthItemNames = { "Medkit", "Medicine", "Bandage" };
        string[] meleeItemNames = { "Sword", "Spear", "Axe"};

        if (rangedItemNames.Contains(gameObject.name))
        {
            RangedWeapon item = new RangedWeapon();
            item.AssignVariables(gameObject.name);
            itemVariable = ReturnVariable(null, null, item);
        }
        else if (meleeItemNames.Contains(gameObject.name))
        {
            MeleeWeapon item = new MeleeWeapon();
            item.AssignVariables(gameObject.name);
            itemVariable = ReturnVariable(null, item, null);
        }
        else if (healthItemNames.Contains(gameObject.name))
        {
            HealthPack item = new HealthPack();
            item.AssignVariables(gameObject.name);
            itemVariable = ReturnVariable(item, null, null);
        }
    }
    public (HealthPack pack, MeleeWeapon melee, RangedWeapon ranged) ReturnVariable(HealthPack pack, MeleeWeapon melee, RangedWeapon ranged)
    {
        return (pack, melee, ranged);
    }
}
