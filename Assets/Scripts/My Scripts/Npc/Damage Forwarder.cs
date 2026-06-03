using UnityEngine;

public class DamageForwarder : MonoBehaviour, IDamageable
{
    public NpcHealth healthSystem;

    public float DamIntensor = 1.0f;
    
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(amount * DamIntensor, hitPoint, hitNormal);
        }
    } 
}
