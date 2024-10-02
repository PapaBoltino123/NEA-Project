using System;
using System.Collections;
using System.Collections.Generic;
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

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        weaponController.SetBool("display axe", true);
        float moveInput = Input.GetAxisRaw("Horizontal");
        Move(moveInput, speed, rb, anim);
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
