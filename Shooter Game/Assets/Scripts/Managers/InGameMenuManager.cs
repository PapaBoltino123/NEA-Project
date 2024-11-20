using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenuManager : Singleton<InGameMenuManager>
{
    #region Variable Declaration
    [SerializeField] public GameObject[] gameUI;
    [SerializeField] GameObject pauseScreen;
    [SerializeField] Slider audioSlider, fovSlider;
    [SerializeField] GameObject[] menuUI;
    #endregion
    #region Methods
    private void Awake()
    {
        SetUIActivityFalse();
        pauseScreen.SetActive(false);
        audioSlider.value = GameManager.Instance.volume;
        fovSlider.value = GameManager.Instance.zoom;
        audioSlider.onValueChanged.AddListener(OnAudioSliderChanged);
        fovSlider.onValueChanged.AddListener(OnFOVSliderChanged);
    }
    public void LoadPauseMenu()
    {
        foreach (var actor in GameManager.Instance.activeActors)
        {
            Rigidbody2D rb = actor.GetComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        Player.Instance.isPaused = true;
        pauseScreen.SetActive(true);
    }
    public void ClosePauseMenu()
    {
        Player.Instance.isPaused = true;
        pauseScreen.SetActive(false);
        foreach (var actor in GameManager.Instance.activeActors)
        {
            Rigidbody2D rb = actor.GetComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
    public void SaveAndExit()
    {
        StopAllCoroutines();

        foreach (var chunk in ChunkManager.Instance.chunkPool.ToList())
        {
            Destroy(chunk);
        }
        foreach(var chunk in ChunkManager.Instance.activeChunks.Values)
        {
            Destroy(chunk);
        }

        GameManager.Instance.fileManager.SaveGame();
        GameManager.Instance.activeActors.Clear();
        GameManager.Instance.LoadMainMenu();
    }
    private void OnAudioSliderChanged(float value)
    {
        GameManager.Instance.volume = (int)value;
    }
    private void OnFOVSliderChanged(float value)
    {
        GameManager.Instance.zoom = value;
    }
    public void LoadSettings()
    {
        for (int i = 0; i < 3; i++)
        {
            menuUI[i].SetActive(false);
        }
        for (int i = 3; i < menuUI.Length; i++)
        {
            menuUI[i].SetActive(true);
        }
    }
    public void LoadPauseMenuUI()
    {
        for (int i = 0; i < 3; i++)
        {
            menuUI[i].SetActive(true);
        }
        for (int i = 3; i < menuUI.Length; i++)
        {
            menuUI[i].SetActive(false);
        }
    }
    public void SwitchUIActivity()
    {
        foreach (var uiElement in gameUI)
        {
            uiElement.SetActive(!uiElement.activeSelf);
        }
    }
    public void SetUIActivityFalse()
    {
        foreach (var uiElement in gameUI)
        {
            uiElement.SetActive(false);
        }
    }
    #endregion
}
