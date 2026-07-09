using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TavernSpawner : MonoBehaviour
{

    [Tooltip("代表自由站立区域的 Collider")]
    public List<BoxCollider> freeStandAreas;

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
    
    public float morningStartTime = 10f;

    public float morningEndTime = 14f;  

    private List<GameObject> spawnedNPCs = new(); // 记录当前刷出来的客人
    
    private bool isPlayerInside = false; // 防止重复触发

    private enum TimePeriod { Night, Morning, Day, None }

    private TimePeriod lastSpawnedPeriod = TimePeriod.None;

    
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
            HideNPCs(); 
        }
    }

    private TimePeriod GetCurrentTimePeriod()
    {
        float t = timeManager.timeOfDay;

        if (t >= nightStartTime || t <= nightEndTime) return TimePeriod.Night;

        else if (morningStartTime <= t && t <= morningEndTime) return TimePeriod.Morning;

        return TimePeriod.Day;
    }

    private void CheckTimeAndSpawn()
    {
        TimePeriod currentPeriod = GetCurrentTimePeriod();

        if (lastSpawnedPeriod == currentPeriod && spawnedNPCs.Count > 0)
        {
            foreach (var npc in spawnedNPCs)
            {
                if (npc != null) npc.SetActive(true);
            }

            return;
        }
        
        DestroyNPCs();

        lastSpawnedPeriod = currentPeriod;

        if (npcPrefabs.Count == 0) return;
        
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
    
        // if (npcPrefabs.Count == 0) yield return null;


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
            Vector3 finalPosition = Vector3.zero;
            bool foundValidPoint = false;
            
            BoxCollider selectedArea = freeStandAreas[Random.Range(0, freeStandAreas.Count)];
            Bounds bounds = selectedArea.bounds;

            Vector3 randomPos = new(Random.Range(bounds.min.x, bounds.max.x), bounds.center.y, Random.Range(bounds.min.z, bounds.max.z));

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                finalPosition = hit.position;
                
                foundValidPoint = true;

                break;
            }

            if (foundValidPoint)
            {
                int index = Random.Range(0, npcPrefabs.Count);

                Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360f), 0);

                GameObject npc = Instantiate(npcPrefabs[index], finalPosition, randomRot);

                spawnedNPCs.Add(npc);

                yield return _waitForSeconds0_1;
            }
        }



    }

    private void HideNPCs()
    {
        
        StopAllCoroutines(); 
        
        
        foreach (var npc in spawnedNPCs)
        {
            if (npc != null) npc.SetActive(false);
        }

    }

    private void DestroyNPCs()
    {
        StopAllCoroutines();

        foreach (var npc in spawnedNPCs)
        {
            if (npc != null) Destroy(npc, 0.1f);
        }

        spawnedNPCs.Clear();
    }
}