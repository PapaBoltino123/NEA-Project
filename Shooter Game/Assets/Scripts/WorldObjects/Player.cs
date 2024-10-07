using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : Actor
{
    [SerializeField] float jumpForce, speed;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Animator weaponController;

    Rigidbody2D rb = new Rigidbody2D();
    Animator anim = new Animator();
    bool isJumping;
    private float moveInput = 0;

    public float MoveInput
    {
        get
        {
            if (moveInput == 0)
                return 1;
            else
                return moveInput;
        }
    }

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
    }
    private void FixedUpdate()
    {
        if (isJumping == true)
        {
            Jump(jumpForce, rb);
            anim.SetBool("grounded", false);
            isJumping = false;
        }
    }
}
