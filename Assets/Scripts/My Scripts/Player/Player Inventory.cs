using System;
using TMPro;
using UnityEngine;


public enum WeaponType 
{
    Pistol = 1,
    Rifle = 2,
    Shotgun = 3,
}

[RequireComponent(typeof(PlayerShooter))]
public class PlayerInventory : MonoBehaviour
{

    [Header("Player Assets")]

    public int gold = 100;

    public int[] ammo = new int[10];

    public WeaponType ammoType;

    private PlayerShooter playerShooter;


    public event Action OnInventoryChanged;

    void Awake()
    {

        playerShooter = GetComponent<PlayerShooter>();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;

            OnInventoryChanged?.Invoke();
        
            return true;
        }

        return false;
    }
    
    public void AddAmmo(int amount, int type)
    {
        ammo[type] += amount;

        OnInventoryChanged?.Invoke();
    }
    
    public void ChangeWeapon(int type)
    {
        ammoType = (WeaponType)type;

        OnInventoryChanged?.Invoke();
    }

    public int ExtractAmmo(WeaponType type, int need)
    {
        int curretnAmmoInBag = ammo[(int)type];

        int ammoToGive = Math.Min(curretnAmmoInBag, need);

        ammo[(int)type] -= ammoToGive;

        OnInventoryChanged?.Invoke();

        return ammoToGive;
    }

}
