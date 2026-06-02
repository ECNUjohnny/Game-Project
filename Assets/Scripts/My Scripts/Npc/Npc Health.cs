using System;
using System.Collections;
using UnityEngine;

public class NpcHealth : MonoBehaviour, IDamageable
{
    // Start is called before the first frame update
    [Header("Health Parameter")]
    
    public float health = 100f;

    public float clearDelay = 20f;

    private float currentHealth = 100f;

    public bool isDead {get; private set; } = false;

    public event Action OnTakeDamage; // 控制伤害的广播

    public event Action OnDeath; // 控制死亡的广播

    public GameObject bloodEffect;


    void Start()
    {
        currentHealth = health;
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isDead) return;

        currentHealth -= damage;

        OnTakeDamage?.Invoke();

        if (bloodEffect != null)
        {
            GameObject blood = Instantiate(bloodEffect, hitPoint, Quaternion.LookRotation(hitNormal));
        
            Destroy(blood, 1.5f);
        }

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

        StartCoroutine(CleanUpCorpse());
    }

    IEnumerator CleanUpCorpse()
    {
        float waitTime = clearDelay;

        yield return new WaitForSeconds(waitTime);

        float sinkRate = 0.5f;
        float sinkDuration = 3f;
        float time = 0;

        while (time < sinkDuration)
        {
            time += Time.deltaTime;

            transform.Translate(sinkRate * Time.deltaTime * Vector3.down, Space.World);

            yield return null;
        }

        Destroy(gameObject);
    }

}
