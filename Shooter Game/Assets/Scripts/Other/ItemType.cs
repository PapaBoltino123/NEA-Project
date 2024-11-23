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
        item = new Item(gameObject.name);
    }
}
