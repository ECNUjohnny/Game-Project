using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance {get; private set;}
    
    public GameObject interactPromptPanel;

    public GameObject dialoguePanel;
    
    public TextMeshProUGUI interactText;

    public TextMeshProUGUI dialogueText;

    public float timebetweenDialogue = 5f;

    private bool isInDialogue;

    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        HideInteractionPrompt();
    }

    public void HideInteractionPrompt()
    {
        interactPromptPanel.SetActive(false);
    }
    
    public void ShowInteractionPrompt(string message)
    {
        interactText.text = message;
        interactPromptPanel.SetActive(true);
    }

    public void ShowDialoguePanel(string message)
    {
        dialogueText.text = message;
        dialoguePanel.SetActive(true);
    }

    public void HideDialoguePanle()
    {
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(string message)
    {
        ShowDialoguePanel(message);

        StartCoroutine(Dialogue());
    }

    IEnumerator Dialogue()
    {
        yield return new WaitForSeconds(timebetweenDialogue);

        HideDialoguePanle();
    }
}
