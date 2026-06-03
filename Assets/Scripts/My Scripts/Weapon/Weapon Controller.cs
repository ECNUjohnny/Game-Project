using System.Collections;
using UnityEngine;


public class WeaponController : MonoBehaviour
{
    [Header("Weapon Setting")]

    public GameObject trace;
    
    public Transform gunMuzzle;

    private float nextFireTime;

    public WeaponData weaponData; 

    public bool IsReloading { get; private set; }

    public int CurrentAmmo { get; private set; } 

    private Vector3 visualEndPoint;

    public void Init(WeaponData data)
    {
        nextFireTime = 0;

        CurrentAmmo = data.ammo;

        weaponData = data;
    }

    public void Shoot(Vector3 aimOrigin, Vector3 aimDirection)
    {
        if (Time.time < nextFireTime || IsReloading || CurrentAmmo <= 0) return;

        nextFireTime = Time.time + weaponData.fireRate;

        CurrentAmmo--;

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

    public void Reload(int bulletsReceived)
    {
        if (IsReloading || bulletsReceived <= 0) return;

        StartCoroutine(Reloading(bulletsReceived));
    }

    private IEnumerator Reloading(int bulletsReceived)
    {
        IsReloading = true;

        yield return new WaitForSeconds(weaponData.reloadRate);

        CurrentAmmo += bulletsReceived;

        IsReloading = false;
    }
}
