using System;
using System.AdditionalDataStructures;
using System.AddtionalEventStructures;
using System.Collections;
using System.Collections.Generic;
using System.ItemStructures;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Player : Singleton<Player>, Actor
{
    public float jumpForce, speed;
    public LayerMask groundLayer;

    public int score;
    public int health;
    public bool isPaused = false;

    [SerializeField] GameObject[] bulletPrefabs;
    private Transform firePoint;
    private Ammo bullet;

    private GameObject activeRangedWeapon;
    private GameObject activeMeleeWeapon;
    private GameObject activeHealthItem;
    private GameObject activeItem;

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
        if (isPaused == false)
        {
            float input = Input.GetAxisRaw("Horizontal");

            try
            {
                activeRangedWeapon = GetActiveItemInSlot(InGameMenuManager.Instance.hotBarSlots[(int)HotBarType.RANGED]);
                activeMeleeWeapon = GetActiveItemInSlot(InGameMenuManager.Instance.hotBarSlots[(int)HotBarType.MELEE]);
                activeHealthItem = GetActiveItemInSlot(InGameMenuManager.Instance.hotBarSlots[(int)HotBarType.HEALTH]);

                GameObject activeItemInHotBar = InventoryManager.Instance.activeSlotIndex switch
                {
                    (int)HotBarType.RANGED => activeRangedWeapon,
                    (int)HotBarType.MELEE => activeMeleeWeapon,
                    (int)HotBarType.HEALTH => activeHealthItem
                };

                activeItem = transform.Cast<Transform>()
                    .Select(child => child.gameObject)
                    .ToList()
                    .FirstOrDefault(child => child.name == activeItemInHotBar.name);

                foreach (var item in transform.Cast<Transform>().Select(c => c.gameObject).ToList())
                {
                    item.gameObject.SetActive(false);
                }

                activeItem.SetActive(true);

                if (activeItemInHotBar == activeRangedWeapon)
                {
                    firePoint = activeItem.transform.GetChild(0);
                    RotateGunTowardsMouse();
                }
            }
            catch { }

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

            if (Input.GetMouseButtonDown(0))
            {
                try
                {
                    RangedWeapon weapon = activeItem.GetComponent<ItemType>().item as RangedWeapon;
                    Shoot(weapon);
                }
                catch
                {
                    try
                    {
                        MeleeWeapon weapon = activeItem.GetComponent<ItemType>().item as MeleeWeapon;
                        //Attack(weapon)
                    }
                    catch
                    {
                        HealthPack pack = activeItem.GetComponent<ItemType>().item as HealthPack;
                        StartHealing(pack);
                    }
                }
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
            anim.SetBool("run", false);
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
    public bool IsGrounded()
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
    private void RotateGunTowardsMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 gunDirection = mousePosition - transform.position;
        float angle = Mathf.Atan2(gunDirection.y, gunDirection.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        activeItem.transform.rotation = Quaternion.Slerp(activeRangedWeapon.transform.rotation, targetRotation, 5f);

        float scaleY = activeItem.transform.rotation.z switch
        {
            >90 and < 180 => -1,
            <-90 and > -180 => -1,
            _ => 1
        };

        if (transform.localScale.x > 0)
            activeItem.transform.localScale = new Vector3(-transform.localScale.x, scaleY, 1);
        else
            activeItem.transform.localScale = new Vector3(-transform.localScale.x, -scaleY, 1);
    }
    private void LoadGame(object sender, DataEventArgs e)
    {
        GameData data = e.gameData;
        score = data.score;
        health = data.playerHealth;
        InitializeForGame();
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
        InitializeForGame();
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
    private void InitializeForGame()
    {
        InGameMenuManager.Instance.gameUI.First().GetComponent<Text>().text = $"Score: {score}";
        InGameMenuManager.Instance.gameUI[1].GetComponent<Slider>().value = health;
        InGameMenuManager.Instance.gameUI.Last().GetComponent<Text>().text = Convert.ToString(health);
        InGameMenuManager.Instance.healthFillArea.GetComponent<Image>().color = InGameMenuManager.Instance.SetBarColor(health);
    }
    public void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        GameManager.Instance.fileManager.dataBroadcast.SendLoadedData += new EventHandler<DataEventArgs>(LoadGame);
        GameManager.Instance.fileManager.dataBroadcast.SendNewData += new EventHandler<DataEventArgs>(NewGame);
        GameManager.Instance.fileManager.dataBroadcast.SaveData += new EventHandler<EventArgs>(SaveGame);
    }
    private void Shoot(RangedWeapon weapon)
    {
        GameObject bulletObject = Instantiate(bulletPrefabs[0], firePoint.position, activeRangedWeapon.transform.rotation * Quaternion.Euler(0, 0, 90));
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        bullet.Direction = GetBulletDirection();
    }
    private Vector3 GetBulletDirection()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 gunDirection = mousePosition - transform.position; 
        return gunDirection;
    }
    private GameObject GetActiveItemInSlot(GameObject parent)
    {
        GameObject activeItem = parent.transform.Cast<Transform>()
            .Select(child => child.gameObject)
            .OrderByDescending(child => child.activeSelf)
            .ToList()
            .First();

        return activeItem;
    }
    private void StartHealing(HealthPack pack)
    {
        int healthBoost = pack.healthBoost;
        float effectLength = pack.effectLength;
        StartCoroutine(Heal(healthBoost, effectLength));
    }
    private IEnumerator Heal(int healthBoost, float effectLength)
    {
        Color normalColour = GetComponent<SpriteRenderer>().color;
        Color healingColour = new Color(21f / 255f, 202f / 255f, 0, 120f / 255f);
        float elapsedTime = 0f;
        int currentHealth = health;
        float timeStep = 0.5f;
        int healthPerTimeStep = (int)(healthBoost / effectLength);

        while (health < 100 && elapsedTime < effectLength)
        {
            gameObject.GetComponent<SpriteRenderer>().color = healingColour;
            currentHealth += healthPerTimeStep;
            currentHealth = System.Math.Min(currentHealth, 100);
            health = currentHealth;

            yield return new WaitForSeconds(timeStep);
            elapsedTime += timeStep;
        }
        gameObject.GetComponent<SpriteRenderer>().color = normalColour;
    }
}