using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcPatrol : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Roaming Settings")]
    public NavMeshAgent agent;
    public float walkRadius = 15.0f;

    [Header("Optimization")]
    public Transform player;

    public float wakeUpDis = 50.0f;

    private bool isSleeping = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        float sqrtDis2Player = (transform.position - player.position).sqrMagnitude;

        float sqrtwakeUpDis = wakeUpDis * wakeUpDis;

        if (sqrtwakeUpDis <= sqrtDis2Player)
        {
            Sleep();

            return;
        }
        else
        {
            wakeUp();

        }
    
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GotoRandomPoint();
        }
    }

    void GotoRandomPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * walkRadius;

        randomDir += transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDir, out hit, walkRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    void Sleep()
    {
        if (isSleeping) return;

        isSleeping = true;

        agent.enabled = false;
    }

    void wakeUp()
    {
        if (!isSleeping) return;
        
        isSleeping = false;

        agent.enabled = true;
    }
}
