using UnityEngine;



[RequireComponent(typeof(WeaponManager))]
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


    void Update()
    {
        if (Input.GetMouseButton(0) && NextFireTime <= Time.time)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            
        
            NextFireTime += currentWeapon.fireRate;
        }
    }

}