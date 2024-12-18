using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage 
{
    public Vector3 damagePosition;
    public int damagePoints;
    public float knockback;

    public Damage(int damagePoints, float knockback)
    {
        this.damagePoints = damagePoints;
        this.knockback = knockback;
        this.damagePosition = Vector3.zero;
    }
    public Damage(int damagePoints, float knockback, Vector3 damagePosition)
    {
        this.damagePoints = damagePoints;
        this.knockback = knockback;
        this.damagePosition = damagePosition;
    }
}
