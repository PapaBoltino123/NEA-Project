using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ItemStructures;

public class InventoryManager : Singleton<InventoryManager>
{
    public GameObject[] hotBarSlots = null;
    private List<Item> acquiredItems;

    public GameObject activeSlot;
    public int activeSlotIndex;
    public HashTable<Item> inventory;

    public int MaxSize
    {
        get { return inventory.maxSize; }
        set
        {
            inventory = new HashTable<Item>(value);
        }
    }
    private void Start()
    {
        acquiredItems = new List<Item>();
    }
    void Update()
    {
        if (GameManager.Instance.mainGameLoaded == true)
        {
            if (acquiredItems.Count > 0)
            {
                foreach (var item in acquiredItems)
                {
                    inventory.AddOrUpdate(item);
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
                activeSlotIndex = (int)HotBarType.RANGED;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                activeSlotIndex = (int)HotBarType.MELEE;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                activeSlotIndex = (int)HotBarType.HEALTH;

            activeSlot = hotBarSlots[activeSlotIndex];

            if (Input.GetKeyDown(KeyCode.F))
            {
                foreach (var item in inventory.ToList())
                    Debug.Log(item + ", " + item.count);
            }
        }
    }

    public void NewGame()
    {
        string[] initialItems = { "Pistol", "PistolAmmo", "Axe", "Medkit"};

        foreach (var item in initialItems)
        {
            Item itemToAdd = new Item(item);
            itemToAdd.count = (itemToAdd.name == "PistolAmmo") ? 30 : 1;
            acquiredItems.Add(itemToAdd);
        }
    }
}
