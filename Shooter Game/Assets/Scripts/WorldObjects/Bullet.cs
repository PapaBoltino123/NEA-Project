using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float bulletSpeed = 3f;
    public float knockBack;
    public int damagePoints = 0;
    private float timeAlive = 70f;
    public Vector3 direction = Vector3.zero;
    [SerializeField] LayerMask playerLayer;

    public Vector3 Direction
    {
        get { return direction; }
        set
        {
            direction = value;
            Shoot();
        }
    }

    private void Shoot()
    {
        GameManager.Instance.activePrefabs.Add(gameObject);
        StartCoroutine(MoveBullet());
    }
    private IEnumerator MoveBullet()
    {
        float timeElapsed = 0f;

        while (true)
        {
            transform.Translate(direction * bulletSpeed * Time.deltaTime, Space.World);

            if (IsCollidingWithObstacle() == true)
            {
                Destroy(gameObject);
                yield break;
            }

            timeElapsed += Time.deltaTime;
            if (timeElapsed >= timeAlive)
            {
                Destroy(gameObject);
                GameManager.Instance.activePrefabs.Remove(gameObject);
                yield break;
            }

            yield return null;
        }
    }
    private bool IsCollidingWithObstacle()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, bulletSpeed * Time.deltaTime, ~playerLayer);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.CompareTag("Zombie"))
                Hit();

            return true;
        }
        else
            return false;
    }

    private void Hit()
    {
        Damage dmg = new Damage(damagePoints, knockBack, transform.position);
        GameManager.Instance.eventBroadcaster.Hit(this, dmg);
    }
}