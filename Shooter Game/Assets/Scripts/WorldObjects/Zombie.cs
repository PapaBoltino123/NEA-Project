using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    public delegate void PathFoundCallback(List<Node> path);
    private float speed;
    private float jumpForce;
    private byte[,] byteMap = null;
    private Grid<Node> nodeMap = null;
    private Pathfinder pathfinder;
    private List<Node> path;
    private BoxCollider2D boxCollider;
    [SerializeField] LayerMask groundLayer;
    bool runTest = true;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        speed = ZombieManager.Instance.speed;
        jumpForce = ZombieManager.Instance.jumpForce;
        nodeMap = ZombieManager.Instance.nodeMap;
        byteMap = ZombieManager.Instance.byteMap;

        if (byteMap != null && nodeMap != null)
        {
            pathfinder = new Pathfinder(nodeMap, byteMap, speed, jumpForce);
        }
    }
    private void Update()
    {
        if (IsGrounded() == true && runTest == true)
        {
            FindNewPath();

            foreach (var node in path)
            {
                Debug.Log(node.x + ", " + node.y);
            }

            runTest = false;
        }
    }
    private void FindNewPath()
    {
        (int x, int y) startCoords = nodeMap.GetXY(transform.position);
        (int x, int y) endCoords = nodeMap.GetXY(Player.Instance.transform.position);

        pathfinder.FindPath(startCoords.x, startCoords.y, endCoords.x, endCoords.y, OnPathFound);
    }
    public void OnPathFound(List<Node> path)
    {
        this.path = path;
    }
    private bool IsGrounded()
    {
        Bounds bounds = boxCollider.bounds;
        Vector2 boxSize = new Vector2(bounds.size.x, 0.1f);
        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y - 0.1f / 2);

        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0f, Vector2.down, 0.1f, groundLayer);

        if (hit.collider != null)
            return true;
        else
            return false;
    }
}
