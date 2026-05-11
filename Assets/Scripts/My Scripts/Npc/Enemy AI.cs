using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public WeaponData weapon;

    private NavMeshAgent agent;

    public GameObject trace;

    private Vector3 randDir;

    private Vector3 randPos;

    private NpcHealth healthSystem;

    private Animator animator;

    private float nextFireTime;

    private GameObject gun;

    [Header("Body Setting")]

    public Transform gunSlot;

    public Transform rifleSlot;

    public Transform rifleAimSlot;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        healthSystem = GetComponent<NpcHealth>();
        animator = GetComponent<Animator>();

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

            healthSystem.OnDeath += Stop;
        }

        if (weapon.type == 1)
        {
            gun = Instantiate(weapon.weaponObj, gunSlot.position, Quaternion.identity);
        }

        else if (weapon.type == 2)
        {
            gun = Instantiate(weapon.weaponObj, rifleSlot.position, Quaternion.identity);
        }
        // gunMuzzle = weapon.

        gunMuzzle = gun.GetComponent<WeaponInstance>().muzzlePoint;
    }

    void FaceTarget()
    {
        Vector3 dir = (player.position - transform.position).normalized;

        dir.y = 0;

        Quaternion lookRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 1.5f);
    

    }
   
    void ShootPlayer()
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + weapon.fireRate;

        Vector3 target = player.position + Vector3.up * 1.45f;

        Vector3 shootDirection = target - gunMuzzle.position;

        Vector3 visualStartPoint = gunMuzzle.position;

        Vector3 visualEndPoint;

        if (Physics.Raycast(gunMuzzle.position, shootDirection, out RaycastHit hit, shootingRange))
        {

            if (hit.collider.TryGetComponent<PlayerHealthSystem>(out var playerHealth))
            {
                playerHealth.TakeDamage(weapon.damage);

                Debug.Log("You are hited!");

            }

            else
            {
                Debug.Log("Lucky!");
 
            }

            visualEndPoint = hit.point;
        }

        else
        {
            visualEndPoint = gunMuzzle.position + gunMuzzle.forward * weapon.range;
        }

        GameObject newTrace = Instantiate(trace);


        newTrace.GetComponent<TracerBehavior>().Init(visualStartPoint, visualEndPoint);
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

            /*if (Vector3.Distance(transform.position, player.position) >= shootingRange)
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

            UpdateAnimator();

            if (Vector3.Distance(player.position, transform.position) <= shootingRange)
            {
                FaceTarget();

                ShootPlayer();
            }

        }
    }

    private void UpdateAnimator()
    {
        if (healthSystem.isDead) return;
        
        if (animator != null && agent.enabled)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }


    private void Stop()
    {
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
    }
}
