using System.Collections;
using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    public GameObject interactPromptPanel;

    public GameObject dialoguePanel;
    
    public TextMeshProUGUI interactText;

    public TextMeshProUGUI dialogueText;

    public float timebetweenDialogue = 5f;

    public bool isInDialogue { get; private set; }

    private bool isInEvent;

    private Action onDialogueCompleteCallback;

    
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
        HideDialoguePanle();
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

    public void StartDialogue(DialogueData dialogueData, Action onComplete = null)
    {
        if (isInDialogue) return;

        isInDialogue = true;

        onDialogueCompleteCallback = onComplete;

        StartCoroutine(Dialogue(dialogueData));
    }

    public void EndDialogue()
    {
        if (!isInDialogue) return;

        isInDialogue = false;

        onDialogueCompleteCallback?.Invoke();

        onDialogueCompleteCallback = null;
    }

    IEnumerator Dialogue(DialogueData dialogueData)
    {
        for (int i = 0; i < dialogueData.dialogues.Length; i++)
        {
            ShowDialoguePanel(dialogueData.dialogues[i]);

            yield return new WaitForSeconds(timebetweenDialogue);
        }

        HideDialoguePanle();

        EndDialogue();
    }
}
