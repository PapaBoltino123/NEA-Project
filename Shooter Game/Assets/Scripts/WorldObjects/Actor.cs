using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Actor : WorldObject
{
    public void Move(float moveInput, float speed, Rigidbody2D rb, Animator anim);
    public void Jump(float jumpForce, Rigidbody2D rb);
}
