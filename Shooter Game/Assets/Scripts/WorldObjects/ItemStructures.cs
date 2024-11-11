using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace System.ItemStructures
{
    public interface iItem
    {
        string name { get; set; }
        Type type { get; set; }
        int count { get; set; }
        void AssignVariables(string name);
    }
    public class Ammo
    {
        public Type type { get; set; }
        public int count { get; set; }
        public string name
        {
            get { return name; }
            set
            {
                name = value;
                AssignVariables(name);
            }
        }

        public void AssignVariables(string name)
        {
            type = typeof(Ammo);
        }
    }
    public class HealthPack : iItem
    {
        public Type type { get; set; }
        public int count { get; set; }
        public string name
        {
            get { return name; }
            set
            {
                name = value;
                AssignVariables(name);
            }
        }
        public int healthBoost = 0;
        public float effectLength = 0f;

        public void AssignVariables(string name)
        {
            type = typeof(HealthPack);

            switch(name)
            {
                case "bandage":
                    healthBoost = 15;
                    effectLength = 1.5f;
                    break;
                case "medicine":
                    healthBoost = 30;
                    effectLength = 3f;
                    break;
                case "medkit":
                    healthBoost = 100;
                    effectLength = 7f;
                    break;
            }
        }
    }
    public class MeleeWeapon : iItem
    {
        public Type type { get; set; }
        public int count { get; set; }
        public string name
        {
            get { return name; }
            set
            {
                name = value;
                AssignVariables(name);
            }
        }
        public int damage = 0;
        public float knockback = 0f;
        public int durability = 0;
        public float attackSpeed = 0f;
        public void AssignVariables(string name)
        {
            type = typeof(MeleeWeapon);

            switch (name)
            {
                case "axe":
                    damage = 5;
                    knockback = 0.5f;
                    durability = 20;
                    attackSpeed = 3f;
                    break;
                case "spear":
                    damage = 10;
                    knockback = 2;
                    durability = 10;
                    attackSpeed = 1.5f;
                    break;
                case "sword":
                    damage = 30;
                    knockback = 1f;
                    durability = 30;
                    attackSpeed = 2f;
                    break;
            }
        }
    }
    public class RangedWeapon
    {
        public Type type { get; set; }
        public int count { get; set; }
        public int damage;
        public float knockback = 0f;
        public float cooldown = 0f;
        public float reloadSpeed = 0f;
        public int magSize;
        public int magCount { get; set; }
        public bool loaded = true;
        public string name
        {
            get { return name; }
            set
            {
                name = value;
                AssignVariables(name);
            }
        }

        public void AssignVariables(string name)
        {
            type = typeof(HealthPack);

            switch (name)
            {
                case "pistol":
                    damage = 5;
                    knockback = 1f;
                    cooldown = 0.6f;
                    reloadSpeed = 3f;
                    magSize = 15;
                    break;
                case "rifle":
                    damage = 10;
                    knockback = 1f;
                    cooldown = 0.9f;
                    reloadSpeed = 4f;
                    magSize = 20;
                    break;
                case "machineGun":
                    damage = 3;
                    knockback = 0.5f;
                    cooldown = 0.2f;
                    reloadSpeed = 5f;
                    magSize = 100;
                    break;
                case "rPG":
                    damage = 50;
                    knockback = 3;
                    cooldown = 3;
                    reloadSpeed = 7f;
                    magSize = 3;
                    break;
            }
        }
    }
}
