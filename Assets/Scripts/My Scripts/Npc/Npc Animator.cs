using UnityEngine;
[RequireComponent(typeof(NpcHealth))]
[RequireComponent(typeof(Animator))]
public class NpcAnimator : MonoBehaviour
{
    
    private Animator animator;
    
    private NpcHealth healthSystem;
    
    private Rigidbody[] ragdollRigidbodies;
    
    private Collider[] ragdollColliders;

    void Start()
    {
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<NpcHealth>(); 

        
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
            
            col.isTrigger = !isRagdollActive;
        
        }
    }

    private void EnableRagdoll()
    {
        
        if (animator != null)
        {
            animator.enabled = false;
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

    void Update()
    {
        
    }
}