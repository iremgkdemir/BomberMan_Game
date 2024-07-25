using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouse : MonoBehaviour
{
    private float x;
    private float y;
    public float sensitivity = -1f;
    private Vector3 rotate;

    // Start is called before the first frame update
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Bekleme modundaysa i�lem yapma
        if (global.PlayMod.Value == global.Mods.Waiting) return;
        if (global.PlayMod.Value == global.Mods.Ending ||
            global.PlayMod.Value == global.Mods.GameOver ||
            global.PlayMod.Value == global.Mods.GameOverOpenned)  // niye de�ildi
        {
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        
        y += Input.GetAxis("Mouse X") * sensitivity;
        x += Input.GetAxis("Mouse Y") * sensitivity;
        x= Mathf.Clamp(x, -90f, 90f);
        rotate = new Vector3(x, y , 0);
        //transform.eulerAngles = transform.eulerAngles - rotate;
        transform.localRotation = Quaternion.Euler(0, y, 0f);

    }

}


