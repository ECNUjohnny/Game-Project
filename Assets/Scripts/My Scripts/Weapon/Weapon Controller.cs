using UnityEngine;


public class WeaponController : MonoBehaviour
{
    [Header("Weapon Setting")]

    public GameObject trace;
    
    public Transform gunMuzzle;

    private int currentAmmo;

    private float nextFireTime;

    private WeaponData weaponData; 

    private bool isReloading;

    private Vector3 visualEndPoint;

    public void Init(WeaponData data)
    {
        nextFireTime = 0;

        currentAmmo = data.ammo;

        weaponData = data;
    }

    public void Shoot(Vector3 aimOrigin, Vector3 aimDirection)
    {
        if (Time.time < nextFireTime || isReloading) return;

        nextFireTime = Time.time + weaponData.fireRate;

        GameObject fire = Instantiate(weaponData.muzzleFlash, gunMuzzle.position, Quaternion.LookRotation(gunMuzzle.forward)).gameObject;
    
        Destroy(fire, 0.25f);

        if (Physics.Raycast(aimOrigin, aimDirection, out RaycastHit hitInfo, weaponData.range))
        {
            visualEndPoint = hitInfo.point;

            
            if (hitInfo.collider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(weaponData.damage, hitInfo.point, hitInfo.normal);
            }
        }
        else
        {
            visualEndPoint = aimOrigin + aimDirection * weaponData.range;
        }
    
        GameObject newTrace = Instantiate(trace);
        newTrace.GetComponent<TracerBehavior>().Init(gunMuzzle.position, visualEndPoint);
    }

    public void Reload()
    {
        
    }
}
