using System;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(CharacterController))]
public class PlayerHealthSystem : MonoBehaviour
{

    [Header("Health Setting")]

    public float fullHealth = 1500f;

    private float Health;

    private Animator animator;

    private CharacterController controller;

    private Rigidbody[] ragdollRigBody;

    private Collider[] ragdollCollider;

    private PlayerCombat combat;

    private PlayerMovement movement; 

    [Header("External setting")]

    public Image HealthSystemMeter;

    public Camera playerCamera;

    public bool isDead = false; 

    public event Action Dead;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        ragdollRigBody = GetComponentsInChildren<Rigidbody>();
        ragdollCollider = GetComponentsInChildren<Collider>();
    
        combat = GetComponent<PlayerCombat>();
        movement = GetComponent<PlayerMovement>();

        SetRagdollState(false);

        Health = fullHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        
        Health -= amount;

        HealthSystemMeter.fillAmount = Health / fullHealth;

        if (Health <= 0)
        {
            isDead = true;

            Dead?.Invoke();

            EnableRagdoll();
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

        // Camera camera = Camera.main;

        // playerCamera.transform.SetParent(null);

        SetRagdollState(true);
    }

    void Update()
    {
        
    }

}
