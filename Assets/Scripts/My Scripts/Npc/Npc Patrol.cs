using UnityEngine;
using UnityEngine.AI;

// 确保物体上一定有这些组件
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class OptimizedWanderNPC : MonoBehaviour
{
    [Header("游荡设置")]
    public float wanderRadius = 25f; // 游荡半径
    public float wanderTimer = 8f;   // 停顿时间（发呆多久再走）

    public float maxDis = 100f; // 最远可以走多远

    [Header("性能优化设置 (LOD)")]
    public Transform player;         // 玩家的 Transform
    public float wakeUpDistance = 130f; // 玩家靠近多远时唤醒 NPC

    private NavMeshAgent agent;
    private Animator animator;
    private float timer;
    private bool isSleeping = false;

    private Vector3 originPos;

    private Vector3 targetPos;

    void Start()
    {
        // 获取组件
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        originPos = transform.position;

        // 如果没有手动拖拽玩家，尝试自动寻找 (尽量在面板拖拽，FindTag 比较耗性能)
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("找不到 Player！请检查标签或手动分配。");
            }
        }

        // 初始化计时器
        timer = wanderTimer; 
    }

    void Update()
    {
        if (player == null) return;

        
        float sqrDistToPlayer = (transform.position - player.position).sqrMagnitude;
        float sqrWakeUpDist = wakeUpDistance * wakeUpDistance;

        if (sqrDistToPlayer > sqrWakeUpDist)
        {
            Sleep();
            return; 
        }
        else
        {
            WakeUp();
        }

        
        if (agent.pathPending || agent.remainingDistance > 0.5f)
        {
            UpdateAnimator();
            return;
        }

        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, NavMesh.AllAreas);
            agent.SetDestination(newPos);
            timer = 0f; // 重置计时器
        }

        UpdateAnimator();
    }

    private Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        targetPos = randDirection;

        float dis = Vector3.Distance(targetPos, originPos);

        NavMeshHit navHit;

        for (int i = 1; i <= 3; i++)
        {
            targetPos = transform.position + Random.insideUnitSphere * dist;

            if (Vector3.Distance(targetPos, originPos) <= maxDis)
            {
                if (NavMesh.SamplePosition(targetPos, out navHit, dist, layermask))
                {
                    return navHit.position;
                }
            }

            else
            {
                Vector3 returnPos = originPos + Random.insideUnitSphere * dist * 0.5f;

                if (NavMesh.SamplePosition(returnPos, out navHit, dist, layermask))
                {
                    return navHit.position;
                }
            }
        }
    
        return transform.position;
    }

   
    private void UpdateAnimator()
    {
        if (animator != null && agent.enabled)
        {
            
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    // 休眠机制：冻结寻路和动画计算
    private void Sleep()
    {
        if (isSleeping) return; // 避免重复调用

        isSleeping = true;

        // 停止寻路
        if (agent.enabled) 
        {
            agent.isStopped = true; // 先刹车
            agent.enabled = false;  // 再关组件
        }

        // 可选：你甚至可以关掉 Animator 彻底省性能 (如果你不介意他们突然定住的话)
        if (animator != null)
        {
            animator.enabled = false; 
        }
    }

    // 唤醒机制：恢复寻路和动画
    private void WakeUp()
    {
        if (!isSleeping) return; // 避免重复调用

        isSleeping = false;

        // 恢复寻路
        if (!agent.enabled)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        // 恢复 Animator
        if (animator != null)
        {
            animator.enabled = true;
        }
    }
}