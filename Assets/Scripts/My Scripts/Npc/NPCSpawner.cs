using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("Spawning Setting")]

    public GameObject npcPrefab;

    public float respawnDelay = 30f;

    public float minSpawnDis = 20f;

    public DialogueData dialogueData;

    [Header("Reference")]

    public Transform player;

    private GameObject currentNpcInstance;
    
    private NpcHealth currentNpcHealth;

    private NpcInteract npcInteract;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("player")?.transform;
        }

        SpawnNpc();

        npcInteract = currentNpcInstance.GetComponent<NpcInteract>();

        if (npcInteract != null)
        {
            npcInteract.dialogueData = dialogueData;
        }
    }

    private void SpawnNpc()
    {
        if (npcPrefab == null) return;

        currentNpcInstance = Instantiate(npcPrefab, transform.position, transform.rotation);
    
        currentNpcHealth = currentNpcInstance.GetComponent<NpcHealth>();

        if (currentNpcHealth != null)
        {
            currentNpcHealth.OnDeath += HandleNpcDeath;

        }
        else
        {
            Debug.Log("There is no health system on this npc");
        }
    }

    private void HandleNpcDeath()
    {
        if (currentNpcHealth != null)
        {
            currentNpcHealth.OnDeath -= HandleNpcDeath;
        }

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (player != null)
        {
            float minDistSqr = minSpawnDis * minSpawnDis;

            while ((transform.position - player.position).sqrMagnitude < minDistSqr)
            {
                yield return new WaitForSeconds(1f);
            }
        }

        SpawnNpc();
    }

    void OnDestroy()
    {
        if (currentNpcHealth != null)
        {
            currentNpcHealth.OnDeath -= HandleNpcDeath;
        }
    }
}
