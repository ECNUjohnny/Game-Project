using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class NpcInteract : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Dialog Setting")]
    
    public string npcName;

    public KeyCode interactKey = KeyCode.E;

    private bool isPlayerInRange = false;

    private bool Interact = false;

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
        if (isPlayerInRange && Interact)
        {
            UIManager.Instance.ShowInteractionPrompt($"Press {interactKey} to talk with the people");
        
            if (Input.GetKey(interactKey))
            {
                UIManager.Instance.HideInteractionPrompt();
            
                Interact = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            Interact = true;

            // Debug.Log("Player nearby");
        }    
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }  
}
