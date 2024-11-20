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
    private Rigidbody2D rb;
    [SerializeField] LayerMask groundLayer;
    bool runTest = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

        StartCoroutine(FollowPath());
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
    public void Walk(Node currentNode, Node nextNode, int direction)
    {
        if (currentNode.y == nextNode.y)
            StartCoroutine(WalkOneNode(direction));
        else
            StartCoroutine(WalkThenFall(currentNode, nextNode, direction));
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
    private IEnumerator WalkOneNode(int direction)
    {
        int frames = (int)System.Math.Ceiling((decimal)(0.16f / (speed * Time.fixedDeltaTime)));
        float distancePerFrame = 0.16f / frames;

        for (int i = 0; i < frames; i++)
        {
            Move(direction, speed, rb);
            yield return new WaitForFixedUpdate();
        }
    }
    private IEnumerator WalkThenFall(Node currentNode, Node targetNode, int direction)
    {
        while (nodeMap.GetGridObject(transform.position) != currentNode && IsGrounded() == false)
        {
            Move(direction, speed, rb);
            yield return new WaitForFixedUpdate();
        }
    }
    private IEnumerator FollowPath()
    {
        for (int i = 0; i < path.Count; i++)
        {
            Node currentNode = path[i];

            if (i + 1 < path.Count)
            {
                Node nextNode = path[i + 1];

                int direction = (nextNode.x > currentNode.x) ? 1 : -1;

                int jumpTargetX = currentNode.x + direction;
                float initialVelocityX = direction * speed;
                float initialVelocityY = jumpForce;

                (int? landingX, int? landingY) = pathfinder.SimulateJump(currentNode.x, currentNode.y, initialVelocityX, initialVelocityY);

                if (landingX.HasValue && landingY.HasValue)
                {
                    if (landingX == nextNode.x && landingY == nextNode.y)
                        Jump(direction, jumpForce, rb);
                    else
                        Walk(currentNode, nextNode, direction);
                }
                else
                    Walk(currentNode, nextNode, direction);

                while (nodeMap.GetGridObject(transform.position) != nextNode)
                    yield return null;
            }
            else
                break;
        }
    }

    void AlignPosition(Node node)
    {
        transform.position = new Vector3(nodeMap.GetWorldPosition(node.x, node.y).x, transform.position.y, 0);
    }
}
