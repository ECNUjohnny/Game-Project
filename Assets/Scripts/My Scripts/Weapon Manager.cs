using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class WeaponManager : MonoBehaviour
{
    [Header("配置数据")]
    public List<WeaponData> weaponDatas; // 在 Inspector 里拖入你的武器数据 (ScriptableObject)
    public Transform pistolSlot;       // 手部的挂载点

    public Transform rifleSlot;
    // 这个列表用来存储【已经生成出来】的实际 GameObject 模型
    private readonly List<GameObject> preloadedWeaponModels = new();
    
    private int currentWeaponIndex = 0;

    private GameObject weaponInstance;

    void Start()
    {
        // 游戏一开始，执行预加载
        PreloadAllWeapons();
    }

    void PreloadAllWeapons()
    {
        // 遍历所有你配置的武器数据
        foreach (WeaponData data in weaponDatas)
        {
            // 1. 生成模型

            if (data.ispistol) weaponInstance = Instantiate(data.weaponObj, pistolSlot);

            else weaponInstance = Instantiate(data.weaponObj, rifleSlot);

            // 2. 对齐坐标和旋转
            weaponInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            // 3. 关键一步：生成后立刻将其隐藏！（假装它不存在）
            weaponInstance.SetActive(false);

            // 4. 将这个造好的模型存入我们的内部列表中备用
            preloadedWeaponModels.Add(weaponInstance);
        }

        // 初始化完成后，默认掏出第一把武器
        if (preloadedWeaponModels.Count > 0)
        {
            EquipWeapon(0);
        }
    }

    void Update()
    {
        // 测试按键：按 Q 键切换下一把武器
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchToNextWeapon();
        }
    }

    void SwitchToNextWeapon()
    {
        // 1. 隐藏当前的武器
        preloadedWeaponModels[currentWeaponIndex].SetActive(false);

        // 2. 计算下一个武器的索引 (如果到底了就回到 0，实现循环)
        currentWeaponIndex++;
        if (currentWeaponIndex >= preloadedWeaponModels.Count)
        {
            currentWeaponIndex = 0;
        }

        // 3. 显示下一把武器
        preloadedWeaponModels[currentWeaponIndex].SetActive(true);

        // 4. 同步更新你的射击脚本中的数据！
        // 这样你的射击脚本就会使用新武器的射速和伤害了
        GetComponent<PlayerShooter>().currentWeapon = weaponDatas[currentWeaponIndex];
    }

    // 提供一个按指定索引装备武器的方法（比如用于按数字键 1,2,3 切枪）
    public void EquipWeapon(int index)
    {
        // 隐藏当前武器
        preloadedWeaponModels[currentWeaponIndex].SetActive(false);
        
        // 更新索引并显示新武器
        currentWeaponIndex = index;
        
        preloadedWeaponModels[currentWeaponIndex].SetActive(true);

        // 同步更新数据
        GetComponent<PlayerShooter>().currentWeapon = weaponDatas[currentWeaponIndex];

        //if (weaponDatas[currentWeaponIndex].weaponObj.GetComponent<weaponInstance>() == null) Debug.Log("Null!");

        GetComponent<PlayerShooter>().gunMuzzle = preloadedWeaponModels[currentWeaponIndex].GetComponent<weaponInstance>().muzzlePoint;
    }
}