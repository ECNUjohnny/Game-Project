using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public DoorControl Script;
    public string prompt = "Prees 'E' to open the door";
    private bool isPlayerNearby = false;
    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Script.OpenTheDoor();

            UIManager.Instance.HideInteractionPrompt();
        }
        else if (!isPlayerNearby)
        {
            Script.OpenTheDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            
            UIManager.Instance.ShowInteractionPrompt(prompt);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            
            UIManager.Instance.HideInteractionPrompt();
        }
    }
}
