using UnityEngine;
using System;
using System.Collections.Generic;


[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerShooter))]

[RequireComponent(typeof(PlayerInventory))]
public class WeaponManager : MonoBehaviour
{
    [Header("配置数据")]
    
    public List<WeaponData> weaponDatas; 
    
    public Transform pistolSlot;       
    
    public Transform rifleSlot;
    
    public int weaponType;
    
    public KeyCode Change = KeyCode.Q;
    
    public Transform rifleAimSlot;

    public event Action<WeaponController> OnWeaponChanged;
    
    // 核心修改 1：这里改为存 WeaponController 的列表
    private readonly List<WeaponController> preloadedWeapons = new();
    
    private int currentWeaponIndex = 0;
    
    private PlayerCombat playerCombat;
    
    private PlayerShooter playerShooter;

    private PlayerInventory playerInventory;

    void Start()
    {
        playerCombat = GetComponent<PlayerCombat>();
        playerShooter = GetComponent<PlayerShooter>();
        playerInventory = GetComponent<PlayerInventory>();
        
        PreloadAllWeapons();
    }

    void PreloadAllWeapons()
    {
        foreach (WeaponData data in weaponDatas)
        {
            GameObject weaponInstance;
            if (data.ispistol) weaponInstance = Instantiate(data.weaponObj, pistolSlot);
            else weaponInstance = Instantiate(data.weaponObj, rifleSlot);

            weaponInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            // 核心修改 2：在生成时，只执行这一次 GetComponent
            WeaponController controller = weaponInstance.GetComponent<WeaponController>();
            if (controller != null)
            {
                // 把图纸数据注入给这把枪
                controller.Init(data);
                // 把这个控制器加入备用列表
                preloadedWeapons.Add(controller);
            }
            else
            {
                Debug.LogError($"预制体 {data.weaponObj.name} 上没有挂载 WeaponController");
            }

            weaponInstance.SetActive(false);
        }

        if (preloadedWeapons.Count > 0)
        {
            EquipWeapon(0);
        }
    }

    void Update()
    {
        if (!Input.GetMouseButton(1) && Input.GetKeyDown(Change))
        {
            SwitchToNextWeapon();
        }

        // 瞄准时的坐标移动逻辑 (注意这里要加 .gameObject)
        if (weaponType == 2 && Input.GetMouseButton(1))
        {
            preloadedWeapons[currentWeaponIndex].gameObject.transform.SetParent(rifleAimSlot);
            preloadedWeapons[currentWeaponIndex].gameObject.transform.localPosition = Vector3.zero;
            preloadedWeapons[currentWeaponIndex].gameObject.transform.localRotation = Quaternion.identity;
        }
        else if (weaponType == 2)
        {
            preloadedWeapons[currentWeaponIndex].gameObject.transform.SetParent(rifleSlot);
            preloadedWeapons[currentWeaponIndex].gameObject.transform.localPosition = Vector3.zero;
            preloadedWeapons[currentWeaponIndex].gameObject.transform.localRotation = Quaternion.identity;
        }
    }

    void SwitchToNextWeapon()
    {
        int nextIndex = currentWeaponIndex + 1;
        if (nextIndex >= preloadedWeapons.Count) nextIndex = 0;
        EquipWeapon(nextIndex);
    }

    public void EquipWeapon(int index)
    {
        if (preloadedWeapons.Count == 0) return;

        

        // 1. 隐藏当前武器
        preloadedWeapons[currentWeaponIndex].gameObject.SetActive(false);
        
        // 2. 更新索引与基础状态
        currentWeaponIndex = index;
        weaponType = weaponDatas[currentWeaponIndex].type;
        playerCombat.weaponType = weaponType;
        
        // 3. 显示新武器
        preloadedWeapons[currentWeaponIndex].gameObject.SetActive(true);

        // 核心修改 3：直接把已经缓存好的“真枪”递给射击脚本！
        playerShooter.currentWeaponController = preloadedWeapons[currentWeaponIndex];

        playerShooter.currentWeapon = weaponDatas[currentWeaponIndex];
        
        playerInventory.ChangeWeapon(weaponType);

        OnWeaponChanged?.Invoke(playerShooter.currentWeaponController);
    }
}