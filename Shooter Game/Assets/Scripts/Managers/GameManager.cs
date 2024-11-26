using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.AdditionalDataStructures;
using System.AddtionalEventStructures;
using System.Threading;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] GameObject player;
    public List<GameObject> activeActors;
    public List<Thread> activeThreads;

    public int volume = 50;
    public float zoom = 1.5f;
    public float loadProgress = 0;

    [SerializeField] GameObject loadingScreen;
    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

    public FileManager fileManager;
    private EventBroadcaster eventBroadcaster;
    public GameData savedData = new GameData();
    public byte[,] grid = null;

    public bool mainGameLoaded;

    private void Awake()
    {
        activeThreads = new List<Thread>();
        eventBroadcaster = new EventBroadcaster();
        fileManager = new FileManager(eventBroadcaster);
        Player.Instance.Initialize();
        activeActors = new List<GameObject>();
        player.SetActive(false);
        loadingScreen.SetActive(false);
        SceneManager.LoadSceneAsync((int)SceneType.TITLESCREEN, LoadSceneMode.Additive);
        mainGameLoaded = false;
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
        float startTime = Time.time;
        float totalTime = 0f;
        loadProgress = 0f;
        player.SetActive(false);

        for (int i = 0; i < scenesLoading.Count; i++)
        {
            float sceneStartTime = Time.time;

            while (!scenesLoading[i].isDone)
            {
                yield return null;
            }

            totalTime += Time.time - sceneStartTime;
            float elapsedTime = Time.time - startTime;
            loadProgress = Mathf.Clamp01(elapsedTime / (totalTime + 7f));
        }

        mainGameLoaded = false;
        loadingScreen.SetActive(false);
        scenesLoading.Clear();
    }
    public IEnumerator GetMainSceneLoadProgress(bool isNewGame)
    {
        float startTime = Time.time;
        float totalTime = 0f;
        loadProgress = 0f;

        for (int i = 0; i < scenesLoading.Count; i++)
        {
            float sceneStartTime = Time.time;

            while (!scenesLoading[i].isDone)
            {
                yield return null;
            }

            totalTime += Time.time - sceneStartTime;
            float elapsedTime = Time.time - startTime;
            loadProgress = Mathf.Clamp01(elapsedTime / (totalTime + 7f));
        }

        mainGameLoaded = true;

        InGameMenuManager.Instance.SetUIActivityFalse();

        int maxSize = InGameMenuManager.Instance.ammoSlots.Length +
    InGameMenuManager.Instance.healthSlots.Length +
    InGameMenuManager.Instance.rangedSlots.Length +
    InGameMenuManager.Instance.meleeSlots.Length;
        InventoryManager.Instance.MaxSize = maxSize;
        InventoryManager.Instance.NewGame();
        InventoryManager.Instance.hotBarSlots = InGameMenuManager.Instance.hotBarSlots;
        InventoryManager.Instance.activeSlotIndex = (int)HotBarType.RANGED;
        player.SetActive(true);

        if (isNewGame == true)
            fileManager.NewGame();
        else
            fileManager.LoadGame();

        ZombieManager.Instance.pathfinder = TerrainManager.Instance.pathfinder;
        ZombieManager.Instance.nodeMap = TerrainManager.Instance.ReturnWorldMap();

        yield return new WaitForSeconds(7f);

        Player.Instance.rb.constraints = RigidbodyConstraints2D.None;
        Player.Instance.rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        loadingScreen.SetActive(false);
        InGameMenuManager.Instance.SwitchUIActivity();
        Player.Instance.isPaused = false;
        Player.Instance.BeginUpdatingScore();
        scenesLoading.Clear();
    }
    public void LoadMainMenu()
    {
        InGameMenuManager.Instance.SwitchUIActivity();
        InventoryManager.Instance.hotBarSlots = null;
        Player.Instance.EndUpdatingScore();
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.UnloadSceneAsync((int)SceneType.MAINGAME));
        scenesLoading.Add(SceneManager.LoadSceneAsync((int)SceneType.TITLESCREEN, LoadSceneMode.Additive));

        StartCoroutine(GetMenuSceneLoadProgress());
    }
    private void OnApplicationQuit()
    {
        foreach (var thread in activeThreads)
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
            }
        }
    }
}
