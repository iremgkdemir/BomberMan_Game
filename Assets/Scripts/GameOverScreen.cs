using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static global;

public class GameOverScreen : MonoBehaviour
{

    /// <summary>
    /// Oyun esnas�nda GameOver ekran�n�n fonksiyonlar�n� i�erir
    /// </summary>

    public GameObject gameOverBackScreen;
    public Text txtScore;
    void Start()
    {
        
    }

    
    void Update()
    {
        // Oyun modu GameOver mod olmu�sa GameOver i�lemlerini ba�lat
        // Debug.Log("Oyun sonu ekran� ba�lat�l�yor");
        if (global.PlayMod.Value == global.Mods.GameOver)
        {
            GameOver();
        }

    }

    public void GameOver()
    {
        // Game over penceresi a��ld� Oyun modunu de�i�tir.
        global.PlayMod.Value = global.Mods.GameOverOpenned;
        gameOverBackScreen.SetActive(true);  // GameOver penceresini g�r�n�r yap
        txtScore.text = "Your Score: " + global.Player1.GetComponent<playerMovement>().score.ToString() + "!";
    }
    public void MainMenuButton()
    {
        // Ana men�ye d�n
        SceneManager.LoadScene("MainMenu");
    }
    public void RestartButton()
    {
        gameOverBackScreen.SetActive(false);
        // Oyunu yeniden ba�lat
        global.PlayMod.Value = global.Mods.StartNewGame;
        //global.BeginGame();
    }
}
