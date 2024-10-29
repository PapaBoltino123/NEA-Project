using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.AdditionalDataStructures;
using System.AddtionalEventStructures;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] GameObject loadingScreen;
    [SerializeField] GameObject player;
    public List<GameObject> activeActors;
    public int volume = 50;
    public float zoom = 1.5f;
    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
    public FileManager fileManager;
    private EventBroadcaster eventBroadcaster;
    public GameData savedData = new GameData();

    private void Awake()
    {
        player.SetActive(false);
        eventBroadcaster = new EventBroadcaster();
        fileManager = new FileManager(eventBroadcaster);
        activeActors = new List<GameObject>();
        loadingScreen.SetActive(false);
        SceneManager.LoadSceneAsync((int)SceneType.TITLESCREEN, LoadSceneMode.Additive);
    }
    public void LoadGame(bool isNewGame)
    {
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.UnloadSceneAsync((int)SceneType.TITLESCREEN));
        scenesLoading.Add(SceneManager.LoadSceneAsync((int)SceneType.MAINGAME, LoadSceneMode.Additive));

        StartCoroutine(GetMainSceneLoadProgress(isNewGame));
    }

    public IEnumerator GetMenuSceneLoadProgress()
    {
        player.SetActive(false);

        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                yield return null;
            }
        }

        loadingScreen.SetActive(false);
        scenesLoading.Clear();
    }
    public IEnumerator GetMainSceneLoadProgress(bool isNewGame)
    {
        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                yield return null;
            }
        }

        player.SetActive(true);
        
        if (isNewGame == true)
            fileManager.NewGame();
        else
            fileManager.LoadGame();
        yield return new WaitForSeconds(7f);
        loadingScreen.SetActive(false);
        InGameMenuManager.Instance.pauseButton.SetActive(true);
        scenesLoading.Clear();
    }
    public void LoadMainMenu()
    {
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.UnloadSceneAsync((int)SceneType.MAINGAME));
        scenesLoading.Add(SceneManager.LoadSceneAsync((int)SceneType.TITLESCREEN, LoadSceneMode.Additive));

        StartCoroutine(GetMenuSceneLoadProgress());
    }
}
