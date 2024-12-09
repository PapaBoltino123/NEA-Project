using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage 
{
    Vector3 deathPosition;
    int damagePoints;
    float knockback;

    public Damage(int damagePoints, float knockback)
    {
        this.damagePoints = damagePoints;
        this.knockback = knockback;
    }
    public Damage(int damagePoints, float knockback, Vector3 deathPosition)
    {
        this.damagePoints = damagePoints;
        this.knockback = knockback;
        this.deathPosition = deathPosition;
    }
}
