using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Player1ItemInfoUi : MonoBehaviour
{
    /// <summary>
    /// Trinty 1.0
    /// Oyuncu bilgilerini Kullan�c� panelinde g�steren fonksiyonlar
    /// </summary>

    public Text txtBombCount;           // Ekrandaki Bomba say�s� 
    public Text txtBombLength;          // Ekrandaki Bomba uzunlu�u 
    public Text txtSpeed;               // Ekrandaki Oyuncu h�z� 
    public Text txtScore;               // Ekrandaki Hayalet fonksiyonu 
    public Text txtPlayerName;               // Ekrandaki Hayalet fonksiyonu 

    public GameObject playerStateUi;    // Oyuncu durum paneli

    // Oyun Ba�lad���nda
    void Start()
    {
        
    }

    // Her frame de
    void Update()
    {
        // Bekleme modundaysa i�lem yapma
        if (global.PlayMod.Value == global.Mods.Waiting ||
            global.PlayMod.Value == global.Mods.TimeOver ||
            global.PlayMod.Value == global.Mods.GameOver ||
            global.PlayMod.Value == global.Mods.Ending ||
            global.PlayMod.Value == global.Mods.StartNewGame ||
            global.PlayMod.Value == global.Mods.WaitingForPlayer) return;
        // Player1 Yoksa ��k.
        if (global.Player1 == null) return;
        // Bomba say�s� uzunlu�u vb ekranda g�ncelle
        txtBombCount.text = "x" + global.Player1.GetComponent<playerMovement>().bombCount.ToString();
        txtBombLength.text = "x" + global.Player1.GetComponent<playerMovement>().bombLength.ToString();
        txtSpeed.text = "x" + global.Player1.GetComponent<playerMovement>().speed.ToString();
        txtPlayerName.text = global.playerName + global.Player1.GetComponent<playerMovement>().OwnerClientId.ToString(); ;
        txtScore.text = global.Player1.GetComponent<playerMovement>().score.ToString();
    }
}
