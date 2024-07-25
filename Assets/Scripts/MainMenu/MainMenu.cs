using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
    public void Start()
    {
        if (PlayerPrefs.HasKey("playername"))
        {
            global.playerName = PlayerPrefs.GetString("playername");
           
        }
    }
    // Start is called before the first frame update
    public void QuitButtonDialogYes()
    {
        PlayerPrefs.Save();
        Application.Quit();
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Debug.Log("Oyun kapandi");
    }
  
    public void SinglePlay()
    {
        global.PlayerCount = 1;
        global.MultiPlayermi = 1;
        global.IlkAcilis = 1;
        global.LoadingSceneName = "SampleScene";
        SceneManager.LoadScene("SampleScene"); 
        //SceneManager.LoadScene("Loading"); 
    }
    public void MultiPlay()
    {
        global.PlayerCount = 2;
        global.MultiPlayermi = 2;
        global.LoadingSceneName = "Multiplayer";
        SceneManager.LoadScene("Multiplayer");
        SceneManager.LoadScene("Multiplayer");
    }

}
