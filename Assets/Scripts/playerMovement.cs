using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class playerMovement : NetworkBehaviour // MonoBehaviour
{
    /// <summary>
    /// Trinity 1.0
    /// Oyuncu Hareket fonsiyonlar�n� i�erir
    /// </summary>
    //public playerMovement parent;
    public GameObject prefab;           // Bomba prefab�
    public GameObject PlayerCamera;     // Oyuncu kameras�
    //public CharacterController characterController;
    public float speed = 5.0f;          // Oyuncunun h�z�
    public float jumpForce = 5.0f;      // Oyuncunun z�plama g�c�
    private float horizontalInput;      // yatay girdi
    private float forwardInput;         // Dikey girdi
    private Rigidbody playerRb;         // Oyuncu fizik nesnesi
    private int health=5;               // Oyuncunun Sa�l�k durumu
    public int score = 0;              // Oyuncunun Skoru
    public int bombCount=1;             // Oyuncunun Bomba b�rakma say�s�
    public int bombLength = 4;          // Oyuncunun Bomba etki uzunlu�u
    private int bombTimeToExplode = 3;  // Bomba patlama s�resi
    private bool CameraMod = true;     // 1 Tepeden 2 Oyuncu g�z�nden
    public Animator playerAnim;
    public Rigidbody playerRigid;
    public float w_speed, wb_speed, olw_speed, rn_speed, ro_speed;
    public bool walking;
    public Transform playerTrans;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;


    //Vector3 velocity;

    bool isGrounded;

    private float mouseX;
    private float mouseY;
    public float sensitivity = -1f;
    private Vector3 rotate;
    [SerializeField] private List<GameObject> spawnedBombList = new List<GameObject>();

    // Rigidbody nesnesi bulunur
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        if (global.MultiPlayermi > 1)
        {
            if (!IsOwner) return;  // Multiplayerda kendi karakteri de�ilse ��k 
                                   // Clienttaki host karakterine kar��ma
                                   // Hosttaki client karakterine kar��ma
        }
        global.parent = this;   /// Burada Multiplayer i�in clientda host da bu de�eri de�i�tirir
                                // Get set fonksiyonu yap    �NEML�
        KameraGuncelle();
    }

    public override void OnNetworkSpawn()
    {
        KameraGuncelle();
        Debug.Log("playerMovement Spawn 67: WaitForPlayer :" + global.playerName + "   PlayMod: " + global.PlayMod.Value.ToString() + "  isServer:" + IsServer + "  isOvner:" + IsOwner + "  isClient:" + IsClient);
        //base.OnNetworkSpawn();
        if (IsOwner)
        {
            //LocalInstance = this;
            global.Player1 = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

            Vector3 position;   // = new Vector3((0 + 0.5f) * global.genislikKatsayisi, 0.5f, (0 + 0.5f) * global.genislikKatsayisi);
            if (!IsClient)   // Client değil ise
            {
                position = new Vector3((0 + 0.5f) * global.genislikKatsayisi, 1.5f, (0 + 0.5f) * global.genislikKatsayisi);
                transform.position = position;
            }
            else
            {
                position = new Vector3((global.en - 1.5f) * global.genislikKatsayisi, 1.5f, (global.boy - 1.5f) * global.genislikKatsayisi);
                transform.position = position;
                GetComponent<CountDownSystem>().CountDownStart();
            }
            
        }
        Debug.Log("Spawned Player: " + this.OwnerClientId.ToString());

        //KameraGuncelle();
    }

    // Oyuncu modunda hareketleri kontrol eder.
    void FixedUpdate()
    {
        if (CameraMod == false)    // Oyuncu Modundaysa fist persona göre hareket et.
        {
            if (Input.GetKey(KeyCode.W))
            {
                //Debug.Log("W");
                playerRigid.velocity = transform.forward * w_speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                //Debug.Log("S");
                playerRigid.velocity = -transform.forward * wb_speed * Time.deltaTime;
            }
        }
    }

    // Her frame de bu i�lemleri yap
    void Update()
    {
        if (global.isLocalGamePaused.Value) return;
        // Multiplayersa
        if (global.MultiPlayermi > 1)
        {
            //if (!IsServer) return;
            if (!IsOwner) return;  // Multiplayerda kendi karakteri de�ilse ��k 
                                   // Clienttaki host karakterine kar��ma
                                   // Hosttaki client karakterine kar��ma
        }

        // Bekleme modundaysa i�lem yapma
        if (global.PlayMod.Value == global.Mods.Waiting ||
            global.PlayMod.Value == global.Mods.TimeOver ||
            global.PlayMod.Value == global.Mods.GameOver ||
            global.PlayMod.Value == global.Mods.Ending ||
            global.PlayMod.Value == global.Mods.StartNewGame ||
            global.PlayMod.Value == global.Mods.WaitingForPlayer) return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        //--------------
        // Klavye fare girdilerine göre hesaplamaları yap animasyonları oluştur
        // Oyuncu girdilerini oku
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        mouseY = Input.GetAxis("Mouse X") * sensitivity;
        playerTrans.Rotate(0, mouseY * Time.deltaTime, 0);

        //mouseX += Input.GetAxis("Mouse Y") * sensitivity;
        //mouseX = Mathf.Clamp(mouseX, -90f, 90f);
        //rotate = new Vector3(mouseX, mouseY, 0);
        //transform.eulerAngles = transform.eulerAngles - rotate;
        //transform.localRotation = Quaternion.Euler(0, mouseY, 0f);



        if (CameraMod == true)  // Top kamera aktifse
        {
            transform.position += Vector3.right * Time.deltaTime * speed * horizontalInput;
            //playerTrans.Rotate(0, -ro_speed * Time.deltaTime, 0);
            transform.position += Vector3.forward * Time.deltaTime * speed * forwardInput;

            // transform.localPosition += Vector3.right * Time.deltaTime * speed * horizontalInput;
            // transform.localPosition += Vector3.forward * Time.deltaTime * speed * forwardInput;

            if (Input.GetKey(KeyCode.W))
            {
                if (transform.rotation.y > -0.2f) playerTrans.Rotate(0, -ro_speed * 5 * Time.deltaTime, 0);
                if (transform.rotation.y < -0.19f) playerTrans.Rotate(0, ro_speed * 5 * Time.deltaTime, 0);
                playerAnim.SetTrigger("walk");
                playerAnim.ResetTrigger("idle");
                walking = true;
            }
            if (Input.GetKey(KeyCode.S))
            {
                if (transform.rotation.y < 0.98f) playerTrans.Rotate(0, ro_speed * 5 * Time.deltaTime, 0);
                if (transform.rotation.y > 0.99f) playerTrans.Rotate(0, -ro_speed * 5 * Time.deltaTime, 0);
                playerAnim.SetTrigger("walk");
                playerAnim.ResetTrigger("idle");
                walking = true;
            }
            if (Input.GetKey(KeyCode.A))
            {
                //Debug.Log("Mouse RoSpeed : " + transform.rotation.y.ToString() + "   MouseY :" + mouseY.ToString());
                if (CameraMod == true && transform.rotation.y > -0.9) playerTrans.Rotate(0, -ro_speed * 5 * Time.deltaTime, 0);
                playerAnim.SetTrigger("walk");
                playerAnim.ResetTrigger("idle");
                walking = true;
            }
            if (Input.GetKey(KeyCode.D))
            {
                if (CameraMod == true && transform.rotation.y < 0.55) playerTrans.Rotate(0, ro_speed * 5 * Time.deltaTime, 0);
                playerAnim.SetTrigger("walk");
                playerAnim.ResetTrigger("idle");
                walking = true;
            }
        }
        else  // CameraMod == false  Oyuncu Modundaysa fist persona g�re hareket et.
        {
            /*
            if (Input.GetKey(KeyCode.W))
            {
                //Debug.Log("W");
                playerRigid.velocity = transform.forward * w_speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                //Debug.Log("S");
                playerRigid.velocity = -transform.forward * wb_speed * Time.deltaTime;
            }
            */
            if (Input.GetKey(KeyCode.A))
            {
                //Debug.Log("Mouse RoSpeed : " + transform.rotation.y.ToString() + "   MouseY :" + mouseY.ToString());
                playerTrans.Rotate(0, -ro_speed * Time.deltaTime, 0);
                playerAnim.SetTrigger("walk");
                playerAnim.ResetTrigger("idle");
                walking = true;
            }
            if (Input.GetKey(KeyCode.D))
            {
                playerTrans.Rotate(0, ro_speed * Time.deltaTime, 0);
                playerAnim.SetTrigger("walk");
                playerAnim.ResetTrigger("idle");
                walking = true;
            }
        }


        /* Characktercontroller i�in
        //check if the player is on the ground so he can jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            //the equation for jumping
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);
        */

        ////----- Animasyon koddlar�

        // Animasyon koddlar�
        if (forwardInput > 0)
        {
            if (!walking)
            {
                playerAnim.SetTrigger("walk");
                playerAnim.ResetTrigger("idle");
                walking = true;
            }
        }
        else if (forwardInput < 0)  // Sadece oyuncu modunda çalışır
        {
            if (!walking)
            {
                if (CameraMod == true)  // Top kamera modu
                {
                    playerAnim.SetTrigger("walk");
                }
                else   // Oyuncu modu
                {
                    playerAnim.SetTrigger("walkback");
                }
                playerAnim.ResetTrigger("idle");
                walking = true;
            }
        }
        else   // 0 ise duruyordur
        {
            if (walking)
            {
                playerAnim.ResetTrigger("walk");
                playerAnim.ResetTrigger("walkback");
                playerAnim.SetTrigger("idle");
                walking = false;
            }
        }

        if (walking)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                w_speed = rn_speed;
                playerAnim.SetTrigger("run");
                playerAnim.ResetTrigger("walk");
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                w_speed = olw_speed;
                playerAnim.ResetTrigger("run");
                playerAnim.SetTrigger("walk");
            }
        }

        ////-----
        if (Input.GetKeyDown(KeyCode.V) && IsServer)
        {
            transform.position = new Vector3(transform.position.x, 3, transform.position.z);
        }

        // Oyuncuyu z�plat 
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            global.audioManager.PlaySFX(global.audioManager.auPlayerJump);
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            playerAnim.SetTrigger("jump");


        }
        // Oyuncunun konumuna Bomba b�rak
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            playerAnim.SetTrigger("attack");


            // map[transform.localPosition.x, 0, transform.localPosition.z] = global.Nesne.Bomba;

            // Birakabilecek bomba varsa Bomba b�rak
            if (bombCount > 0)
            {
                // Multiplayersa
                if (global.PlayerCount > 1)
                {
                    bombServerRpc();
                }
                else
                {
                    Vector3 position = new Vector3((((int)transform.localPosition.x) + 0.5f) * global.genislikKatsayisi, 0f, (((int)transform.localPosition.z) + 0.5f) * global.genislikKatsayisi);
                    // Debug.Log("Bomba Olusturuluyor :");
                    GameObject bomba = Instantiate(prefab, position, Quaternion.identity);
                    bomba.GetComponent<BombSystem>().timeToExplode = bombTimeToExplode;
                    bomba.GetComponent<BombSystem>().bombLength = bombLength;
                    bomba.GetComponent<BombSystem>().explodeTime = 3;
                    bomba.GetComponent<BombSystem>().parent = this;   // Bombanın sahibi kim

                    //parent = this;
                    // Bombay� g�nder Network Bile�eni ile

                    bombCount--;            // Bomba sayac�n� azalt
                }
                //Destroy(bomba);
                StartCoroutine(startBombCountRestore());    // Bomba patlay�nca sayac� art�r
            }

        }

        // Kamera de�i�tirme i�lemi
        if (Input.GetKeyDown(KeyCode.C))
        {
            CameraMod = !CameraMod;
            KameraGuncelle();
        }

    }

    public void KameraGuncelle()
    {
        if (CameraMod == true)  // Top kamera aktif ise
        {
            PlayerCamera.SetActive(false);
            global.MainCamera.SetActive(true);
        }
        if (CameraMod == false)  // Oyuncu kameras� aktif ise
        {
            global.MainCamera.SetActive(false);
            PlayerCamera.SetActive(true);
        }
    }

    [ServerRpc]
    private void bombServerRpc()
    {
        Vector3 position = new Vector3((((int)transform.localPosition.x) + 0.5f) * global.genislikKatsayisi, 0f, (((int)transform.localPosition.z) + 0.5f) * global.genislikKatsayisi);
        Debug.Log("player: Bomba Olusturuluyor :" + global.playerName + "   PlayMod: " + global.PlayMod + "  isServer:" + IsServer + "  isOvner:" + IsOwner + "  isClient:" + IsClient);
        GameObject bomba = Instantiate(prefab, position, Quaternion.identity);
        bomba.GetComponent<BombSystem>().timeToExplode = bombTimeToExplode;
        bomba.GetComponent<BombSystem>().bombLength = bombLength;
        bomba.GetComponent<BombSystem>().explodeTime = 3;
        bomba.GetComponent<BombSystem>().parent = this;    // Bombanın sahibi belli olsun.

        //parent = this;   // Kodu ba�ka bir tarafa al. Parent ayr� yerde olsun.
        // Bombay� g�nder Network Bile�eni ile
        if (global.PlayerCount > 1)
        {
            spawnedBombList.Add(bomba);
            // Parent e aktar.
            bomba.GetComponent<BombSystem>().parent = this;
            bomba.GetComponent<NetworkObject>().Spawn();
        }
        bombCount--;            // Bomba sayac�n� azalt
    }

    [ServerRpc(RequireOwnership =false)]
    public void DestroyServerRpc()
    {
        GameObject toDestroy = spawnedBombList[0];
        toDestroy.GetComponent<NetworkObject>().Despawn();
        spawnedBombList.Remove(toDestroy);
        Destroy(toDestroy);
    }



    // Bomba patlad���nda bomba sayac�n� art�r�r.
    IEnumerator startBombCountRestore()
    {
        {
            yield return new WaitForSeconds(bombTimeToExplode);
            bombCount++;
            StopCoroutine(startBombCountRestore());     // Coroutine i durdur
        }


    }

    // Oyuncu �ld���nde ger�ekle�tirilecek i�lemler
    public void die()
    {
        CameraMod = true;
        KameraGuncelle();
        global.audioManager.PlaySFX(global.audioManager.auPlayerExplode);
        //Debug.Log("Oyuncu �l�yor");
        global.PlayMod.Value = global.Mods.Ending;            // Oyun bitiyor durumuna ge�
        gameObject.SetActive(false);                // Oyuncuyu kaybet
        playerAnim.SetTrigger("die");

        StartCoroutine(DeactivateAfterAnimation());
    }

    IEnumerator DeactivateAfterAnimation()
{
    yield return new WaitForSeconds(3); // Adjust time according to the length of the die animation
    gameObject.SetActive(false);
}

    // Oyuncu bir nesneye temas ettiyse
    private void OnCollisionEnter(Collision collision)
    {
        // Bomba Say�s�n� art�ran PowerUp a �arpm��sa: Tag
        if (collision.gameObject.CompareTag("puBombCount"))
        {
            global.audioManager.PlaySFX(global.audioManager.auPowerUpEat);
            bombCount++;
            // bomba artt���nda bomba icon u b�y�s�n k���ls�n
            //global.bombIcon.transform.LeanScale(new Vector3(1.2f, 1.2f, 1.2f), 1).setEaseInOutQuart();
            Destroy(collision.gameObject);
        }

        // Bomba etki alan�n� art�ran Bomba alan art�r�c�ya �arpm��sa: Tag
        if (collision.gameObject.CompareTag("puBombLength"))
        {
            global.audioManager.PlaySFX(global.audioManager.auPowerUpEat);
            bombLength++;
            Destroy(collision.gameObject);
        }

        // Bomba H�z�n� art�ran Speed a �arpm��sa: Tag
        if (collision.gameObject.CompareTag("puSpeed"))
        {
            global.audioManager.PlaySFX(global.audioManager.auPowerUpEat);
            speed +=2;
            Destroy(collision.gameObject);
        }

        // Bomba Hayalet fonksiyonu Powerupa �arpm��sa: Tag
        if (collision.gameObject.CompareTag("puGhost"))
        {
            global.audioManager.PlaySFX(global.audioManager.auPowerUpEat);
            Destroy(collision.gameObject);
        }

        // Ba�ka bir Player a �arpm��sa: Tag
        if (collision.gameObject.CompareTag("Player"))
        {
            global.audioManager.PlaySFX(global.audioManager.auPlayerHi);
            Debug.Log("Kimsin karde�...");
        }
        
    }


    ///------- Animasyon fonksiyonlar�
 



    /// 
    /// ------------

}
