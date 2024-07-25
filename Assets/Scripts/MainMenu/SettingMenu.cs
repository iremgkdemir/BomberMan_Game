using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour
{
    public Transform settingsMenu;
    public CanvasGroup background;
    public static bool isSettingMenuShown;
    public AudioManager audioManager;
    public InputField inpPlayerName;

    void Start()
    {
        settingsMenu.gameObject.SetActive(false);
        background.alpha = 0;
        settingsMenu.localPosition = new Vector2(0, -Screen.height);
        Time.timeScale = 1f; // Ensure the game starts unpaused
        isSettingMenuShown = false; // Ensure the game starts in an unpaused state
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Log("[Pause aialog] ESC tusuna basildi");
            if (isSettingMenuShown)
            {
                CloseDialog();
            }
        }
    }

    public void ShowSettingsMenu()
    {
        Debug.Log("Openning");
        settingsMenu.gameObject.SetActive(true);
        //Cursor.lockState = CursorLockMode.None;
        background.gameObject.SetActive(true);
        isSettingMenuShown = true;
        //global.isLocalGamePaused.Value = true;
        background.alpha = 0;
        background.LeanAlpha(1, 0.5f);
        background.gameObject.SetActive(true);

        settingsMenu.localPosition = new Vector2(0, -(Screen.height * 2));
        settingsMenu.LeanMoveLocalY(0, 0.1f).setEaseOutExpo();
        audioManager.SettingsMenuOn = true;
        if (PlayerPrefs.HasKey("playername"))
        {
            global.playerName = PlayerPrefs.GetString("playername");
            inpPlayerName.text = global.playerName;
        }
    }

    public void CloseDialog()
    {
        Debug.Log("Closing");
        isSettingMenuShown = false;
        //global.isLocalGamePaused.Value = false;
        background.LeanAlpha(0, 0.5f);
        settingsMenu.LeanMoveLocalY(-(Screen.height * 3), 0.5f).setEaseInExpo();
        //settingsMenu.gameObject.SetActive(false);
        background.gameObject.SetActive(false);
        global.playerName = inpPlayerName.text;
        PlayerPrefs.SetString("playername", global.playerName);
        Time.timeScale = 1f;
        audioManager.SettingsMenuOn = false;
    }

    public void ExitButton()
    {
        Application.Quit();
        Debug.Log("Oyun kapandi");
    }

    public void ContinueButton()
    {
        CloseDialog();
    }

    public void RestartButton()
    {
        CloseDialog();   // Pause menüsünü kapat
        // Oyunu yeniden baþlat
        global.PlayMod.Value = global.Mods.StartNewGame;

    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnComplete()
    {
        Time.timeScale = 0f;
    }
}
