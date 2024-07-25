using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class FlareSystem : NetworkBehaviour
{
    /// <summary>
    /// Alev sistemini kontrol eden fonksiyonlar
    /// Flare prefab i�lemlerini i�erir
    /// Flare belli bir y�nde ve oyuncunun bomba etki alan� uzunlu�una g�re alev s�re�lerini y�netir.
    /// 
    /// </summary>
    public playerMovement parent;

    public int flareLength;             // Alev uzunlu�u. Oyuncu �zelliklerine g�re
    public float speed;                 // H�z�
    public float timeToFlare;           // Alev olu�turma s�resi
    public float explodeTime;           // Patlama s�resi
    public Vector3 direction;           // Alevin y�n�
    public GameObject bombFlareprefab;  // Alev prefab�
    public Time startTime;              // ba�lama zaman�

    [SerializeField] private List<GameObject> spawnedFlareList = new List<GameObject>();


    // Patlama Coroutine �al��t�r�l�r
    void Start()
    {
        StartCoroutine(startExplode());
    }


    IEnumerator startExplode()
    {
        Debug.Log("FlareSystem 1 - " + global.Player1.gameObject.name + " - " + global.MultiPlayermi.ToString() + " - " + IsOwner);

        // Multiplayersa
        if (global.MultiPlayermi < 3)
        {
            if (IsOwner || global.MultiPlayermi==1) // A� da ise
            {
                // patlama rutini
                {
                    // Alev olu�turma s�resine kadar bekle
                    yield return new WaitForSeconds(timeToFlare);

                    if (flareLength > 0)        // Bomban�n Alev� sonuna gelmediyse
                    {
                        if (global.MultiPlayermi > 1)
                        {
                            flareCreateServerRpc();
                        }
                        else
                        {
                            // Vector3 position = new Vector3((transform.localPosition.x) * global.genislikKatsayisi, 0.5f, (transform.localPosition.z+1f) * global.genislikKatsayisi);
                            Vector3 position = transform.position + direction;
                            // Debug.Log("Bir sonraki Flare Olusturuluyor :");
                            GameObject flare = Instantiate(bombFlareprefab, position, Quaternion.identity);
                            flare.transform.Rotate(direction, Space.World);
                            flare.GetComponent<FlareSystem>().direction = direction;
                            flare.GetComponent<FlareSystem>().timeToFlare = timeToFlare;
                            flare.GetComponent<FlareSystem>().flareLength = flareLength - 1;  // Alev uzunluk mant���
                            flare.GetComponent<FlareSystem>().explodeTime = explodeTime;
                        }
                    }
                    Destroy(this.gameObject, explodeTime);  // Mevcut Alev S�resi bitince yok edilsin
                }
            }
            StopCoroutine(startExplode());          // Patlama Coroutini silinsin.
        }
    }

    // Alev nesnesi bir nesneye �arpt�ysa
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Flare �arpt�");
        StopCoroutine(startExplode());                  // Patlama Coroutini durdurulur
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Alev Duvara �arpm��sa 
            global.audioManager.PlaySFX(global.audioManager.auWallRockHit);
            flareLength = 0;                // Daha fazla alev olu�turma
            Destroy(this.gameObject);       // Bu alevi yoket
        }
        if (collision.gameObject.CompareTag("Rock"))
        {
            // Alev Kayaya �arpm��sa 
            global.audioManager.PlaySFX(global.audioManager.auWallRockHit);
            flareLength = 0;                // Daha fazla alev olu�turma
            Destroy(this.gameObject);       // Bu alevi yoket
        }
        if (collision.gameObject.CompareTag("Player"))
        {
            // Alev Oyuncuya �arpm��sa 
            flareLength = 0;                // Daha fazla alev olu�turma
            Destroy(this.gameObject);       // Bu alevi yoket
            // �arpt���n oyuncuyu �ld�r
            collision.gameObject.GetComponent<playerMovement>().die();
            // �arpt��� oyuncu rakip ise puan ver 
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Alev D��mana �arpm��sa 
            global.audioManager.PlaySFX(global.audioManager.auEnemyExplode);
            flareLength = 0;                // Daha fazla alev olu�turma
            Destroy(collision.gameObject);  // �arpt���n nesneyi yoket
            Destroy(this.gameObject);       // Bu alevi yoket
            parent.score += 3;
            global.CanavarDied();
        }
        if (collision.gameObject.CompareTag("Box"))
        {
            // Alev Kutuya �arpm��sa 
            global.audioManager.PlaySFX(global.audioManager.auBoxExplode);
            flareLength = 0;                // Daha fazla alev olu�turma
            Destroy(this.gameObject);       // Bu alevi yoket
                                            // �arpt���n bombay� patlat
            parent.score += 1;
            collision.gameObject.GetComponent<boxSystem>().Explode();
        }
        if (collision.gameObject.CompareTag("PowerUp"))
        {
            // Alev PowerUpa �arpm��sa 
            global.audioManager.PlaySFX(global.audioManager.auPlayerJump);
            flareLength = 0;                // Daha fazla alev olu�turma
            Destroy(this.gameObject);       // Bu alevi yoket
        }
        if (collision.gameObject.CompareTag("Bomb"))
        {
            // Alev Bombaya �arpm��sa 
            flareLength = 0;                // Daha fazla alev olu�turma
            Destroy(this.gameObject);       // Bu alevi yoket
            collision.gameObject.GetComponent<BombSystem>().BombExplode();
            parent.score += 1;
        }
        if (collision.gameObject.CompareTag("BombFlare"))
        {
            // Alev Aleve �arpm��sa 
            //Destroy(this.gameObject);       // Bu alevi yoket
        }


    }

    [ServerRpc]
    private void flareCreateServerRpc()
    {
        // Vector3 position = new Vector3((transform.localPosition.x) * global.genislikKatsayisi, 0.5f, (transform.localPosition.z+1f) * global.genislikKatsayisi);
        Vector3 position = transform.position + direction;
        // Debug.Log("Bir sonraki Flare Olusturuluyor :");
        GameObject flare = Instantiate(bombFlareprefab, position, Quaternion.identity);
        flare.transform.Rotate(direction, Space.World);
        flare.GetComponent<FlareSystem>().direction = direction;
        flare.GetComponent<FlareSystem>().timeToFlare = timeToFlare;
        flare.GetComponent<FlareSystem>().flareLength = flareLength - 1;  // Alev uzunluk mant���
        flare.GetComponent<FlareSystem>().explodeTime = explodeTime;

        if (global.MultiPlayermi > 1)
        {
            spawnedFlareList.Add(flare);
            // Parent e aktar.
            flare.GetComponent<FlareSystem>().parent = global.parent;
            flare.GetComponent<NetworkObject>().Spawn();
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
