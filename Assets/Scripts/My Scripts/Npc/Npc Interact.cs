using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class NpcInteract : MonoBehaviour
{
    [Header("Dialogue")]

    public DialogueData dialogueData;

    [Header("Mission and Store and other options")]

    public KeyCode actionKey = KeyCode.E;

    public UnityEvent onTaskAccepted;

    [Header("Dialogue Setting")]
    
    public string npcName;

    public KeyCode interactKey = KeyCode.E;

    public Transform cameraFocusPoint;

    private bool isPlayerInRange = false;

    private bool isInteract = false;

    private bool isWaitingforTask = false;

    private BoxCollider col;

    void Start()
    {
        col = GetComponent<BoxCollider>();

        col.isTrigger = true;

        col.center = new(0, 1.4f, 1.25f);

        col.size = new(2f, 2f, 2.5f);
    }

    void Update()
    {
        if (!isPlayerInRange) return;
        
        if (!isInteract && !isWaitingforTask)
        {
            UIManager.Instance.ShowInteractionPrompt($"Press {interactKey} to talk with the people");
        
            if (!isWaitingforTask && Input.GetKey(interactKey))
            {
                UIManager.Instance.HideInteractionPrompt();

                UIManager.Instance.StartDialogue(dialogueData, () => {
                    
                    isWaitingforTask = true;
                    UIManager.Instance.ShowInteractionPrompt($"Press {actionKey} to start the mission");
                
                });
            
                isInteract = true;
            }
        }
        else if (isWaitingforTask && Input.GetKey(actionKey))
        {
            onTaskAccepted?.Invoke();

            isWaitingforTask = false;

            UIManager.Instance.HideInteractionPrompt();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            isInteract = false;

            // Debug.Log("Player nearby");
        }    
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            isWaitingforTask = false;
            
            UIManager.Instance.HideInteractionPrompt();
        
            if (UIManager.Instance.isInDialogue)
            {
                UIManager.Instance.EndDialogue(); 
            }
        }
    }  

    
}
