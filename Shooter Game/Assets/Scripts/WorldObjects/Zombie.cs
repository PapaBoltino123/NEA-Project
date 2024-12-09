using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Zombie : MonoBehaviour
{
    private float speed;
    private float maxJumpForce;
    private int direction;
    private Grid<Node> nodeMap = null;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    [SerializeField] LayerMask groundLayer;
    private float gravity;
    bool isJumping = false;
    bool isBeingAttacked = false;
    bool handlingJumpAboveWater = false;

    private void Awake()
    {
        gravity = System.Math.Abs(Physics2D.gravity.y);
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        speed = ZombieManager.Instance.speed;
        maxJumpForce = ZombieManager.Instance.jumpForce;
        nodeMap = ZombieManager.Instance.nodeMap;
    }

    private void Update()
    {
        direction = Player.Instance.transform.position.x > transform.position.x ? 1 : -1;

        if (handlingJumpAboveWater == false)
            Move(direction, speed, rb);

        if (IsColliding(new Vector2(direction, 0)) == true)
            isJumping = true;

        if (isBeingAttacked == false)
        {
            if (CheckIfAboveWater(new Vector2Int(nodeMap.GetXY(transform.position).x + 1,
                nodeMap.GetXY(transform.position).y)) == true)
            {
                handlingJumpAboveWater = true;
                isJumping = true;
            }
        }
    }
    private void FixedUpdate()
    {
        if (isJumping == true)
        {
            Jump(maxJumpForce, rb);
            StartCoroutine(StopMoveIfAboveLilyPad());
            isJumping = false;
        }
    }

    private bool IsColliding(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 0.1f, groundLayer);

        if (hit.collider != null)
            return true;
        else
            return false;
    }
    public void Jump(float jumpForce, Rigidbody2D rb)
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
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
    private bool CheckIfAboveWater(Vector2Int position)
    {   
        Vector3 worldPosition = nodeMap.GetWorldPosition(position.x, position.y);
        float worldWaterHeight = TerrainManager.Instance.waterLevel * 0.16f;
        float distance = transform.position.y - worldWaterHeight + 0.16f;

        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.down, distance, groundLayer);

        if (hit.collider == null)
            return true;
        else
            return false;
    }
    private IEnumerator StopMoveIfAboveLilyPad()
    {
        while (CheckIfAboveWater(new Vector2Int(nodeMap.GetXY(transform.position).x,
                nodeMap.GetXY(transform.position).y)) == true)
        {
            Move(direction, speed, rb);
            yield return null;
        }

        rb.velocity = new Vector3(0, 0);
        handlingJumpAboveWater = false;
    }
}