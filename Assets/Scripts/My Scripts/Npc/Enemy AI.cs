using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");


    [Header("Target Setting")]

    public Transform player;

    public float minDis = 2.3f;

    public float turnSpeed = 1.0f;

    private NavMeshAgent agent;

    private Vector3 randDir;

    private Vector3 randPos;

    private NpcHealth healthSystem;

    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        healthSystem = GetComponent<NpcHealth>();
        animator = GetComponent<Animator>();

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
    }

    // Update is called once per frame
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
            
            agent.isStopped = false;

            if (Vector3.Distance(transform.position, player.position) >= minDis)
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
            }

            UpdateAnimator();

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
