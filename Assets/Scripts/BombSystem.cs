using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class BombSystem : NetworkBehaviour
{
    /// <summary>
    /// Bomba prefab� fonksiyonlar�
    /// Her oyuncu i�in bomba de�erleri farkl� olabilir. 
    /// Bomba olu�turuldu�unda Oyuncunun bilgilerine g�re bomba de�erleri y�klenir.
    /// </summary>
    public playerMovement parent;
    public int bombLength;                  // Bomba uzunlu�u 
    public float speed;                     // Bomba h�z�
    public float timeToExplode;             // Patlamadan �nceki s�re
    public float explodeTime;               // Patlama s�resi
    public GameObject bombFlareprefab;      // Bomban�n yakt��� Alev prefab�
    public Time startTime;                  // Ba�lama zaman�

    [SerializeField] private List<GameObject> spawnedFlareList = new List<GameObject>();


    // Bomba olu�turuldu�unda 
    void Start()
    {
        // Bomba olu�turuldu�unda b�y�y�p k���ltme animasyonu ba�lat
        //transform.LeanScale(new Vector3(1.4f, 1.4f, 1.4f), 0.4f).setEaseInOutQuart().setLoopPingPong();
        StartCoroutine(startExplode());         // Patlama rutinini �al��t�r
    }

    void Update()
    {
        //Debug.Log("BombSystem - " + global.MultiPlayermi.ToString() + " - "+ IsOwner);
        // Multiplayersa
        if (global.MultiPlayermi > 1)
        {
            //if (!IsOwner) return;
        }

    }

    IEnumerator startExplode()
    {
        // Patlama s�resine kadar bekle
        yield return new WaitForSeconds(1.2f);   // Bomba oluşturulduktan sonra bir müddet şeffaf olsun
        GetComponent<SphereCollider>().isTrigger = false;
        yield return new WaitForSeconds(timeToExplode - 1.2f);
        // S�re dolunca Patlama ger�ekle�sin
        if (IsOwner || global.MultiPlayermi == 1) BombExplode();
        StopCoroutine(startExplode());   // CoRoutine i haf�zadan siliyoruz
    }

    public void BombExplode()
    {
        // Patlama sesi �al��t�r
        global.audioManager.PlaySFX(global.audioManager.auBombExplode);

        //Debug.Log("BombSystem 2 - " + global.MultiPlayermi.ToString() + " - " + IsOwner);

        // �leri y�nl� Alev olu�tur
        if (global.MultiPlayermi == 2)    // Host ise
        {
            flareCreateForwardServerRpc();
        }
        else
        {
            Vector3 position = new Vector3((transform.localPosition.x) * global.genislikKatsayisi, 0.5f, (transform.localPosition.z + 1) * global.genislikKatsayisi);
            GameObject flare = Instantiate(bombFlareprefab, position, Quaternion.Euler(new Vector3(0, 0, 0)));
            flare.GetComponent<FlareSystem>().direction = Vector3.forward;
            flare.GetComponent<FlareSystem>().timeToFlare = 0.2f;
            flare.GetComponent<FlareSystem>().flareLength = bombLength;
            flare.GetComponent<FlareSystem>().explodeTime = 0.35f;
            flare.GetComponent<FlareSystem>().parent = parent;
        }
        // Geri y�nl� Alev olu�tur
        if (global.MultiPlayermi == 2)    // Host ise
        {
            flareCreateBackwardServerRpc();
        }
        else
        {
            Vector3 position2 = new Vector3((transform.localPosition.x) * global.genislikKatsayisi, 0.5f, (transform.localPosition.z - 1) * global.genislikKatsayisi);
            GameObject flare2 = Instantiate(bombFlareprefab, position2, Quaternion.Euler(new Vector3(0, 0, 0)));
            //flare2.transform.localRotation = Quaternion.Euler(0f, 180, 0f);
            flare2.transform.Rotate(Vector3.back, Space.World);
            flare2.GetComponent<FlareSystem>().direction = Vector3.back;
            flare2.GetComponent<FlareSystem>().timeToFlare = 0.2f;
            flare2.GetComponent<FlareSystem>().flareLength = bombLength;
            flare2.GetComponent<FlareSystem>().explodeTime = 0.35f;
            flare2.GetComponent<FlareSystem>().parent = parent;
        }
        // Sa� y�nl� Alev olu�tur

        if (global.MultiPlayermi == 2)    // Host ise
        {
            flareCreateRightServerRpc();
        }
        else
        {
            Vector3 position3 = new Vector3((transform.localPosition.x + 1) * global.genislikKatsayisi, 0.5f, (transform.localPosition.z) * global.genislikKatsayisi);
            //Debug.Log("Flare Olusturuluyor :");
            GameObject flare3 = Instantiate(bombFlareprefab, position3, Quaternion.Euler(new Vector3(0, 0, 0)));
            //flare3.transform.localRotation = Quaternion.Euler(0f, 90, 0f);
            flare3.transform.Rotate(Vector3.right, Space.World);
            flare3.GetComponent<FlareSystem>().direction = Vector3.right;
            flare3.transform.Rotate(0.0f, 0.0f, 0.0f, Space.World);
            flare3.GetComponent<FlareSystem>().timeToFlare = 0.2f;
            flare3.GetComponent<FlareSystem>().flareLength = bombLength;
            flare3.GetComponent<FlareSystem>().explodeTime = 0.35f;
            flare3.GetComponent<FlareSystem>().parent = parent;
        }

        // Sol y�nl� Alev olu�tur
        if (global.MultiPlayermi == 2)    // Host ise
        {
            flareCreateLeftServerRpc();
        }
        else
        {
            Vector3 position4 = new Vector3((transform.localPosition.x - 1) * global.genislikKatsayisi, 0.5f, (transform.localPosition.z) * global.genislikKatsayisi);
            //Debug.Log("Flare Olusturuluyor :");
            GameObject flare4 = Instantiate(bombFlareprefab, position4, Quaternion.Euler(new Vector3(0, 0, 0)));
            //flare4.transform.localRotation = Quaternion.Euler(0f, 270, 0f);
            //flare2.transform.Rotate(0.0f, 0.0f, 0.0f, Space.World);
            flare4.transform.Rotate(Vector3.left, Space.World);
            flare4.GetComponent<FlareSystem>().direction = Vector3.left;
            flare4.GetComponent<FlareSystem>().timeToFlare = 0.2f;
            flare4.GetComponent<FlareSystem>().flareLength = bombLength;
            flare4.GetComponent<FlareSystem>().explodeTime = 0.35f;
            flare4.GetComponent<FlareSystem>().parent = parent;
        }
        Destroy(this.gameObject, 0.4f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (global.MultiPlayermi > 1) if (!IsOwner) return;
        // Bomba bir yere de�di�inde ate� topunu yokeder
        /*
        GameObject hitImpact = Instantiate(bombFlareprefab, transform.position, Quaternion.identity);
        hitImpact.transform.localEulerAngles = new Vector3(0f,0f,-90f);
        Destroy(gameObject);*/
        parent.DestroyServerRpc();
    }

    [ServerRpc]
    private void flareCreateForwardServerRpc()
    {
        Vector3 position = new Vector3((transform.localPosition.x) * global.genislikKatsayisi, 0.5f, (transform.localPosition.z + 1) * global.genislikKatsayisi);
        GameObject flare = Instantiate(bombFlareprefab, position, Quaternion.Euler(new Vector3(0, 0, 0)));
        flare.GetComponent<FlareSystem>().direction = Vector3.forward;
        flare.GetComponent<FlareSystem>().timeToFlare = 0.2f;
        flare.GetComponent<FlareSystem>().flareLength = bombLength;
        flare.GetComponent<FlareSystem>().explodeTime = 0.35f;
        flare.GetComponent<FlareSystem>().parent = parent;

        if (global.MultiPlayermi > 1)    // Multiplayer ise
        {
            spawnedFlareList.Add(flare);
            // Parent e aktar.
            flare.GetComponent<FlareSystem>().parent = global.parent;
            flare.GetComponent<NetworkObject>().Spawn();
        }
    }

    [ServerRpc]
    private void flareCreateBackwardServerRpc()
    {
        Vector3 position2 = new Vector3((transform.localPosition.x) * global.genislikKatsayisi, 0.5f, (transform.localPosition.z - 1) * global.genislikKatsayisi);
        GameObject flare2 = Instantiate(bombFlareprefab, position2, Quaternion.Euler(new Vector3(0, 0, 0)));
        //flare2.transform.localRotation = Quaternion.Euler(0f, 180, 0f);
        flare2.transform.Rotate(Vector3.back, Space.World);
        flare2.GetComponent<FlareSystem>().direction = Vector3.back;
        flare2.GetComponent<FlareSystem>().timeToFlare = 0.2f;
        flare2.GetComponent<FlareSystem>().flareLength = bombLength;
        flare2.GetComponent<FlareSystem>().explodeTime = 0.35f;
        flare2.GetComponent<FlareSystem>().parent = parent;

        if (global.MultiPlayermi > 1)    // Multiplayer ise
        {
            spawnedFlareList.Add(flare2);
            // Parent e aktar.
            flare2.GetComponent<FlareSystem>().parent = global.parent;
            flare2.GetComponent<NetworkObject>().Spawn();
        }
    }

    [ServerRpc]
    private void flareCreateLeftServerRpc()
    {
        Vector3 position4 = new Vector3((transform.localPosition.x - 1) * global.genislikKatsayisi, 0.5f, (transform.localPosition.z) * global.genislikKatsayisi);
        //Debug.Log("Flare Olusturuluyor :");
        GameObject flare4 = Instantiate(bombFlareprefab, position4, Quaternion.Euler(new Vector3(0, 0, 0)));
        //flare4.transform.localRotation = Quaternion.Euler(0f, 270, 0f);
        //flare2.transform.Rotate(0.0f, 0.0f, 0.0f, Space.World);
        flare4.transform.Rotate(Vector3.left, Space.World);
        flare4.GetComponent<FlareSystem>().direction = Vector3.left;
        flare4.GetComponent<FlareSystem>().timeToFlare = 0.2f;
        flare4.GetComponent<FlareSystem>().flareLength = bombLength;
        flare4.GetComponent<FlareSystem>().explodeTime = 0.35f;
        flare4.GetComponent<FlareSystem>().parent = parent;

        if (global.MultiPlayermi > 1)    // Multiplayer ise
        {
            spawnedFlareList.Add(flare4);
            // Parent e aktar.
            flare4.GetComponent<FlareSystem>().parent = global.parent;
            flare4.GetComponent<NetworkObject>().Spawn();
        }
    }

    [ServerRpc]
    private void flareCreateRightServerRpc()
    {
        Vector3 position3 = new Vector3((transform.localPosition.x + 1) * global.genislikKatsayisi, 0.5f, (transform.localPosition.z) * global.genislikKatsayisi);
        //Debug.Log("Flare Olusturuluyor :");
        GameObject flare3 = Instantiate(bombFlareprefab, position3, Quaternion.Euler(new Vector3(0, 0, 0)));
        //flare3.transform.localRotation = Quaternion.Euler(0f, 90, 0f);
        flare3.transform.Rotate(Vector3.right, Space.World);
        flare3.GetComponent<FlareSystem>().direction = Vector3.right;
        flare3.transform.Rotate(0.0f, 0.0f, 0.0f, Space.World);
        flare3.GetComponent<FlareSystem>().timeToFlare = 0.2f;
        flare3.GetComponent<FlareSystem>().flareLength = bombLength;
        flare3.GetComponent<FlareSystem>().explodeTime = 0.35f;
        flare3.GetComponent<FlareSystem>().parent = parent;

        if (global.MultiPlayermi > 1)    // Multiplayer ise
        {
            spawnedFlareList.Add(flare3);
            // Parent e aktar.
            flare3.GetComponent<FlareSystem>().parent = global.parent;
            flare3.GetComponent<NetworkObject>().Spawn();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc()
    {
        GameObject toDestroy = spawnedFlareList[0];
        toDestroy.GetComponent<NetworkObject>().Despawn();
        spawnedFlareList.Remove(toDestroy);
        Destroy(toDestroy);
    }
}
