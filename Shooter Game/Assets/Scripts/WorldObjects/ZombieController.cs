using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    Pathfinder pathfinder;
    bool mainGameLoaded = false;
    List<Vector2Int> mapPath;
    List<Vector3> worldPath;
    Grid<Node> map;

    private void Start()
    {
        mainGameLoaded = false;
        mapPath = new List<Vector2Int>();
        worldPath = new List<Vector3>();
    }
    void Update()
    {
        if (mainGameLoaded == false)
        {
            try
            {
                pathfinder = GameManager.Instance.pathfinder;
                mainGameLoaded = true;
            }
            catch
            {
                mainGameLoaded = false;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                map = TerrainManager.Instance.ReturnWorldMap();
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int startPosition = new Vector2Int(map.GetXY(mousePosition).x, map.GetXY(mousePosition).y);
                Vector2Int endPosition = new Vector2Int(map.GetXY(Player.Instance.transform.position).x, map.GetXY(Player.Instance.transform.position).y);
                mapPath = pathfinder.FindPath(startPosition, endPosition, (short)5);
                mapPath.Reverse();

                for (int i = 0; i < mapPath.Count; i++)
                {
                    Vector2Int point = mapPath[i];
                    worldPath.Add(map.GetWorldPosition(point.x, point.y));
                }
                foreach (var point in mapPath)
                {
                    Debug.Log(point);
                }
            }
        }
    }
}
