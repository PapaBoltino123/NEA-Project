using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.AdditionalDataStructures;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] GameObject loadingScreen;
    public List<GameObject> activeActors;
    public int volume = 50;
    public float zoom = 1.5f;
    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

    private void Awake()
    {
        activeActors = new List<GameObject>();
        loadingScreen.SetActive(false);
        SceneManager.LoadSceneAsync((int)SceneType.TITLESCREEN, LoadSceneMode.Additive);
    }
    public void LoadGame()
    {
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.UnloadSceneAsync((int)SceneType.TITLESCREEN));
        scenesLoading.Add(SceneManager.LoadSceneAsync((int)SceneType.MAINGAME, LoadSceneMode.Additive));

        StartCoroutine(GetMainSceneLoadProgress());
    }

    public IEnumerator GetMenuSceneLoadProgress()
    {
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
    public IEnumerator GetMainSceneLoadProgress()
    {
        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                yield return null;
            }
        }

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
