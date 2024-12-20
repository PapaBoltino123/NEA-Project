using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public float playerX;
    public float playerY;
    public int score;
    public int playerHealth;

    public int seed;
    public int smoothness;

    public float zombieSpeed;
    public int zombieHealth;
    public float zombieSpawnRate;
    public int zombieDamagePoints;

    public GameData()
    {
        playerX = -10;
        playerY = -10;
        score = 0;
        playerHealth = 50;

        seed = 100000;
        smoothness = 50;
        zombieSpeed = 2;
        zombieHealth = 10;
        zombieSpawnRate = 1.5f;
        zombieDamagePoints = 5;
    }
}
