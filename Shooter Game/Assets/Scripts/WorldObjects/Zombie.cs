using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
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

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        speed = ZombieManager.Instance.speed;
        maxJumpForce = ZombieManager.Instance.jumpForce;
        nodeMap = ZombieManager.Instance.nodeMap;
    }

    void Start()
    {
        GameManager.Instance.activePrefabs.Add(gameObject);
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Determine if the zombie should change direction based on the player's position
        int direction = CheckDirection();
        Move(direction, speed, rb);

        // Check for walls and jump if needed
        if (IsHittingWall() == true)
        {
            Jump(maxJumpForce, rb);
        }
    }

    private int CheckDirection()
    {
        return Player.Instance.transform.position.x > transform.position.x ? 1 : -1;
    }

    private bool IsHittingWall()
    {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.right * CheckDirection();
        float distance = 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer);
        return hit.collider != null;
    }
    private bool IsApproachingWater()
    {
        float groundCheckDistance = 0.16f;
        float jumpRange = 10 * 0.16f;
        Vector2 position = transform.position + Vector3.right * CheckDirection() * groundCheckDistance;
        RaycastHit2D groundHit = Physics2D.Raycast(position, Vector2.down, groundCheckDistance, groundLayer);

        // If no ground ahead, check for a lily pad
        if (!groundHit.collider)
        {
            return true;
        }

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
}