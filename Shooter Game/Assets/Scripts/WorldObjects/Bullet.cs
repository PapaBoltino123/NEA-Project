using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float bulletSpeed = 3f;
    private float timeAlive = 70f;
    public Vector3 direction = Vector3.zero;

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
                yield break;
            }

            yield return null;
        }
    }
    private bool IsCollidingWithObstacle()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, bulletSpeed * Time.deltaTime);

        if (hit.collider != null)
            return true;
        else
            return false;
    }
}
