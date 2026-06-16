using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NpcHealth))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class NpcAnimator : MonoBehaviour
{
    private static readonly int TReloadHash = Animator.StringToHash("tReload");

    private static readonly int TypeHash = Animator.StringToHash("type");
    
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private static readonly int BShootingHash = Animator.StringToHash("bShooting");

    private static readonly int BAimingHash = Animator.StringToHash("bAiming");

    private Animator animator;
    
    private NpcHealth healthSystem;
    
    private Rigidbody[] ragdollRigidbodies;
    
    private Collider[] ragdollColliders;

    private NavMeshAgent agent;

    // private EnemyAI enemyAI;

    public bool bAiming;

    public bool bShooting;

    public int type;

    void Start()
    {
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<NpcHealth>(); 
        agent = GetComponent<NavMeshAgent>();

        
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        // Debug.Log(ragdollColliders.Length);

        
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

    public void TriggerReloadAnimation()
    {
        animator.SetTrigger(TReloadHash);
    } 

    void Update()
    {
        animator.SetBool(BAimingHash, bAiming);

        animator.SetBool(BShootingHash, bShooting);

        animator.SetInteger(TypeHash, type);

        if (agent && agent.enabled) animator.SetFloat(SpeedHash, agent.velocity.magnitude);
    }
}