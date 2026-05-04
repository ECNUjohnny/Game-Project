using UnityEngine;
[RequireComponent(typeof(NpcHealth))]
[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(Collider))]
public class NpcRagdoll : MonoBehaviour
{
    
    private Animator animator;
    
    private NpcHealth healthSystem;
    
    private Collider mainCollider; 
    
    private Rigidbody[] ragdollRigidbodies;
    
    private Collider[] ragdollColliders;

    void Start()
    {
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<NpcHealth>();
        mainCollider = GetComponent<Collider>(); 

        
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        
        SetRagdollState(false);

       
        healthSystem.OnDeath += EnableRagdoll;
    }

    
    private void SetRagdollState(bool isRagdollActive)
    {
        
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            
            rb.isKinematic = !isRagdollActive; 
        }

        
        foreach (Collider col in ragdollColliders)
        {
            
            if (col != mainCollider)
            {
                col.isTrigger = !isRagdollActive;
            }
        }
    }

    private void EnableRagdoll()
    {
        
        if (animator != null)
        {
            animator.enabled = false;
        }

        
        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }

        
        SetRagdollState(true);
    }

    
    void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= EnableRagdoll;
        }
    }
}