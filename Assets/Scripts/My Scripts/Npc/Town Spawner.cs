using UnityEngine;
using System.Collections;

public class TownSpawner : MonoBehaviour
{
    [Header("Spawn Setting")]

    public int maxNpcs = 20;

    public Transform[] spawnPoints;

    public float spawnInterval = 5f;

    public int currentNpcCounts = 0;  

    public GameObject[] NPCPrefabs; 

    void Start()
    {
        StartCoroutine(SpawnRoutine());        
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (currentNpcCounts < maxNpcs && spawnPoints.Length > 0)
            {
                SpawnNpc();
                Debug.Log($"{currentNpcCounts}");
            }            

            yield return new WaitForSeconds(spawnInterval);
        }

    }

    void SpawnNpc()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedPoint = spawnPoints[randomIndex];

        int randomNPCIndex = Random.Range(0, NPCPrefabs.Length);
        GameObject selectedNPC = NPCPrefabs[randomNPCIndex];

        Instantiate(selectedNPC, selectedPoint.position, selectedPoint.rotation);
        currentNpcCounts++;
    }

}
