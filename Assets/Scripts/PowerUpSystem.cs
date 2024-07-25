using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PowerUpSystem : NetworkBehaviour
{
    public playerMovement parent;
    [SerializeField] private List<GameObject> spawnedPowerUpList = new List<GameObject>();

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.LeanRotate(new Vector3(0f, 180f, 0f), 1f).setEaseInOutQuart().setLoopPingPong();  // setEaseInOutQuart().setLoopPingPong();
        //LeanTween.rotateY(gameObject, 360.0f, 1.0f).setRepeat(999);
        //transform.LeanScale(new Vector3(1f, 1f, 1f), 0.6f).setEaseInOutQuart().setLoopPingPong();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
