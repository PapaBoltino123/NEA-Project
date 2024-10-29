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
    [SerializeField] GameObject[] startingMenuUI;
    [SerializeField] Slider audioSlider, fovSlider;
    [SerializeField] Text fileLocationText;
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
        foreach (var gameObject in startingMenuUI)
        {
            gameObject.SetActive(false);
        }
    }
    public void LoadStartingOptionsMenu()
    {
        for (int i = 0; i < 4; i++)
        {
            initialMenuUI[i].SetActive(false);
        }
        string path = GameManager.Instance.fileManager.RecentFile();

        if (path == null)
        {
            startingMenuUI[0].SetActive(false);
            fileLocationText.text = "No file located.\nStart new game?";
            for (int i = 1; i < startingMenuUI.Length; i++)
            {
                startingMenuUI[i].SetActive(true);
            }

            startingMenuUI[startingMenuUI.Length - 1].GetComponent<RectTransform>().localPosition = new Vector3(-847, 145, 0);
        }
        else
        {
            fileLocationText.text = "A file was located.\nLoad save or new game?";
            for (int i = 0; i < startingMenuUI.Length; i++)
            {
                startingMenuUI[i].SetActive(true);
            }
            startingMenuUI[startingMenuUI.Length - 1].GetComponent<RectTransform>().localPosition = new Vector3(-847, 60, 0);
        }
    }
    public void StartGame()
    {
        StopAllCoroutines();

        foreach (var actor in actors)
        {
            Destroy(actor);
        }

        GameManager.Instance.LoadGame(false);
    }
    public void NewGame()
    {
        StopAllCoroutines();

        foreach (var actor in actors)
        {
            Destroy(actor);
        }

        GameManager.Instance.LoadGame(true);
    }
    public void CloseStartingOptionsMenu()
    {
        foreach (GameObject gameObject in startingMenuUI)
        {
            gameObject.SetActive(false);
        }
        for (int i = 0; i < 4; i++)
        {
            initialMenuUI[i].SetActive(true);
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
