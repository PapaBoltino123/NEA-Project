using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : Singleton<Player>, Actor
{
    public float jumpForce, speed;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Animator weaponController;

    [NonSerialized] public Rigidbody2D rb = new Rigidbody2D();
    Animator anim = new Animator();
    bool isJumping;
    private float moveInput = 0;
    public PlayerState playerState = PlayerState.ALIVE;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        weaponController.SetBool("display axe", true);
        float input = Input.GetAxisRaw("Horizontal");
        if (input > 0)
            moveInput = input;
        else if (input < 0)
            moveInput = input;
        Move(input, speed, rb, anim);
        bool isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, groundLayer);

        if (isGrounded == true)
        {
            anim.SetBool("grounded", true);

            if (Input.GetKeyDown(KeyCode.Space))
                isJumping = true;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Pathfinder p = new Pathfinder();
            p.FindPath();
        }
    }
    private void FixedUpdate()
    {
        if (isJumping == true)
        {
            anim.SetBool("grounded", false);
            Jump(jumpForce, rb);
            isJumping = false;
        }
    }
    public void Move(float moveInput, float speed, Rigidbody2D rb, Animator anim)
    {
        rb.velocity = new Vector3(moveInput * speed, rb.velocity.y);
        if (moveInput > 0)
        {
            transform.localScale = Vector3.one;
        }
        if (moveInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        anim.SetBool("run", moveInput != 0);
    }
    public void Jump(float jumpForce, Rigidbody2D rb)
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }
}
public enum PlayerState
{
    ALIVE, 
    DEAD
}