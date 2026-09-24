using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Westerntownbountymission : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds6 = new(6f);
    private static WaitForSeconds _waitForSeconds3 = new(3f);
    [Header("Settings")]
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
    
    private State currentState;
    
    private int totalEnemiesThisEpoch;
    
    private bool isSpawning;


    [Header("Spawn Settings")]
    public float spawnInterval = 1.5f;
    
    public Dictionary<int, GameObject> spawnedEnemies = new Dictionary<int, GameObject>();
    
    public int enemiesCount; 

    [Header("UI Settings")]
    public GameObject dialoguePanel;
    
    public UIManager ui;
    
    void Start()
    {
        epochs = 1;

        StartCoroutine(EpochFlow());
    }

    /// <summary>
    /// 控制整个轮次状态机的核心协程
    /// </summary>
    IEnumerator EpochFlow()
    {
        while (true)
        {
            currentState = State.head;
            ui.ShowInteractionPrompt($"Rounnd {epochs} will start");
            yield return _waitForSeconds3; // 提示保留 3 秒

            currentState = State.body;
            
            totalEnemiesThisEpoch = 5 + (int)(epochs * 1.5); 
            enemiesCount = totalEnemiesThisEpoch;
            
            isSpawning = true;
            StartCoroutine(SpawnEnemiesRoutine());

            while (isSpawning || enemiesCount > 0)
            {
                ui.ShowInteractionPrompt($"Enemies remain: {enemiesCount} / {totalEnemiesThisEpoch}");
                yield return null; // 等待下一帧
            }

            currentState = State.end;
            ui.ShowInteractionPrompt($"Round {epochs} is end");
            
            if (epochs % 5 == 0)
            {
                savedEpochs = epochs;
                Debug.Log($"Current saved epochs: round {savedEpochs}");
            }

            yield return _waitForSeconds6; // 提示保留 3 秒
            
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

            yield return new WaitForSeconds(spawnInterval);
        }
        
        isSpawning = false;
    }

    void Update()
    {
        // 核心流程已经交由 IEnumerator EpochFlow 接管
        // Update 留作处理诸如玩家输入、全局暂停等杂项逻辑
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
