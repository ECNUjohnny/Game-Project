using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Start is called before the first frame update
    public bool bAiming;
    public bool bShooting;
    public bool bDeadEye;
    private float TimeScale = 0.5f;
    private float defaultFixedDeltaTime;
    private float maxDeadEyeTime;
    private float currentTime;
    void Start()
    {
        bDeadEye = false;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
        maxDeadEyeTime = 10f;
        currentTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        bAiming = Input.GetMouseButton(1);
        bShooting = Input.GetMouseButton(0);
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            DeadEye();
        }
        
        if (bDeadEye)
        {
            currentTime += Time.unscaledDeltaTime;

            if (currentTime > maxDeadEyeTime)
            {
                DeadEye();
            }    
        }
    }

    void DeadEye()
    {
        bDeadEye = !bDeadEye;
        if (bDeadEye)
        {
            Time.timeScale = TimeScale;

            Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
            
            currentTime = 0;
        }
        else
        {
            Time.timeScale = 1.0f;

            Time.fixedDeltaTime = defaultFixedDeltaTime;
        }
    }
}
