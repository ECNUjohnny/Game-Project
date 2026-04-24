using UnityEngine;

[RequireComponent(typeof(DoorControl))]
public class DoorInteraction : MonoBehaviour
{
    public DoorControl Script;
    public string prompt = "Prees 'E' to open the door";
    private bool isPlayerNearby = false;
    private bool isDoorOpen = false;
    private float time = 0;
    public float CloseDoor = 7.0f;

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Script.OpenTheDoor();

            UIManager.Instance.HideInteractionPrompt();

            isDoorOpen = true;
        }

        if (time > CloseDoor)
        {
            if (isDoorOpen) Script.OpenTheDoor();
            
            isDoorOpen = false;

            time = 0;
        }

        time += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            //Debug.Log(1);
            
            if (!isDoorOpen) UIManager.Instance.ShowInteractionPrompt(prompt);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            //Script.OpenTheDoor();
            
            UIManager.Instance.HideInteractionPrompt();
        }
    }
}
