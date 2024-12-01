using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        path.Insert(0, nodeMap.GetGridObject(transform.position));
        path.Add(nodeMap.GetGridObject(Player.Instance.transform.position));
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

        //Vector2 direction = (Player.Instance.transform.position.x > transform.position.x) ? Vector2.left : Vector2.right;

        //if (TerrainLevelChange() == true)
        //{
        //    StartCoroutine(WalkDistance((int)direction.x, (float)System.Math.Abs((decimal)(Player.Instance.transform.position.x - transform.position.x))));
        //}
        //else
        FindNewPath();

        yield return new WaitForSeconds(3);

        if (path.Count > 0)
        {
            FollowPath();
        }
        //else if (TerrainLevelChange() == true)
        //{
        //    StartCoroutine(WalkDistance((int)direction.x, (float)System.Math.Abs((decimal)(Player.Instance.transform.position.x - transform.position.x))));
        //}
    }
    private void FollowPath()
    {
        for (int i = 0; i < path.Count; i++)
        {
            if (i + 1 < path.Count)
            {
                if (path[i].y == path[i + 1].y)
                {
                    float distance = System.Math.Abs(path[i + 1].x - path[i].x) * 0.16f;
                    int direction = (path[i + 1].x > path[i].x) ? 1 : -1;
                    StartCoroutine(WalkDistance(direction, distance));
                }
                else if (path[i].y < path[i + 1].y)
                {
                    int direction = (path[i + 1].x > path[i].x) ? 1 : -1;
                    StartCoroutine(JumpToNextNode(direction, path[i + 1]));
                }
                else if (path[i].y > path[i + 1].y)
                {
                    float distance = System.Math.Abs(path[i + 1].x - path[i].x) * 0.16f;
                    int direction = (path[i + 1].x > path[i].x) ? 1 : -1;
                    StartCoroutine(WalkDistance(direction, distance));
                }
            }
            else
                break;
        }
    }
    public void Jump(int direction, float jumpForce, Rigidbody2D rb)
    {
        rb.velocity = new Vector2(speed * direction, jumpForce);
    }
    public void Move(int direction, float speed, Rigidbody2D rb)
    {
        rb.velocity = new Vector3(direction * speed, rb.velocity.y);
        if (direction > 0)
        {
            transform.localScale = Vector3.one;
        }
        if (direction < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    private IEnumerator WalkDistance(int direction, float distance, bool lastMoveWasJump = false)
    {
        if (lastMoveWasJump == true)
            distance -= 0.08f;

        int frames = (int)System.Math.Ceiling((decimal)(distance / (speed * Time.fixedDeltaTime)));

        for (int i = 0; i < frames; i++)
        {
            Move(direction, speed, rb);
            yield return new WaitForFixedUpdate();
        }

        rb.velocity = Vector3.zero;
    }
    private IEnumerator JumpToNextNode(int direction, Node nextNode)
    {
        Jump(direction, jumpForce, rb);

        while (nodeMap.GetGridObject(transform.position).y < nextNode.y)
        {
            yield return new WaitForFixedUpdate();
        }

        StartCoroutine(WalkDistance(direction, 0.24f, true));
    }
    private bool TerrainLevelChange()
    {
        List<int> heights = TerrainManager.Instance.surfaceHeights.Skip(nodeMap.GetXY(transform.position).x)
            .Take((int)System.Math.Abs((decimal)(Player.Instance.transform.position.x - transform.position.x)))
            .ToList();

        return !(heights.All(item => item == heights[0]));
    }
}