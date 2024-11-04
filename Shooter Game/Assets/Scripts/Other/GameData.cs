using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public float playerX;
    public float playerY;
    public int seed;

    public GameData()
    {
        playerX = -10;
        playerY = -10;
        seed = 100000;
    }
}
