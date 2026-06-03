using UnityEngine;
[RequireComponent(typeof(WeaponManager))]



[RequireComponent(typeof(PlayerInventory))]
public class PlayerShooter : MonoBehaviour
{
    [Header("Setting")]

    public WeaponData currentWeapon;

    public Camera playerCamera;

    public Transform gunMuzzle;       // 枪口位置

    public GameObject trace;  

    public GameObject gun;

    [Header("Visual Effect")]
    
    public float tracerDuration = 0.5f; // 线痕迹显示的时间（非常短）

    public float NextFireTime {get; private set; } = 0f;

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
        if (currentWeaponController == null) return;
        
        if (Input.GetMouseButton(0) && NextFireTime <= Time.time)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            currentWeaponController.Shoot(ray.origin, ray.direction);
        
            NextFireTime += currentWeapon.fireRate;
        }

        if (currentWeaponController.CurrentAmmo == 0 && Input.GetMouseButton(0))
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
            Debug.Log("换弹失败：背包里没有这种备用子弹了！");
        }
    }
}   