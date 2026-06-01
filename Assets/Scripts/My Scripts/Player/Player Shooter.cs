using UnityEngine;


public class PlayerShooter : MonoBehaviour
{
    [Header("Setting")]

    public WeaponData currentWeapon;

    public Camera playerCamera;

    public Transform gunMuzzle;       // 枪口位置

    public GameObject trace;  

    [Header("Visual Effect")]
    public float tracerDuration = 0.5f; // 线痕迹显示的时间（非常短）

    public float NextFireTime {get; private set; } = 0f;

    // public GameObject bloodEffect;

    // public GameObject GunShootFire;


    void Start()
    {
        NextFireTime = Time.time + 0.2f;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= NextFireTime)
        {
            Shoot();

            NextFireTime = Time.time + currentWeapon.fireRate;
        }
    }

    void Shoot()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));


        // 确定可视化的起点和终点
        Vector3 visualStartPoint = gunMuzzle.position;

        Vector3 visualEndPoint;

        GameObject fire = Instantiate(currentWeapon.muzzleFlash, gunMuzzle.position, Quaternion.LookRotation(gunMuzzle.forward)).gameObject;

        Destroy(fire, 0.25f);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, currentWeapon.range))
        {
            visualEndPoint = hitInfo.point;

            
            if (hitInfo.collider.TryGetComponent<DamageForwarder>(out var forwarder))
            {
                forwarder.TakeDamage(currentWeapon.damage);

                GameObject blood = Instantiate(currentWeapon.blood, hitInfo.point, Quaternion.identity).gameObject;
            
                Destroy(blood, 2f);
            }

        }
        else
        {
            // 如果未命中，线延伸到最大射程
            visualEndPoint = ray.origin + ray.direction * currentWeapon.range;
        }

        // 核心视觉逻辑：启动协程绘制线段
        GameObject newTrace = Instantiate(trace);

        newTrace.GetComponent<TracerBehavior>().Init(visualStartPoint, visualEndPoint);
    }

}