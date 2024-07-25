using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
//using static UnityEditor.PlayerSettings;

public class global : NetworkBehaviour
{
    /// <summary>
    /// Trinity 1.0
    /// Global tan�mlamalar�n yap�ld��� kod alan�
    /// </summary>

    // Haritadaki nesnelerin tan�mlar�
    // Bos     0
    // Player  1
    // Kaya    2
    // Kutu    3
    // Canavar 4

    public enum Nesne
    {
        Bos = 0,
        Player = 1,
        Kaya = 2,
        Kutu = 3,
        Canavar = 4,
        Bomba = 5,
        Duvar = 6
    }

    // O anki oyun modu durumlar�
    public enum Mods
    {
        Waiting = 0,
        Playing = 1,
        Pause = 2,
        TimeOver  = 3,
        Ending = 4,
        GameOver = 5,
        GameOverOpenned = 6,
        Paused = 7,
        StartNewGame = 8,
        WaitingForPlayer = 9,
        WaitingToStart = 10
    }
    [SerializeField] public static Image bombIcon;                 // Bomba ikonu efekti i�in
    public static GameObject MainCamera;               // Tepe kameras�
    public Image gameOverScreen;      // Oyun sonu diyalog kutusu i�lemleri
    public GameObject NetworkManagerUI;             // NetworkManagerUI
    public GameObject DebugConsole;             // InGameDebugConsole
    public GameObject WaitingUI;
    public static string LoadingSceneName;
    public bool debugMod;
    public const float genislikKatsayisi = 1;          // Prefab geni�li�i hesaplar� i�in
    public static int en = 20;                         // Harita En
    public static int boy = 20;                        // Harita Boy
    public static int KutuSayisi = 70;                 // Kutu say�s�
    public static int MaksKutuSayisi = 50;                 // Kutu say�s�
    public static int MinKutuSayisi = 75;                 // Kutu say�s�
    public static int CanavarSayisi = 0;              // Canavar1 say�s�
    public static int MaksCanavarSayisi = 4;              // Canavar1 say�s�
    public static int MinCanavarSayisi = 7;              // Canavar1 say�s�
    public static int CountDownTime = 3;
    public static int PlayerCount;                      // Multiplayer mi
    public static int MultiPlayermi;                    // Multiplayer mi
    public static int IlkAcilis;                        // �lk a��l�� m�
    public static int CurrentPlayerCount;
   // public static int Multiplayer;                         // Multiplayer mi
    public static Nesne[,] map;                        // Harita Nesneleri matrisi
    
    // NetworkVariables --------
    //public static Mods PlayMod.Value = Mods.StartNewGame;         // Oyun Modunu tutar
    public static NetworkVariable<Mods> PlayMod = new NetworkVariable<Mods>(Mods.StartNewGame);         // Oyun Modunu tutar

    public static NetworkVariable<bool> isLocalGamePaused = new NetworkVariable<bool>(false);
    public static bool isLocalPlayerReady;
    public static int MaxPlayTime = 150;               // Oyun s�resi
    [SerializeField] public TextMeshProUGUI txtTimer;  // Saat Text nesnesi
    float elapsedTime;                                 // Her FPS de ge�en s�reyi tutar
    int minute, second;                                // Ge�en s�reyi Dakika ve Saniye olarak tutar
    public static AudioManager audioManager;           // Ses olaylar� y�neticisi
    [SerializeField] private NavMeshSurface navMeshSurface;    // Baking i�lemleri i�in


    [SerializeField] public GameObject kayaPrefab; // Prefab for rocks
    public GameObject duvarPrefab; // Prefab for boxes
    public GameObject kutuPrefab; // Prefab for boxes
    public GameObject canavar1Prefab; // Prefab for monsters
    public GameObject canavar2Prefab; // Prefab for monsters
    public GameObject canavar3Prefab; // Prefab for monsters
    public GameObject oyuncuPrefab; // Prefab for player
    public Terrain zemin;

    [SerializeField] private List<GameObject> spawnedBoxList = new List<GameObject>();
    [SerializeField] private List<GameObject> spawnedEnemyList = new List<GameObject>();

