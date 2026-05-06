using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "FPS/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("基础信息")]
    
    public string weaponName;
    
    public GameObject weaponObj;    
    
    public bool ispistol;
    
    public int type;

    [Header("射击参数")]
    
    public float damage = 20.0f;    // 伤害
    
    public float fireRate = 0.1f;   // 射击间隔（秒）
    
    public float range = 100f;      // 射程
    
    public int magSize = 30;        // 弹匣容量

    [Header("反馈与效果")]
    
    public float recoilForce = 1.5f; // 后坐力强度
    
    public ParticleSystem muzzleFlash; // 枪口火焰特效
    
    public AudioClip fireSound;     // 开火音效
}
