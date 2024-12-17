using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenuManager : Singleton<InGameMenuManager>
{
    #region Variable Declaration
    [SerializeField] public GameObject[] gameUI;
    [SerializeField] private Sprite[] slotState;

    public GameObject pauseScreen, inventoryScreen, healthFillArea;
    public GameObject[] hotBarSlots;
    public GameObject[] healthSlots;
    public GameObject[] ammoSlots;
    public GameObject[] rangedSlots;
    public GameObject[] meleeSlots;

    [SerializeField] Slider audioSlider, fovSlider;
    private Slider healthBar;
    [SerializeField] GameObject[] menuUI;
    #endregion
    #region Methods
    private void Awake() //upon loading the main game scene
    {
        SetUIActivityFalse(); //the ui must not be seen behind the loading screen
        pauseScreen.SetActive(false); //the pause screen is not active
        inventoryScreen.SetActive(false);

        audioSlider.value = GameManager.Instance.volume; //the value of the sliders in the pause menu must be equal to the values saved previosuly
        fovSlider.value = GameManager.Instance.zoom;
        healthBar = gameUI[1].GetComponent<Slider>();
        healthBar.onValueChanged.AddListener(OnHealthBarValueChanged); //adds listeners to perform functions if the slider value changes
        audioSlider.onValueChanged.AddListener(OnAudioSliderChanged);
        fovSlider.onValueChanged.AddListener(OnFOVSliderChanged);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch(Player.Instance.isPaused)
            {
                case true:
                    ClosePauseMenu();
                    break;
                case false:
                    LoadPauseMenu();
                    break;        
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            switch (inventoryScreen.activeSelf)
            {
                case true:
                    CloseInventory();
                    break;
                case false:
                    LoadInventory();
                    break;
            }
        }

        if (Player.Instance.isPaused == false)
        {
            foreach (var slot in hotBarSlots)
            {
                if (slot == InventoryManager.Instance.activeSlot)
                    slot.GetComponent<Image>().sprite = slotState[1];
                else
                    slot.GetComponent<Image>().sprite = slotState[0];
            }
        }
    }
    public void LoadPauseMenu()
    {
        inventoryScreen.SetActive(false);
        Player.Instance.isPaused = true; //the player is paused
        pauseScreen.SetActive(true); //activate the pause screen
    }
    public void ClosePauseMenu()
    {
        Player.Instance.isPaused = false; //the player is unpaused
        pauseScreen.SetActive(false); //hide the pause screen
    }
    public void LoadInventory()
    {
        ClosePauseMenu();
        SwitchUIActivity();
        inventoryScreen.SetActive(true);
    }
    public void CloseInventory()
    {
        SwitchUIActivity();
        inventoryScreen.SetActive(false);
    }
    public void SaveAndExit() //when the player presses to save and exit
    {
        StopAllCoroutines(); //stop all routines from running

        foreach (var chunk in ChunkManager.Instance.chunkPool.ToList()) //delete all chunks
        {
            Destroy(chunk);
        }
        foreach(var chunk in ChunkManager.Instance.activeChunks.Values)
        {
            Destroy(chunk);
        }

        GameManager.Instance.fileManager.SaveGame(); //save the game data
        GameManager.Instance.LoadMainMenu(); //load the main menu scene
    }
    private void OnAudioSliderChanged(float value)
    {
        GameManager.Instance.volume = (int)value; //set the game volume to be equal to the slider value
    }
    private void OnFOVSliderChanged(float value)
    {
        GameManager.Instance.zoom = value; //set the game zoom to be equal to the slider value
    }
    public void LoadSettings()
    {
        for (int i = 0; i < 3; i++)
        {
            menuUI[i].SetActive(false); //hide the initial menu ui
        }
        for (int i = 3; i < menuUI.Length; i++)
        {
            menuUI[i].SetActive(true); //display the settings ui
        }
    }
    public void LoadPauseMenuUI()
    {
        for (int i = 0; i < 3; i++)
        {
            menuUI[i].SetActive(true); //load the initial menu ui
        }
        for (int i = 3; i < menuUI.Length; i++)
        {
            menuUI[i].SetActive(false); //hide the settings ui
        }

    }
    public void SwitchUIActivity()
    {
        foreach (var uiElement in gameUI)
        {
            uiElement.SetActive(!uiElement.activeSelf); //sets the ui's activity to the opposite of it's current activity
        }
    }
    public void SetUIActivityFalse()
    {
        foreach (var uiElement in gameUI)
        {
            uiElement.SetActive(false); //hides the ui from being seen when loading screen is active 
        }
    }
    private void OnHealthBarValueChanged(float value)
    {
        int health = (int)value;

        healthFillArea.GetComponent<Image>().color = SetBarColor(health); //sets the colour of the healthbar depending on the current health of the player
    }
    public Color SetBarColor(int health)
    {
        Color green = new Color(21f / 255f, 202f / 255f, 0); //loads colours based on rgb values
        Color red = new Color(219f / 255f, 0, 0);
        Color yellow = new Color(186f / 255f, 202f / 255f, 0);

        Color returnColor = health switch //uses a switch statement to determine colour
        {
            > 50 => green, //green if above 50
            < 51 and > 20 => yellow, //yellow if above 20
            < 21 => red //red if below that
        };

        return returnColor; //returns said colour
    }
    public void OnItemSelected()
    {

    }
    #endregion
}
