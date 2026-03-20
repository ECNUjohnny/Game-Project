using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Start is called before the first frame update
    public bool bAiming;
    public bool bShooting;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bAiming = Input.GetMouseButton(1);
        bShooting = Input.GetMouseButton(0);
    }
}
