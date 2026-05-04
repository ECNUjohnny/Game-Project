using System;
using Unity.VisualScripting;
using UnityEngine;

public class NpcHealth : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Health Parameter")]
    
    public float health = 100f;

    private float currentHealth = 100f;

    public bool isDead {get; private set; } = false;

    public event Action OnTakeDamage; // 控制伤害的广播

    public event Action OnDeath; // 控制死亡的广播


    void Start()
    {
        currentHealth = health;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        OnTakeDamage?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        } 
    } 

    private void Die()
    {
        isDead = true;
    
        OnDeath?.Invoke();

        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
        }
    }

}
