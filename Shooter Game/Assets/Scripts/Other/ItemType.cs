using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ItemStructures;
using System;
using System.Linq;

public class ItemType : MonoBehaviour
{
    public Item item;

    public void AssignItem()
    {
        string name = gameObject.name;

        if (name.Contains('('))
            name = name.Substring(0, name.Length - 7);


        item = new Item(name);
    }
}
