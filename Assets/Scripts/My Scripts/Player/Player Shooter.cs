using System;
using System.Collections;
using UnityEngine;
[RequireComponent(typeof(WeaponManager))]



[RequireComponent(typeof(PlayerInventory))]
public class PlayerShooter : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds2_5 = new(2.5f);
    
    [Header("Setting")]

    public WeaponData currentWeapon;

    public Camera playerCamera;

    public Transform gunMuzzle;       

    public GameObject trace;  

    public GameObject gun;

    public KeyCode Reload = KeyCode.R;

    [Header("Visual Effect")]
    
    public float tracerDuration = 0.5f; // 线痕迹显示的时间（非常短）

    public float NextFireTime { get; private set; } = 0f;

    [Header("Weapon State")]

    public bool isWeaponDrawn = false;

    public bool isDrawing = false;

    [HideInInspector]

    public WeaponController currentWeaponController; 

    private PlayerInventory playerInventory;

    private PlayerAnimator playerAnimator;

    void Start()
    {
        playerInventory = GetComponent<PlayerInventory>();

        playerAnimator = GetComponent<PlayerAnimator>();
    }

    void Update()
    {

        gunMuzzle = currentWeaponController.gunMuzzle;

        
        if (currentWeaponController == null) return;
        
        if (Input.GetMouseButton(0) && NextFireTime <= Time.time)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Input.GetMouseButton(1)) currentWeaponController.Shoot(ray.origin, ray.direction);

            else
            {
                StartDrawingWeapon();

                Debug.Log(isWeaponDrawn);

                if (isWeaponDrawn) currentWeaponController.Shoot(gunMuzzle.position, transform.forward);
            }

            NextFireTime += currentWeapon.fireRate;
        }

        if (Input.GetKey(Reload) || (currentWeaponController.CurrentAmmo == 0 && Input.GetMouseButton(0)))
        {
            HandleReload();
        }
    }

    private void HandleReload()
    {
        if (currentWeaponController.IsReloading) return;

        int ammoNeed = currentWeaponController.weaponData.clipSize - currentWeaponController.CurrentAmmo;

        if (ammoNeed <= 0) return;

        int ammoGotFromBag = playerInventory.ExtractAmmo(currentWeapon.ammoType, ammoNeed);
    
        if (ammoGotFromBag > 0)
        {
            currentWeaponController.Reload(ammoGotFromBag);

            playerAnimator.TriggerReloadAnimation();
        }
        else
        {
            Debug.Log("No more bullets");
        }
    }

    private void StartDrawingWeapon()
    {
        isDrawing = true;

        playerAnimator.TriggerShootAnimation();

        Debug.Log("Drawing");

        StartCoroutine(DrawWeaponRoutine());
    }

    IEnumerator DrawWeaponRoutine()
    {
        yield return _waitForSeconds2_5;

        isDrawing = false;

        isWeaponDrawn = true;

        
    }

}   