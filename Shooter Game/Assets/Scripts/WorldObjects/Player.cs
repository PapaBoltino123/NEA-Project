using System;
using System.AdditionalDataStructures;
using System.AddtionalEventStructures;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Player : Singleton<Player>, Actor
{
    public float jumpForce, speed;
    public LayerMask groundLayer;

    public int score = 0;
    public int health = 50;
    public bool isPaused = false;

    [NonSerialized] public Rigidbody2D rb = new Rigidbody2D();
    private BoxCollider2D boxCollider;
    Animator anim = new Animator();
    bool isJumping;
    private float moveInput = 0;
    public PlayerState playerState = PlayerState.ALIVE;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        GameManager.Instance.fileManager.dataBroadcast.SendLoadedData += new EventHandler<DataEventArgs>(LoadGame);
        GameManager.Instance.fileManager.dataBroadcast.SendNewData += new EventHandler<DataEventArgs>(NewGame);
        GameManager.Instance.fileManager.dataBroadcast.SaveData += new EventHandler<EventArgs>(SaveGame);
    }
    private void Update()
    {
        float input = Input.GetAxisRaw("Horizontal");

        if (input > 0)
            moveInput = input;
        else if (input < 0)
            moveInput = input;
        Move(input, speed, rb, anim);

        if (IsGrounded() == true)
        {
            anim.SetBool("grounded", true);

            if (Input.GetKeyDown(KeyCode.Space))
                isJumping = true;
        }

        InGameMenuManager.Instance.gameUI.First().GetComponent<Text>().text = $"Score: {score}";
        InGameMenuManager.Instance.gameUI[1].GetComponent<Slider>().value = health;
        InGameMenuManager.Instance.gameUI.Last().GetComponent<Text>().text = Convert.ToString(health);
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
    private void LoadGame(object sender, DataEventArgs e)
    {
        GameData data = e.gameData;
        score = data.score;
        health = data.playerHealth;
        Initialize();
    }
    private void SaveGame(object sender, EventArgs e)
    {
        GameManager.Instance.savedData.playerHealth = health;
        GameManager.Instance.savedData.score = score;
    }
    private void NewGame(object sender, DataEventArgs e)
    {
        GameData data = e.gameData;
        score = data.score;
        health = data.playerHealth;
        Initialize();
    }
    public void BeginUpdatingScore()
    {
        StartCoroutine(UpdateScore());
    }
    public void EndUpdatingScore()
    {
        StopCoroutine(UpdateScore());
    }
    private IEnumerator UpdateScore()
    {
        while (true)
        {
            if (!isPaused)
            {
                score += 10;
            }
            yield return new WaitForSeconds(1f);
        }
    }
    private void Initialize()
    {
        InGameMenuManager.Instance.gameUI.First().GetComponent<Text>().text = $"Score: {score}";
        InGameMenuManager.Instance.gameUI[1].GetComponent<Slider>().value = health;
        InGameMenuManager.Instance.gameUI.Last().GetComponent<Text>().text = Convert.ToString(health);
        InGameMenuManager.Instance.healthFillArea.GetComponent<Image>().color = InGameMenuManager.Instance.SetBarColor(health);
        GameManager.Instance.activeActors.Add(gameObject);
    }
}