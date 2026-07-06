using UnityEngine;

public class DamageForwarder : MonoBehaviour, IDamageable
{
    public NpcHealth healthSystem;

    public float DamIntensor = 1.0f;
    
    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        
        Debug.Log(name);
        
        if (healthSystem != null)
        {
            healthSystem.Damage(damage * DamIntensor, hitPoint, hitNormal);
        }
    } 
}