    // Playerlar i�in de�i�kenler
    private Dictionary<ulong, bool> playerReadyDict;
    private Dictionary<ulong, bool> playerPauseDict;

    public static playerMovement parent;

    public static GameObject Player1;          // Oyuncuya eri�mek i�in global de�i�ken

    public MeshRenderer meshRenderer;
    public static string playerName;
    private NetworkVariable<FixedString128Bytes> networkPlayerName = new NetworkVariable<FixedString128Bytes>(
        "Player: 0", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public List<Color> colors = new List<Color>();
    private void Awake()
    {
        // Instance = this;
        //PlayMod.Value = Mods.StartNewGame;
        playerReadyDict = new Dictionary<ulong, bool>();
        playerPauseDict = new Dictionary<ulong, bool>();
        meshRenderer = GetComponentInChildren<MeshRenderer>(); // Oyuncunun i�indeki k�p bile�enin meshrenderirine eri�mek i�in

    }

    void Start()
    {
        global.map = new global.Nesne[global.en, global.boy];
        // Ses y�neticisi bulunuyor. Tag: AudioTag 
        audioManager = GameObject.FindGameObjectWithTag("AudioTag").GetComponent<AudioManager>();
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        
        if (MultiPlayermi == 1) NetworkManagerUI.SetActive(false);
        if (!debugMod) DebugConsole.SetActive(false);
        if (MultiPlayermi > 1 && IsServer)
        {
            WaitingUI.SetActive(true);
        }
    }

    private void Update()
    {
        //Debug.Log("global: Update :" + global.playerName + " NetName :" + networkPlayerName.Value + "   PlayMod: " + global.PlayMod.Value.ToString() + "  isServer:" + IsServer + "  isOvner:" + IsOwner + "  isClient:" + IsClient);

        /*// Multiplayersa
        if (global.Multiplayer > 1)
        {
            if (!IsServer) return;      // Tutorialde server de�ilse ��k diyor.
        }*/

        
        if (MultiPlayermi == 1)    // SinglePlayer ise
        {
            if (global.PlayMod.Value == Mods.StartNewGame)
            {
                /*
                // Hostu kapat
                if (NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.Shutdown();
                    if (NetworkManager.Singleton != null)
                    {
                        //Destroy(NetworkManager.Singleton.gameObject);
                    }
                }
                */
                global.PlayMod.Value = global.Mods.Waiting;
                PrepareNewLevel();
                GenerateLevel();
                navMashBake();
                GetComponent<CountDownSystem>().CountDownStart();
            }
        }
        else   // Multiplayer ise
        {
            //Debug.Log("Multiplayer ---" + PlayMod + " " + MultiPlayermi.ToString());
            if ((PlayMod.Value == Mods.WaitingForPlayer && CurrentPlayerCount == 2))
            {
                Debug.Log("global 181: WaitForPlayer :" + global.playerName + " NetName :" + networkPlayerName.Value + "   PlayMod: " + global.PlayMod.Value.ToString() + "  isServer:" + IsServer + "  isOvner:" + IsOwner + "  isClient:" + IsClient);

                WaitingUI.SetActive(false);
                PlayMod.Value = Mods.Waiting;
                if (IsServer)  // isserver i de�i�tirdim.
                {
                    //PrepareNewLevel();
                    GenerateLevel();
                }
                navMashBake();
                GetComponent<CountDownSystem>().CountDownStart();

            }

            if (global.PlayMod.Value == Mods.StartNewGame)
            {
                Debug.Log("Multiplayer Oyun Ba�l�yor");
                // Hostu kapat
                if (NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.Shutdown();
                    if (NetworkManager.Singleton != null)
                    {
                        //Destroy(NetworkManager.Singleton.gameObject);
                    }
                }
                //if (CurrentPlayerCount == 2)
                {
           //if (!IsOwner) return;
                    PrepareNewLevel();
                    Debug.Log("Yeni Level haz�rland�....");
                    global.PlayMod.Value = global.Mods.WaitingForPlayer;
                }
            }

        }
        // Oyun bitmi�se EndGame yap.  PlayMod.Value: 
        if (global.PlayMod.Value == global.Mods.Ending ||
            global.PlayMod.Value == global.Mods.TimeOver)
        {
            Debug.Log("Oyun sonu ekran� ba�lat�l�yor 1");
            EndGame();
        }
        // Oynama modunda ise.  PlayMod.Value: Playing
        if (PlayMod.Value == Mods.Playing)
        {
            elapsedTime += Time.deltaTime;     // Ge�en s�reyi ekle
            minute = Mathf.FloorToInt(elapsedTime / 60);
            second = Mathf.FloorToInt(elapsedTime % 60);
            // Ge�en s�reyi Saat formatla yaz.
            txtTimer.text = string.Format("{0:00}:{1:00}", minute, second); //elapsedTime.ToString();
        }
        // Oynama s�resi Bitmi�se
        if (elapsedTime > MaxPlayTime)
        {
            PlayMod.Value = Mods.TimeOver;
            txtTimer.transform.LeanScale(new Vector3(1f, 1f, 1f), 0f).setLoopType(LeanTweenType.once);
        }
        // Oynama s�resine 10 saniye kalm��sa alarm durumuna ge�
        if (elapsedTime > MaxPlayTime - 10)
        {
            txtTimer.color = Color.red;
            txtTimer.transform.LeanScale(new Vector3(1.4f, 1.4f, 1.4f), 0.5f).setEaseInOutQuart().setLoopPingPong();
        }

        // Debug modu A� Kapat
        if (Input.GetKeyDown(KeyCode.G))
        {
            debugMod = !debugMod;
            if (debugMod) DebugConsole.SetActive(true);
            else DebugConsole.SetActive(false);
            
        }
    }

    // Oyun Ba�lat�l�yor.
    public static void BeginGame()
    {
        
        PlayMod.Value = Mods.Playing;
    }

    public static void CanavarDied()
    {
        global.CanavarSayisi--;
        if (CanavarSayisi <= 0) global.PlayMod.Value = global.Mods.Ending;

    }

    // Oyun bitti. GameOverScreen a�
    public void EndGame()
    {
        PlayMod.Value = Mods.Waiting;
        Debug.Log("globalScreen Ac�l�yor");
        gameOverScreen.gameObject.SetActive(true);
        //gameOverScreen.gameObject.LeanMoveLocalY(0, 1f).setEaseOutQuart();
        gameOverScreen.gameObject.LeanScale(new Vector3(1.4f, 1.4f, 1.4f),1).setEaseOutQuart();
    }

    // Tag le belirtilen t�m nesneleri yokeder
    public void DestroyGameObjects(string Tag)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(Tag);
        foreach (GameObject item in gameObjects)
            Destroy(item);

    }
    public void PrepareNewLevel()
    {
        Debug.Log("Prepare new level ba�lat�l�yor...");
        if (IlkAcilis != 1)  // �lk a��l�� de�ilse Mevcut nesneleri sil
        {
            DestroyGameObjects("Box");
            DestroyGameObjects("Bomb");
            //DestroyGameObjects("Player");
            DestroyGameObjects("Wall");
            DestroyGameObjects("Rock");
            DestroyGameObjects("Enemy");
            DestroyGameObjects("PowerUp");

        }
            elapsedTime = 0;
        minute = 0;
        second = 0;
        CountDownTime = 3;
        if (global.map != null)  // Haritay� bo�alt
        {
            for (int i = 0;i<en;i++)
                for (int j = 0;j<boy;j++)
                    map[i,j] = Nesne.Bos;
        }
        // Oyundaki karakter say�lar� belirleniyor
        KutuSayisi = UnityEngine.Random.Range(MinKutuSayisi, MaksKutuSayisi);
        CanavarSayisi = UnityEngine.Random.Range(MinCanavarSayisi, MaksCanavarSayisi);
 
        // Terrain B�y�kl��� ayarlan�yor geni�lik katsay�s�na g�re
        zemin.terrainData.size = new Vector3(global.en * global.genislikKatsayisi, 0, global.boy * global.genislikKatsayisi);
        //Debug.Log("Harita olu�turuldu :");

        // �st ve s�n�rdaki kayalar yerle�tiriliyor. Dizide yeri yoktur.
        for (int i = -1; i < global.en; i += 1)
        {
            Vector3 position = new Vector3((i + 0.5f) * global.genislikKatsayisi, 0.5f, (-1 + 0.5f) * global.genislikKatsayisi);
            Instantiate(kayaPrefab, position, Quaternion.identity);
            position = new Vector3((i + 0.5f) * global.genislikKatsayisi, 0.5f, (global.en - 1 + 0.5f) * global.genislikKatsayisi);
            Instantiate(kayaPrefab, position, Quaternion.identity);
        }
        // Sol ve sa� s�n�rdaki kayalar yerle�tiriliyor. Dizide yeri yoktur.
        for (int i = -1; i < global.boy; i += 1)
        {
            Vector3 position = new Vector3((-1 + 0.5f) * global.genislikKatsayisi, 0.5f, (i + 0.5f) * global.genislikKatsayisi);
            Instantiate(kayaPrefab, position, Quaternion.identity);
            position = new Vector3((global.boy + 0.5f) * global.genislikKatsayisi, 0.5f, (i + 0.5f) * global.genislikKatsayisi);
            Instantiate(kayaPrefab, position, Quaternion.identity);
        }

        // Oyun alan� i�indeki kayalar haritaya yerle�tiriliyor
        for (int i = 1; i < global.en - 1; i += 2)
            for (int j = 1; j < global.boy - 1; j += 2)
            {
                global.map[i, j] = global.Nesne.Kaya;
                PlaceObject(kayaPrefab, i, j);
            }
        if (MultiPlayermi == 2 && IlkAcilis == 1)
        {
            Debug.Log("Multiplayer Host ba�lat�l�yor...");
            NetworkManager.Singleton.StartHost();
            // global.Player1 = FindObjectOfType<NetworkManager>().client.connection.playerControllers[0].gameObject;
            global.Player1 = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

        }
        if (MultiPlayermi == 3 && IlkAcilis == 1)
        {
            Debug.Log("ClientBa�lat�ld� haz�rland�....");
            NetworkManager.Singleton.StartClient();
            global.Player1 = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

        }
        IlkAcilis = 2;  // �lk a��l�� ger�ekle�ti
    }
    public void GenerateLevel()
    {

        
        if (global.MultiPlayermi > 1) // Multiplayersa
        {
            for (int i = 0; i < KutuSayisi; i++)
            {
                boxCreateServerRpc();
            }
        }
        else // Single player ise
        {

            PlaceObjectsOnMap(kutuPrefab, KutuSayisi);          // Kutular olu�turuluyor
        }

        if (global.MultiPlayermi > 1) // Multiplayersa
        {
            for (int i = 0; i < CanavarSayisi; i++)
            {
                canavarCreateServerRpc();
            }
        }
        else
        {
            PlaceCanavarOnMap(CanavarSayisi);

        }


        if (MultiPlayermi == 1)   // Single Player Oyun A��lm��sa Oyuncuyu yerle�tir
        {
            // Oyuncu karakteri yerle�tiriliyor
            PlacePlayerOnMap(oyuncuPrefab);
        }
    }

