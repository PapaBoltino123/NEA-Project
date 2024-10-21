using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject zombiePrefab;
    List<GameObject> actors;
    bool canSpawn = true;
    void Start()
    {
        actors = new List<GameObject>();
        StartCoroutine(SpawnActors(2f, 0.2f, zombiePrefab, playerPrefab));
    }
    void Update()
    {
        if (canSpawn == true)
            StartCoroutine(SpawnActors(2f, 0.2f, zombiePrefab, playerPrefab));
    }
    private IEnumerator DestroyPlayer(float delay, GameObject actor, GameObject playerController)
    {
        yield return new WaitForSeconds(delay);
        Destroy(actor);
        actors.Remove(actor);

        if (actors.Count == 0)
        {
            canSpawn = true;
        }
    }
    private IEnumerator SpawnActors(float playerDelay, float zombieDelay, GameObject zombieController, GameObject playerController)
    {
        canSpawn = false;
        GameObject actor;
        actor = Instantiate(playerController);
        actors.Add(actor);
        StartCoroutine(DestroyPlayer(8f, actor, playerController));
        yield return new WaitForSeconds(playerDelay);
        actor = Instantiate(zombieController);
        actors.Add(actor);
        StartCoroutine(DestroyZombie(8f, actor, zombieController));
        yield return new WaitForSeconds(zombieDelay);
        actor = Instantiate(zombieController);
        actors.Add(actor);
        StartCoroutine(DestroyZombie(8f, actor, zombieController));
        yield return new WaitForSeconds(zombieDelay);
        actor = Instantiate(zombieController);
        actors.Add(actor);
        StartCoroutine(DestroyZombie(8f, actor, zombieController));
    }
    private IEnumerator DestroyZombie(float delay, GameObject actor, GameObject controller)
    {
        yield return new WaitForSeconds(delay);
        Destroy(actor);
        actors.Remove(actor);

        if (actors.Count == 0)
        {
            canSpawn = true;
        }
    }
    public void LoadGame()
    {
        StopAllCoroutines();

        foreach (var actor in actors)
        {
            Destroy(actor);
        }

        GameManager.Instance.LoadGame();
    }
}
