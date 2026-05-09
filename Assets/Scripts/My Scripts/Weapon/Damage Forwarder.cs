using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageForwarder : MonoBehaviour
{
    public NpcHealth healthSystem;

    public float DamIntensor = 1.0f;
    
    public void TakeDamage(float amount)
    {
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(amount * DamIntensor);
        }
    } 
}
