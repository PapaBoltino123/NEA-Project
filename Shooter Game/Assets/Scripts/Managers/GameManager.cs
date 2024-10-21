using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.AdditionalDataStructures;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] GameObject loadingScreen;
    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

    private void Awake()
    {
        loadingScreen.SetActive(false);
        SceneManager.LoadSceneAsync((int)SceneType.TITLESCREEN, LoadSceneMode.Additive);
    }
    public void LoadGame()
    {
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.UnloadSceneAsync((int)SceneType.TITLESCREEN));
        scenesLoading.Add(SceneManager.LoadSceneAsync((int)SceneType.MAINGAME, LoadSceneMode.Additive));

        StartCoroutine(GetSceneLoadProgress());
    }

    public IEnumerator GetSceneLoadProgress()
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
    }
}