    public void navMashBake()
    {
        navMeshSurface.BuildNavMesh();  // Haritaya g�re gidilebilecek yollar� belirle
    }
    void PlaceObjectsOnMap(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Debug.Log("Nesne Olusturuluyor :" + i.ToString());
            PlaceObject(prefab);

        }
    }

    private bool BosBirakilacakAlanmi(int x, int y)
    {
        bool sonuc = false;
        if (x == en - 1 && y == boy - 1) sonuc = true;
        if (x == 0 && y == boy - 1) sonuc = true;
        if (x == 0 && y == boy - 2) sonuc = true;
        if (x == 1 && y == boy - 1) sonuc = true;
        if (x == 0 && y == 0) sonuc = true;
        if (x == 0 && y == 1) sonuc = true;
        if (x == 1 && y == 0) sonuc = true;
        return sonuc;
    }
    void PlaceCanavarOnMap(int canavarSayisi)
    {
        for (int i = 0; i < canavarSayisi; i++)
        {
            switch ((int)UnityEngine.Random.Range(0, 3))
            {
                case 0:
                    PlaceObject(canavar1Prefab);       // Canavarlar olu�turuluyor
                    break;
                case 1:
                    PlaceObject(canavar2Prefab);       // Canavarlar olu�turuluyor
                    break;
                case 2:
                default:
                    PlaceObject(canavar3Prefab);       // Canavarlar olu�turuluyor
                    break;
            }
        }
    }

    // Oyuncu olu�turuluyor ve global.Player1 e referans� ba�lan�yor.
    void PlacePlayerOnMap(GameObject prefab)
    {
        // Bir nesne olu�turur  0,0 konumuna bunu oyuncu say�s�na g�re d�zenle
        global.map[0, 0] = global.Nesne.Player;
        GameObject gameobj;
        Vector3 position = new Vector3((0 + 0.5f) * global.genislikKatsayisi, 5.5f, (0 + 0.5f) * global.genislikKatsayisi);
        gameobj = Instantiate(prefab, position, Quaternion.identity);
        global.Player1 = gameobj;
    }

    // Olu�turulan nesnenin referans�n� d�ndererek bir nesne olu�turur
    GameObject PlaceObject(GameObject prefab)
    {
        // Rastgele bir konum belirle
        GameObject gameobj;
        int x, z;
        do    // Dizideki konum bo� olana kadar yeni say� �ret
        {
            x = UnityEngine.Random.Range(0, global.en - 1);
            z = UnityEngine.Random.Range(0, global.boy - 1);
        } while (global.map[x, z] != global.Nesne.Bos);

        // Prefab � yerle�tir
        Vector3 position = new Vector3((x + 0.5f) * global.genislikKatsayisi, 0.5f, (z + 0.5f) * global.genislikKatsayisi);
        //Debug.Log("Prefab Olusturuluyor :");
        gameobj = Instantiate(prefab, position, Quaternion.identity);
        //Debug.Log("Prefab Olusturuldu :");
        global.map[x, z] = global.Nesne.Kutu; // Mark the position as occupied

        return gameobj;
    }



    // Olu�turulan nesnenin referans�n� d�ndererek bir nesne olu�turur
    GameObject PlaceObject(global.Nesne nesne)
    {
        // Rastgele bir konum belirle
        GameObject gameobj;
        GameObject prefab = null;
        int x, z;
        do    // Dizideki konum bo� olana kadar yeni say� �ret
        {
            x = UnityEngine.Random.Range(0, global.en - 1);
            z = UnityEngine.Random.Range(0, global.boy - 1);
        } while (global.map[x, z] != global.Nesne.Bos || BosBirakilacakAlanmi(x, z));

        // Prefab � yerle�tir
        Vector3 position = new Vector3((x + 0.5f) * global.genislikKatsayisi, 0.5f, (z + 0.5f) * global.genislikKatsayisi);
        switch (nesne)
        {
            case global.Nesne.Kutu: prefab = kutuPrefab; break;
            case global.Nesne.Player: prefab = oyuncuPrefab; break;
            case global.Nesne.Canavar: prefab = canavar1Prefab; break;
            case global.Nesne.Kaya: prefab = kayaPrefab; break;
            case global.Nesne.Duvar: prefab = duvarPrefab; break;
            //case global.Nesne.Kutu: prefab = kutuPrefab; break;
            default: Debug.Log("Nesne Bulunamad�"); break;
        }
        gameobj = Instantiate(prefab, position, Quaternion.identity);
        //Debug.Log("Prefab Olusturuldu :");
        global.map[x, z] = nesne; // Mark the position as occupied

        return gameobj;
    }

    // x y konumuna Prefab olu�turur
    GameObject PlaceObject(GameObject prefab, int x, int y)
    {
        GameObject gameobj = null;
        // Check if the position is empty
        if (global.map[x, y] != global.Nesne.Bos)
        {
            Vector3 position = new Vector3((x + 0.5f) * global.genislikKatsayisi, 0.5f, (y + 0.5f) * global.genislikKatsayisi);
            gameobj = Instantiate(prefab, position, Quaternion.identity);
        }
        return gameobj;
    }

    // ------- OnNetWorkSpawn
    public override void OnNetworkSpawn()
    {
        Debug.Log(playerName + " kat�ld�.");
        PlayMod.OnValueChanged += PlayMod_OnValueChanged;
        networkPlayerName.Value = playerName; // + (OwnerClientId + 1);
        playerName = networkPlayerName.Value.ToString();
        
        meshRenderer.material.color = colors[(int)OwnerClientId];

    }

    private void PlayMod_OnValueChanged(Mods previousValue, Mods newValue)
    {
       // OnPlayModChanged?.Invoke(this, EventArgs.Empty);
    }

    // ----  Rpc  -----

    [ServerRpc]
    private void boxCreateServerRpc()
    {
        GameObject box;

            // Debug.Log("Nesne Olusturuluyor :" + i.ToString());
            box = PlaceObject(kutuPrefab);


            if (global.MultiPlayermi > 1)
            {
                spawnedBoxList.Add(box);
                // Parent e aktar.
                box.GetComponent<boxSystem>().parent = parent;
                box.GetComponent<NetworkObject>().Spawn();
            }
        
    }

    [ServerRpc]
    private void canavarCreateServerRpc()
    {
        GameObject canavar;

        // Debug.Log("Nesne Olusturuluyor :" + i.ToString());
        //canavar = PlaceObject(kutuPrefab);

        switch ((int)UnityEngine.Random.Range(0, 3))
        {
            case 0:
                canavar = PlaceObject(canavar1Prefab);       // Canavarlar olu�turuluyor
                break;
            case 1:
                canavar = PlaceObject(canavar2Prefab);       // Canavarlar olu�turuluyor
                break;
            case 2:
            default:
                canavar = PlaceObject(canavar3Prefab);       // Canavarlar olu�turuluyor
                break;
        }
        if (global.PlayerCount > 1)
        {
            spawnedEnemyList.Add(canavar);
            // Parent e aktar.
            canavar.GetComponent<EnemyAI>().parent = parent;
            canavar.GetComponent<NetworkObject>().Spawn();
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc()
    {
        GameObject toDestroy = spawnedBoxList[0];
        toDestroy.GetComponent<NetworkObject>().Despawn();
        spawnedBoxList.Remove(toDestroy);
        Destroy(toDestroy);
    }

    /// <summary>
    /// Server taraf�nda �al���yor
    /// Parametreyi fonksiyondan g�ndermek hackerlar�n i�ine yarar.
    /// Onlar� atlatmak i�in daha g�venli Serverparametreleri kullan�lacak
    /// 
    /// On interaction fonksiyonunda bu komut �a��r�l�r
    /// SetPlayerReadyServerRpc();
    /// </summary>
    [ServerRpc(RequireOwnership =false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default) 
    {
        Debug.Log("[global] recieved ClientId: " + serverRpcParams.Receive.SenderClientId);
        playerReadyDict[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) 
        {
            if (!playerReadyDict.ContainsKey(clientId) || !playerReadyDict[clientId])
            {
                // Bu oyuncu haz�r de�il
                allClientsReady = false;
                break;  // d�ng�den ��k sonraki d�ng�lere gerek yok
            }
        }
        Debug.Log("allClientsReady: " + allClientsReady);
        if (allClientsReady)
        {
            PlayMod.Value = Mods.Playing;  // Countdown start olmal�.
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("PauseGameServerRpc :" + serverRpcParams.Receive.SenderClientId.ToString());
        playerPauseDict[serverRpcParams.Receive.SenderClientId] |= true;    // direkt = kullan�labilir
        TestGamePausedState();
    }
    [ServerRpc(RequireOwnership = false)]
    private void UnPauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("UnPauseGameServerRpc :" + serverRpcParams.Receive.SenderClientId.ToString());
        playerPauseDict[serverRpcParams.Receive.SenderClientId] &= false;   // direkt e�it kullan�labilir.
        TestGamePausedState();
    }

    private void TestGamePausedState()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPauseDict.ContainsKey(clientId) && playerPauseDict[clientId])
            {
                // this player paused
                isLocalGamePaused.Value = true;
                return;
            }
        }
        // All player are unpaused
        isLocalGamePaused.Value = false;
        Debug.Log("TestGamePausedState :" + isLocalGamePaused.Value.ToString());

    }
}
