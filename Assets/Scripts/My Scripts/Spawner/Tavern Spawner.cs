using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TavernSpawner : MonoBehaviour
{

    [Tooltip("代表自由站立区域的 Collider")]
    public BoxCollider freeStandArea;

    private static readonly WaitForSeconds _waitForSeconds0_1 = new(0.1f);
    [Header("System Ref")]
    public TimeManager timeManager; 
    
    [Header("Spawn Setting")]
    
    public List<GameObject> npcPrefabs; 
    
    public List<Transform> spawnPoints;
    
    public int minGuests = 8;


    [Tooltip("Night Market")]
    
    public float nightStartTime = 18f; 

    public float nightEndTime = 1f;
    
    [Tooltip("Day Market")]
    
    public float morningStartTime = 12f;

    public float morningEndTime = 2f;  

    private List<GameObject> spawnedNPCs = new(); // 记录当前刷出来的客人
    
    private bool isPlayerInside = false; // 防止重复触发

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerInside)
        {
            isPlayerInside = true;
            CheckTimeAndSpawn();
        }
    }

    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isPlayerInside)
        {
            isPlayerInside = false;
            ClearNPCs(); 
        }
    }

    private void CheckTimeAndSpawn()
    {
       
        if (timeManager.timeOfDay >= nightStartTime || timeManager.timeOfDay <= nightEndTime)
        {
            StartCoroutine(SpawnNPCsCoroutine(Random.Range(minGuests, 24)));
        }

        else if (timeManager.timeOfDay >= morningStartTime && timeManager.timeOfDay <= morningEndTime)
        {
            StartCoroutine(SpawnNPCsCoroutine(Random.Range(minGuests, 24)));
        }

        else
        {
            StartCoroutine(SpawnNPCsCoroutine(Random.Range(3, minGuests + 1)));
        }
    }

    
    private IEnumerator SpawnNPCsCoroutine(int spawnCount)
    {
        
        // int spawnCount = Random.Range(1, spawnPoints.Count + 1); 
        
        
        List<Transform> shuffledSeats = new(spawnPoints);
        
        for (int i = 0; i < shuffledSeats.Count; i++)
        {
            Transform temp = shuffledSeats[i];
            int randomIndex = Random.Range(i, shuffledSeats.Count);
            shuffledSeats[i] = shuffledSeats[randomIndex];
            shuffledSeats[randomIndex] = temp;
        }

        int sitCount = Random.Range(0, Mathf.Min(spawnCount, shuffledSeats.Count));

        int standCount = spawnCount - sitCount;

        for (int i = 0; i < sitCount; i++)
        {
            int index = Random.Range(0, npcPrefabs.Count);

            GameObject npc = Instantiate(npcPrefabs[index], shuffledSeats[i].position, shuffledSeats[i].rotation);
            spawnedNPCs.Add(npc);

            yield return _waitForSeconds0_1;
        }

        for (int i = 0; i < standCount; i++)
        {
            
        }

    }

    private void ClearNPCs()
    {
        
        StopAllCoroutines(); 
        
        
        foreach (var npc in spawnedNPCs)
        {
            if (npc != null) Destroy(npc);
        }
        spawnedNPCs.Clear();
    }
}