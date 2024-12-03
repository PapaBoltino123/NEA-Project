using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ItemStructures;
using System;
using System.Linq;
using UnityEditor.ShaderKeywordFilter;

public class InventoryManager : Singleton<InventoryManager>
{
    public GameObject[] hotBarSlots = null;

    public GameObject activeSlot;
    public int activeSlotIndex;
    public HashTable inventory;

    public int MaxSize
    {
        get { return inventory.maxSize; }
        set
        {
            inventory = new HashTable(value);
        }
    }
    void Update()
    {
        try
        {
            if (GameManager.Instance.mainGameLoaded == true)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    activeSlotIndex = (int)HotBarType.RANGED;
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                    activeSlotIndex = (int)HotBarType.MELEE;
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                    activeSlotIndex = (int)HotBarType.HEALTH;

                activeSlot = hotBarSlots[activeSlotIndex];

                List<Item> itemList = inventory.ToList();
                itemList = itemList.Where(item => item != null && !string.IsNullOrEmpty(item.name)).ToList();

                if (itemList.Count > 0)
                {
                    List<string> itemNames = itemList.Select(i => i.name).ToList();

                    foreach (var slotObject in InGameMenuManager.Instance.healthSlots)
                    {
                        slotObject.SetActive(true);
                        string slotName = slotObject.name.Substring(0, slotObject.name.Length - 4);
                        GameObject slot = slotObject.transform.Find(slotName).gameObject;

                        if (itemNames.Contains(slotName))
                            slot.SetActive(true);
                        else
                        {
                            slot.SetActive(false);
                        }
                    }
                    foreach (var slotObject in InGameMenuManager.Instance.ammoSlots)
                    {
                        slotObject.SetActive(true);
                        string slotName = slotObject.name.Substring(0, slotObject.name.Length - 4);
                        GameObject slot = slotObject.transform.Find(slotName).gameObject;

                        if (itemNames.Contains(slotName))
                            slot.SetActive(true);
                        else
                        {
                            slot.SetActive(false);
                        }
                    }
                    foreach (var slotObject in InGameMenuManager.Instance.rangedSlots)
                    {
                        slotObject.SetActive(true);
                        string slotName = slotObject.name.Substring(0, slotObject.name.Length - 4);
                        GameObject slot = slotObject.transform.Find(slotName).gameObject;

                        if (itemNames.Contains(slotName))
                            slot.SetActive(true);
                        else
                        {
                            slot.SetActive(false);
                        }
                    }
                    foreach (var slotObject in InGameMenuManager.Instance.meleeSlots)
                    {
                        slotObject.SetActive(true);
                        string slotName = slotObject.name.Substring(0, slotObject.name.Length - 4);
                        GameObject slot = slotObject.transform.Find(slotName).gameObject;

                        if (itemNames.Contains(slotName))
                            slot.SetActive(true);
                        else
                        {
                            slot.SetActive(false);
                        }
                    }
                    for (int i = 0; i < InGameMenuManager.Instance.inventoryHotBarSlots.Length; i++)
                    {
                        GameObject slotObject = InGameMenuManager.Instance.inventoryHotBarSlots[i].gameObject;
                        slotObject.SetActive(true);

                        string slotName = hotBarSlots[i].transform.Cast<Transform>()
                                        .Select(child => child.gameObject)
                                        .OrderByDescending(child => child.activeSelf)
                                        .ToList()
                                        .First()
                                        .name;

                        if (slotObject.transform.Find(slotName).gameObject != null)
                        {
                            GameObject slot = slotObject.transform.Find(slotName).gameObject;
                            slot.SetActive(true);
                        }
                    }
                }
            }
        }
        catch
        {

        }
    }

    public void NewGame()
    {
        string[] initialItems = { "Pistol", "PistolAmmo", "Axe", "Medkit"};

        foreach (var item in initialItems)
        {
            Item itemToAdd = new Item(item);
            itemToAdd.count = (itemToAdd.name == "PistolAmmo") ? 30 : 1;
            inventory.AddOrUpdate(itemToAdd);
        }
    }
}
