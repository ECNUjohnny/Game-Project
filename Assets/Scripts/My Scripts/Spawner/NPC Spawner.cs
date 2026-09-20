using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new(1f);

    public enum NpcState
    {
        IDLE,

        Talk,

        farmLabor,

        chopLaber,

        fishLaber,

        Sentinal,   
    }

    [Header("Spawning Setting")]

    public GameObject npcPrefab;

    public float respawnDelay = 30f;

    public float minSpawnDis = 20f;

    public DialogueData dialogueData;

    public Material npcMat;

    public NpcState initState;

    public Transform cameraFocusPoint;


    [Header("Reference")]

    public Transform player;

    private GameObject currentNpcInstance;
    
    private NpcHealth currentNpcHealth;

    private NpcInteract npcInteract;

    private NpcAnimator npcAnimator;


    public List<ItemData> inventory;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        SpawnNpc();

        npcInteract = currentNpcInstance.GetComponent<NpcInteract>();

        if (npcInteract != null)
        {
            npcInteract.dialogueData = dialogueData;
        
            npcInteract.inventory = inventory;

            // foreach (ItemData item in npcInteract.inventory)
            // {
            //     Debug.Log($"{item.name}\n");
            // }
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
            Debug.Log($"There is no health system on this npc: {npcPrefab.name}");
        }

        if (currentNpcInstance.TryGetComponent(out npcAnimator))
        {
            if (initState == NpcState.Talk)
            {
                npcAnimator.InitTalk();
            } 
            else if (initState == NpcState.farmLabor)
            {
                                
            }
            
        }

        if (npcMat != null)
        {
            if (currentNpcInstance.TryGetComponent(out LODGroup lodGroup))
            {
                LOD[] lods = lodGroup.GetLODs();

                for (int i = 0; i < lods.Length; i++)
                {
                    foreach (Renderer r in lods[i].renderers)
                    {
                        SkinnedMeshRenderer lodSmr = r as SkinnedMeshRenderer;
                        if (lodSmr != null)
                        {
                            Material[] materials = lodSmr.materials;
                            materials[0] = npcMat;
                            lodSmr.materials = materials;
                        }   
                    }
                }   
            }            
        }

        if (cameraFocusPoint != null)
        {
            if (currentNpcInstance.TryGetComponent(out NpcInteract interact))
            {
                interact.cameraFocusPoint = cameraFocusPoint;   
            }
        }

        if (currentNpcInstance.TryGetComponent(out NpcMovement movement))
        {
            movement.enabled = false;
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
                yield return _waitForSeconds1;
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
