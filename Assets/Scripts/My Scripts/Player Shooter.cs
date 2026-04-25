using UnityEngine;
using System.Collections; // 必须引入这一行才能使用协程

public class PlayerShooter : MonoBehaviour
{
    [Header("引用")]
    public WeaponData currentWeapon;
    public Camera playerCamera;
    public Transform gunMuzzle;       // 枪口位置
    public LineRenderer tracerEffect; // 拖入刚才设置的 LineRenderer 物体

    [Header("设置")]
    public float tracerDuration = 0.05f; // 线痕迹显示的时间（非常短）

    private float nextFireTime = 0f;

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
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
            // 处理伤害逻辑...
        }
        else
        {
            // 如果未命中，线延伸到最大射程
            visualEndPoint = ray.origin + ray.direction * currentWeapon.range;
        }

        // 核心视觉逻辑：启动协程绘制线段
        StartCoroutine(RenderTracerEffect(visualStartPoint, visualEndPoint));
    }

    // 协程：控制线段的生成和快速消失
    IEnumerator RenderTracerEffect(Vector3 start, Vector3 end)
    {
        // 1. 启用线渲染器
        tracerEffect.enabled = true;

        // 2. 设置线的起点 (索引0) 和终点 (索引1)
        tracerEffect.SetPosition(0, start);
        
        tracerEffect.SetPosition(1, end);

        // 3. 等待极短的时间 (例如 0.05秒)
        yield return new WaitForSeconds(tracerDuration);

        // 4. 关闭线渲染器，使其消失
        tracerEffect.enabled = false;
    }
}