using System;
using System.AdditionalDataStructures;
using System.AddtionalEventStructures;
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
using UnityEngine.UI;

public class Zombie : MonoBehaviour
{
    private float speed;
    private float maxJumpForce;
    private float cooldown = 0.2f;
    private int direction;
    private Grid<Node> nodeMap = null;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Text damageTextPrefab;
    private bool isCollidingWithPlayer = false;

    float knockback = 0.5f;
    int damagePoints;
    int health = 10;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        speed = ZombieManager.Instance.speed;
        damagePoints = ZombieManager.Instance.damagePoints;
        health = ZombieManager.Instance.healthMax;
        maxJumpForce = ZombieManager.Instance.jumpForce;
        nodeMap = ZombieManager.Instance.nodeMap;

        GameManager.Instance.eventBroadcaster.SendDamage += new EventHandler<DamageEventArgs>(ReceiveHit);
    }

    void Start()
    {
        GameManager.Instance.activePrefabs.Add(gameObject);
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(AttackingCheck());
    }

    void Update()
    {
        if (health < 1)
        {
            GameManager.Instance.activePrefabs.Remove(gameObject);
            ZombieManager.Instance.zombieCount--;
            Player.Instance.score += 50;
            ZombieManager.Instance.SpawnItem(transform.position);
            Destroy(gameObject);
        }

        if (Player.Instance.isPaused == false)
        {
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            int direction = CheckDirection();
            Move(direction, speed, rb);

            if (IsHittingWall() == true)
            {
                Jump(maxJumpForce, rb);
            }
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.velocity = Vector3.zero;
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
    private IEnumerator AttackingCheck()
    {
        while (true)
        {
            if (isCollidingWithPlayer == true)
            {
                Attack();
                isCollidingWithPlayer = false;
                yield return new WaitForSeconds(cooldown);
            }
            yield return null;
        }
    }
    private void Attack()
    {
        Damage dmg = new Damage(damagePoints, knockback, transform.position);
        GameManager.Instance.eventBroadcaster.Hit(this, dmg);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isCollidingWithPlayer = true;
        }
    }
    public void ReceiveHit(object sender, DamageEventArgs e)
    {
        if (sender is Player || sender is Bullet)
        {
            if (Vector2.Distance(transform.position, e.damage.damagePosition) < 0.02f)
            {
                health -= e.damage.damagePoints;
                StartCoroutine(DamageTextIndicator(Convert.ToString(e.damage.damagePoints)));
            }
        }
    }
    private IEnumerator DamageTextIndicator(string damage)
    {
        Text damageText = Instantiate(damageTextPrefab, transform.position, Quaternion.identity).GetComponent<Text>();
        damageText.text = damage;
        yield return new WaitForSeconds(0.3f);
        damageText.text = "";
        Destroy(damageText.gameObject);
    }
}