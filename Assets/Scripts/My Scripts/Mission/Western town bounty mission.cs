using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Westerntownbountymission : MonoBehaviour
{
    [Header("Map Setting")]

    public GameObject enemyBlip;
    
    [Header("Mission Settings")]
    public Transform[] spawnPoints; 

    public int epochs;     
    
    public static int savedEpochs; 
    
    public GameObject[] Enemies;

    private enum State
    {
        head,
        body,
        end,
    }
    
    public KeyCode quit = KeyCode.P;

    private State currentState;
    
    private int totalEnemiesThisEpoch;
    
    private bool isSpawning;

    private Coroutine spawnCoroutine;

    private Coroutine processCoroutine;

    public float time2wait = 3f;


    [Header("Spawn Settings")]
    public float spawnInterval = 1f;    
    public Dictionary<int, GameObject> spawnedEnemies = new Dictionary<int, GameObject>();
    
    public int enemiesCount; 

    
    void Start()
    {
        epochs = 1;
    }

    public void StartMission()
    {
        Debug.Log("Mission");

        processCoroutine = StartCoroutine(EpochFlow());    

    }

    /// <summary>
    /// 控制整个轮次状态机的核心协程
    /// </summary>
    IEnumerator EpochFlow()
    {
        while (true)
        {
            currentState = State.head;

            UIManager.Instance.ShowInteractionPrompt($"You have started the bounty hunter mission. Press '{quit}' to quit mission");
            yield return new WaitForSecondsRealtime(time2wait);

            UIManager.Instance.ShowInteractionPrompt($"Rounnd {epochs} will start");
            yield return new WaitForSecondsRealtime(time2wait); // 提示保留 3 秒

            currentState = State.body;
            
            totalEnemiesThisEpoch = 5 + (int)(epochs * 1.5); 
            enemiesCount = totalEnemiesThisEpoch;
            
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnEnemiesRoutine());

            while (isSpawning || enemiesCount > 0)
            {
                UIManager.Instance.ShowInteractionPrompt($"Enemies remain: {enemiesCount} / {totalEnemiesThisEpoch}");
                yield return null; // 等待下一帧
            }

            currentState = State.end;
            UIManager.Instance.ShowInteractionPrompt($"Round {epochs} is end");
            
            if (epochs % 5 == 0)
            {
                savedEpochs = epochs;
                Debug.Log($"Current saved epochs: round {savedEpochs}");
            }

            yield return new WaitForSecondsRealtime(time2wait * 2); // 提示保留 3 秒
            
            epochs++; 
        }
    }

    /// <summary>
    /// 处理敌人生成的具体逻辑
    /// </summary>
    IEnumerator SpawnEnemiesRoutine()
    {
        for (int i = 0; i < totalEnemiesThisEpoch; i++)
        {
            int maxEnemyIndex = Mathf.Min(Enemies.Length - 1, epochs / 2); 
            int minEnemyIndex = Mathf.Max(0, maxEnemyIndex - 2); 
            int selectedEnemyIndex = Random.Range(minEnemyIndex, maxEnemyIndex + 1);

            int spawnIndex = Random.Range(0, spawnPoints.Length);
            
            GameObject enemyObj = Instantiate(Enemies[selectedEnemyIndex], spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
            
            int instanceId = enemyObj.GetInstanceID();
            if (!spawnedEnemies.ContainsKey(instanceId))
            {
                spawnedEnemies.Add(instanceId, enemyObj);
            }

            GameObject blip = Instantiate(enemyBlip, MapController.Instance.mapBackground);

            blip.GetComponent<MapBlip>().target = enemyObj.transform;

            if (enemyObj.TryGetComponent(out NpcHealth healthSystem))
            {
                healthSystem.OnDeath += () =>
                {
                    Destroy(blip);
                    OnEnemyDeath(enemyObj);
                };
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    void Update()
    {
        // 核心流程已经交由 IEnumerator EpochFlow 接管
        // Update 留作处理诸如玩家输入、全局暂停等杂项逻辑
        if (Input.GetKeyDown(quit))
        {
            StopMission();            
        }

    }

    public void StopMission()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

        if (processCoroutine != null) StopCoroutine(processCoroutine);

        UIManager.Instance.ShowInteraction($"The mission is stoped. Recorded round: {epochs}", time2wait * 2);

        
    }

    /// <summary>
    /// 提供给外部（敌人自身的脚本）在死亡时调用的接口
    /// </summary>
    public void OnEnemyDeath(GameObject deceasedEnemy)
    {
        int id = deceasedEnemy.GetInstanceID();
        if (spawnedEnemies.ContainsKey(id))
        {
            spawnedEnemies.Remove(id);
            enemiesCount--; 
        }
    }
}
