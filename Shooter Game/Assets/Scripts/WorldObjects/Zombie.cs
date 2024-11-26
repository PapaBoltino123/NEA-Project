using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    public delegate void PathFoundCallback(List<Node> path);
    private float speed;
    private float jumpForce;
    private Grid<Node> nodeMap = null;
    private Pathfinder pathfinder;
    private List<Node> path;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    [SerializeField] LayerMask groundLayer;
    private float gravity;
    bool runTest = true;

    private void Awake()
    {
        gravity = System.Math.Abs(Physics2D.gravity.y);
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        speed = ZombieManager.Instance.speed;
        jumpForce = ZombieManager.Instance.jumpForce;
        nodeMap = ZombieManager.Instance.nodeMap;
        pathfinder = ZombieManager.Instance.pathfinder;
    }

    private void Start()
    {
        StartCoroutine(CalculatePath());
    }

    private void FindNewPath()
    {
        pathfinder.FindPath(nodeMap.GetGridObject(transform.position), nodeMap.GetGridObject(Player.Instance.transform.position), OnPathFound);
    }
    public void OnPathFound(List<Node> path)
    {
        this.path = path;
    }

    private float CalculateMaxJumpHeight()
    {
        float maxHeight = (jumpForce * jumpForce) / (2 * gravity);
        return maxHeight;
    }
    private float CalculateMaxJumpWidth()
    {
        float maxDistance = (2 * speed * jumpForce) / gravity;
        return maxDistance;
    }
    private int ConvertToGrid(float n)
    {
        return (int)System.Math.Floor(n / nodeMap.CellSize);
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
    private IEnumerator CalculatePath()
    {
        while (IsGrounded() == false)
        {
            yield return null;
        }
        FindNewPath();

        yield return new WaitForSeconds(5f);

        foreach (var node in path)
        {
            Debug.Log(node);
        }
    }
}
