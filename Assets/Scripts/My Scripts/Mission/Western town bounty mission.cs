using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Westerntownbountymission : MonoBehaviour
{
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
    
    // 当前游戏状态
    private State currentState;
    // 当前轮次需要生成的总敌人数
    private int totalEnemiesThisEpoch;
    // 标记是否正在生成敌人
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
        // 如果有存档逻辑，可以在这里从 savedEpochs 恢复 epochs 值
        
        // 使用协程来管理状态流转，比在 Update 里用 if 判断更清晰、更容易控制时间
        StartCoroutine(EpochFlow());
    }

    /// <summary>
    /// 控制整个轮次状态机的核心协程
    /// </summary>
    IEnumerator EpochFlow()
    {
        while (true)
        {
            // ================= State: head (准备阶段) =================
            currentState = State.head;
            ui.ShowInteractionPrompt($"第 {epochs} 轮即将开始！");
            yield return new WaitForSeconds(3f); // 提示保留 3 秒

            // ================= State: body (战斗阶段) =================
            currentState = State.body;
            
            // 计算当前轮的敌人总数：基础 5 个，每多一轮增加 2 个
            totalEnemiesThisEpoch = 5 + (epochs * 2); 
            enemiesCount = totalEnemiesThisEpoch;
            
            isSpawning = true;
            StartCoroutine(SpawnEnemiesRoutine());

            // 持续监控当前轮次进度，直到敌人全部生成完毕且全部被消灭
            while (isSpawning || enemiesCount > 0)
            {
                // 通过 API 实时显示剩余敌人与总敌人
                ui.ShowInteractionPrompt($"当前剩余敌人: {enemiesCount} / {totalEnemiesThisEpoch}");
                yield return null; // 等待下一帧
            }

            // ================= State: end (结束与结算阶段) =================
            currentState = State.end;
            ui.ShowInteractionPrompt($"第 {epochs} 轮结束！");
            
            // 每 5 轮记录一次游戏状态
            if (epochs % 5 == 0)
            {
                savedEpochs = epochs;
                Debug.Log($"[系统] 游戏状态已记录，当前安全存档点：第 {savedEpochs} 轮");
                // 在这里可以接入其他数据序列化/存档代码
            }

            yield return new WaitForSeconds(3f); // 提示保留 3 秒
            
            epochs++; // 轮数递增，自动进入下一层 while 循环
        }
    }

    /// <summary>
    /// 处理敌人生成的具体逻辑
    /// </summary>
    IEnumerator SpawnEnemiesRoutine()
    {
        for (int i = 0; i < totalEnemiesThisEpoch; i++)
        {
            // 核心逻辑：轮数越多，生成的敌人越强
            // 随着 epochs 增加，随机抽取的最大索引会逐渐向 Enemies 数组的末尾移动
            int maxEnemyIndex = Mathf.Min(Enemies.Length - 1, epochs / 2); 
            int minEnemyIndex = Mathf.Max(0, maxEnemyIndex - 2); 
            int selectedEnemyIndex = Random.Range(minEnemyIndex, maxEnemyIndex + 1);

            // 随机选择生成点
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            
            // 实例化敌人
            GameObject enemyObj = Instantiate(Enemies[selectedEnemyIndex], spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
            
            // 存入字典以供后续追踪
            int instanceId = enemyObj.GetInstanceID();
            if (!spawnedEnemies.ContainsKey(instanceId))
            {
                spawnedEnemies.Add(instanceId, enemyObj);
            }

            // 等待规定的间隔时间后再生成下一个[cite: 2]
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
            enemiesCount--; // 减少数量，触发 EpochFlow 中的退出循环条件
        }
    }
}
