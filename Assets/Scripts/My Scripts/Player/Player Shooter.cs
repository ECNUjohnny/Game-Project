using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System; // 必须引入这一行才能使用协程

public class PlayerShooter : MonoBehaviour
{
    [Header("Setting")]

    public WeaponData currentWeapon;

    public Camera playerCamera;

    public Transform gunMuzzle;       // 枪口位置

    public GameObject trace;  

    [Header("Visual Effect")]
    public float tracerDuration = 0.5f; // 线痕迹显示的时间（非常短）

    private float nextFireTime = 0f;

    public GameObject bloodEffect;


    void Start()
    {
        nextFireTime = Time.time + 0.2f;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();

            nextFireTime = Time.time + currentWeapon.fireRate;
        }
    }

    void Shoot()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit hitInfo;

        // 确定可视化的起点和终点
        Vector3 visualStartPoint = gunMuzzle.position;
        
        Vector3 visualEndPoint;

        if (Physics.Raycast(ray, out hitInfo, currentWeapon.range))
        {
            visualEndPoint = hitInfo.point;

            
            if (hitInfo.collider.TryGetComponent<NpcHealth>(out var npcHealth))
            {
                npcHealth.TakeDamage(currentWeapon.damage);

                if (bloodEffect != null)
                {
                    GameObject blood = Instantiate(bloodEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                
                    Destroy(blood, 2f);
                }
            }
            else
            {
                
                if (hitInfo.collider.TryGetComponent<DamageForwarder>(out var forwarder))
                {
                    forwarder.TakeDamage(currentWeapon.damage);

                    if (bloodEffect != null)
                    {
                        GameObject blood = Instantiate(bloodEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                
                        Destroy(blood, 2f);
                    }
                }
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