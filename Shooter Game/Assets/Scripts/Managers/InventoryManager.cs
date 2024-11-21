using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    public GameObject[] hotBarSlots = null;
    public GameObject activeSlot;
    public int activeSlotIndex;

    void Update()
    {
        try
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                activeSlotIndex = (int)HotBarType.RANGED;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                activeSlotIndex = (int)HotBarType.MELEE;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                activeSlotIndex = (int)HotBarType.HEALTH;

            activeSlot = hotBarSlots[activeSlotIndex];
        }
        catch { }
    }
}
