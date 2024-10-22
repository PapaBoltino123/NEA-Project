using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    #region Variable Declaration
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject zombiePrefab;
    [SerializeField] GameObject[] initialMenuUI;
    [SerializeField] Slider audioSlider, fovSlider; 
    List<GameObject> actors;
    bool canSpawn = true;
    #endregion
    #region Methods
    void Start()
    {
        audioSlider.value = GameManager.Instance.volume;
        fovSlider.value = GameManager.Instance.zoom;
        actors = new List<GameObject>();
        audioSlider.onValueChanged.AddListener(OnAudioSliderChanged);
        fovSlider.onValueChanged.AddListener(OnFOVSliderChanged);

        LoadMainMenu();
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
    public void ExitApplication()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
    public void LoadSettings()
    {
        for (int i = 0; i < 4; i++)
        {
            initialMenuUI[i].SetActive(false);
        }
        for (int i = 4; i < initialMenuUI.Length; i++)
        {
            initialMenuUI[i].SetActive(true);
        }
    }
    public void LoadMainMenu()
    {
        for (int i = 0; i < 4; i++)
        {
            initialMenuUI[i].SetActive(true);
        }
        for (int i = 4; i < initialMenuUI.Length; i++)
        {
            initialMenuUI[i].SetActive(false);
        }
    }
    private void OnAudioSliderChanged(float value)
    {
        GameManager.Instance.volume = (int)value;
    }
    private void OnFOVSliderChanged(float value)
    {
        GameManager.Instance.zoom = value;
    }
    #endregion
}
