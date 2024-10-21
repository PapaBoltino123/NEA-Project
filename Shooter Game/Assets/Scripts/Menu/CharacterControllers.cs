using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllers : MonoBehaviour
{
    [SerializeField] LayerMask groundLayer;
    private Rigidbody2D rb;
    Animator anim = new Animator();
    bool isJumping;
    float speed, jumpForce;
    Vector3 spawnPosition = new Vector3(-5.13f, 0.65f, 0);

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        speed = 3; jumpForce = 5;

        transform.position = spawnPosition;
    }

    void Update()
    {
        rb.velocity = new Vector3(speed , rb.velocity.y);
        anim.SetBool("run", rb.velocity.x != 0);

        bool isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, groundLayer);

        if (isGrounded == true)
        {
            anim.SetBool("grounded", true);

            if (rb.velocity.x == 0)
                isJumping = true;
        }
    }
    private void FixedUpdate()
    {
        if (isJumping == true)
        {
            anim.SetBool("grounded", false);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = false;
        }
    }
}
