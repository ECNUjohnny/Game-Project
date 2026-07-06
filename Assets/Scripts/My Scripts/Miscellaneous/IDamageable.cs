using UnityEngine;

// 任何想要能被子弹打中的东西，都必须有这个函数
public interface IDamageable
{
    void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal);
}