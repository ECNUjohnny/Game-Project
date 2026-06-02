using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NpcHealth))]
public class EnemyAI : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");


    [Header("Target Setting")]

    public Transform player;

    public float shootingRange = 10f;

    public float turnSpeed = 1.0f;

    [Header("Weapon Setting")]

    public Transform gunMuzzle;

    public float shootFreq = 2.0f;

    public WeaponData weapon;

    private NavMeshAgent agent;

    public GameObject trace;

    private NpcHealth healthSystem;

    private Animator animator;

    private NpcAnimator npcAnimator;

    private float nextFireTime;

    private GameObject gun;

    private WeaponController weaponController;

    [Header("Body Setting")]

    public Transform gunSlot;

    public Transform rifleSlot;

    public Transform rifleAimSlot;

    public Transform spine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        healthSystem = GetComponent<NpcHealth>();
        
        animator = GetComponent<Animator>();
        
        npcAnimator = GetComponent<NpcAnimator>();

        agent.stoppingDistance = shootingRange;

        if (player == null)
        {
            GameObject gameObject = GameObject.FindGameObjectWithTag("Player");

            if (gameObject != null)
            {
                player = gameObject.transform;
            }

            else
            {
                Debug.Log("Cannot find the player!");
            }

            // healthSystem.OnDeath += Stop;
        }

        if (weapon.type == 1)
        {
            gun = Instantiate(weapon.weaponObj, gunSlot);
        }

        else if (weapon.type == 2)
        {
            gun = Instantiate(weapon.weaponObj, rifleSlot);
        }
        // gunMuzzle = weapon.

        gun.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        gunMuzzle = gun.GetComponent<WeaponInstance>().muzzlePoint;

        npcAnimator.type = weapon.type;

        if (gun.TryGetComponent(out weaponController))
        {
            weaponController.Init(weapon);
        }

        healthSystem.OnDeath += Stop;
    }

    void FaceTarget()
    {
        Vector3 dir = (player.position - transform.position).normalized;

        dir.y = 0;

        Quaternion lookRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 4.5f);
    
        npcAnimator.bAiming = true;

        npcAnimator.bShooting = false;
    }
   
    void ShootPlayer()
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + weapon.fireRate * shootFreq;

        Vector3 target = player.position + Vector3.up * 1.45f;

        Vector3 shootDirection = target - gunMuzzle.position;

        if (weaponController != null)
        {
            weaponController.Shoot(gunMuzzle.position, shootDirection);
        }
    }

    void Update()
    {
        if (player != null)
        {
            if (healthSystem.isDead)
            {
                animator.SetFloat(SpeedHash, 0);
            
                animator.enabled = false;

                return;
            }
            
            // agent.isStopped = false;

            agent.SetDestination(player.position);
            /*
            if (Vector3.Distance(transform.position, player.position) >= shootingRange)
            {
                randDir = Random.insideUnitSphere * 2f;

                randPos = player.position + randDir;

                agent.SetDestination(randPos);
            }
            else
            {
                agent.isStopped = true;

                Vector3 dir = player.position - transform.position;

                dir.Normalize();

                Quaternion targetRot = Quaternion.LookRotation(dir);

                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }*/

            // UpdateAnimator();

            if (Vector3.Distance(player.position, transform.position) <= shootingRange)
            {
                FaceTarget();

                ShootPlayer();
            }

        }
    }

    
    void LateUpdate()
    {
        
        if (healthSystem.isDead || Vector3.Distance(player.position, transform.position) > shootingRange) return;
        
        Vector3 targetChest = player.position + Vector3.up * 1.45f;

        Vector3 dirToTarget = targetChest - spine.position;

        float pitchAngle = Vector3.SignedAngle(transform.forward, dirToTarget, transform.right);

        spine.Rotate(transform.right, pitchAngle, Space.World);
    }
    
    /*
    private void UpdateAnimator()
    {
        if (healthSystem.isDead) return;
        
        if (animator != null && agent.enabled)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }
    */

    private void Stop()
    {
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        Debug.Log("Dying");

        // StartCoroutine(CleanUpCorpse());
    }

}
