using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(CharacterController))]
public class PlayerHealthSystem : MonoBehaviour
{

    [Header("Health Setting")]

    public float Health = 1500f;

    private Animator animator;

    private CharacterController controller;

    private Rigidbody[] ragdollRigBody;

    private Collider[] ragdollCollider;

    private PlayerCombat combat;

    private PlayerMovement movement; 

    public bool isDead {get; private set; } = false; 

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        ragdollRigBody = GetComponentsInChildren<Rigidbody>();
        ragdollCollider = GetComponentsInChildren<Collider>();
    
        combat = GetComponent<PlayerCombat>();
        movement = GetComponent<PlayerMovement>();

        SetRagdollState(false);
    }

    void TakeDamage(float amount)
    {
        Health -= amount;

        if (Health <= 0)
        {
            isDead = true;
        }
    }

    private void SetRagdollState(bool isRagdollActive)
    {
        foreach (Rigidbody rb in ragdollRigBody)
        {
            rb.isKinematic = !isRagdollActive; 
        }

        foreach (Collider col in ragdollCollider)
        {
            if (col != controller) 
            {
                col.isTrigger = !isRagdollActive;
            }
        }
    }

    public void EnableRagdoll()
    {
        controller.enabled = false;

        movement.enabled = false;

        combat.enabled = false;

        animator.enabled = false;

        Camera camera = Camera.main;

        camera.transform.SetParent(null);

        SetRagdollState(true);
    }

    void Update()
    {
        
    }
}
