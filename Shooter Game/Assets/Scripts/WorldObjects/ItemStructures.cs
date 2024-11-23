using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace System.ItemStructures
{
    public class Item
    {
        public string name { get; set; }
        public Type type { get; set; }
        public int count { get; set; }
        
        public Item(string name)
        {
            string[] rangedItemNames = { "Pistol", "SMG", "RocketLauncher", "Rifle" };
            string[] healthItemNames = { "Medkit", "Medicine", "Bandage" };
            string[] meleeItemNames = { "Sword", "Spear", "Axe" };
            string[] ammoItemNames = { "PistolAmmo", "SMGAmmo", "RocketLauncherAmmo", "RifleAmmo" };

            this.name = name;

            if (rangedItemNames.Contains(name))
                type = typeof(RangedWeapon);
            else if (healthItemNames.Contains(name))
                type = typeof(HealthPack);
            else if (meleeItemNames.Contains(name))
                type = typeof(MeleeWeapon);
            else if (ammoItemNames.Contains(name))
                type = typeof(Ammo);
            else
                throw new Exception("Invalid name");
        }

        public virtual void AssignVariables(string name)
        {
            Debug.Log("Assigning variables");
        }
        public Item ToItem()
        {
            Item item = new Item(this.name);
            item.count = this.count;
            return item;
        }
        public override string ToString()
        {
            return name;
        }
    }
    public class Weapon : Item
    {
        public int damage { get; set; }
        public float knockback { get; set; }

        public Weapon(string name) : base(name)
        {
            this.name = name;

            string[] rangedItemNames = { "Pistol", "SMG", "RocketLauncher", "Rifle" };
            string[] meleeItemNames = { "Sword", "Spear", "Axe" };

            if (rangedItemNames.Contains(name))
                type = typeof(RangedWeapon);
            else if (meleeItemNames.Contains(name))
                type = typeof(MeleeWeapon);
            else
                throw new Exception("Invalid name");
        }
    }
    public class Ammo : Item
    {
        public Ammo(string name) : base(name)
        {
            this.name = name;
            type = typeof(Ammo);
        }
    }
    public class HealthPack : Item
    {
        public int healthBoost = 0;
        public float effectLength = 0f;

        public HealthPack(string name) : base(name) 
        {
            this.name = name;
            type = typeof(HealthPack);
            AssignVariables(name);
        }

        public override void AssignVariables(string name)
        {
            switch(name)
            {
                case "Bandage":
                    healthBoost = 15;
                    effectLength = 1.5f;
                    break;
                case "Medicine":
                    healthBoost = 30;
                    effectLength = 3f;
                    break;
                case "Medkit":
                    healthBoost = 100;
                    effectLength = 5f;
                    break;
            }
        }
    }
    public class MeleeWeapon : Weapon
    {
        public float attackSpeed = 0f;

        public MeleeWeapon(string name) : base(name)
        {
            this.name = name;
            type = typeof(MeleeWeapon);
            AssignVariables(name);
        }

        public override void AssignVariables(string name)
        {
            switch (name)
            {
                case "Axe":
                    damage = 5;
                    knockback = 0.5f;
                    attackSpeed = 3f;
                    break;
                case "Spear":
                    damage = 10;
                    knockback = 2;
                    attackSpeed = 1.5f;
                    break;
                case "Sword":
                    damage = 30;
                    knockback = 1f;
                    attackSpeed = 2f;
                    break;
            }
        }
    }
    public class RangedWeapon : Weapon
    {
        public float cooldown = 0f;
        public float reloadSpeed = 0f;
        public int magSize;
        public int magCount { get; set; }
        public bool loaded = true;

        public RangedWeapon(string name) : base(name)
        {
            this.name = name;
            type = typeof(RangedWeapon);
            AssignVariables(name);
        }

        public override void AssignVariables(string name)
        {
            switch (name)
            {
                case "Pistol":
                    damage = 5;
                    knockback = 1f;
                    cooldown = 0.6f;
                    reloadSpeed = 3f;
                    magSize = 15;
                    break;
                case "Rifle":
                    damage = 10;
                    knockback = 1f;
                    cooldown = 0.9f;
                    reloadSpeed = 4f;
                    magSize = 20;
                    break;
                case "SMG":
                    damage = 3;
                    knockback = 0.5f;
                    cooldown = 0.2f;
                    reloadSpeed = 5f;
                    magSize = 100;
                    break;
                case "RocketLauncher":
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
