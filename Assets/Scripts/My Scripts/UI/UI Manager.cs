using System.Collections;
using System;
using TMPro;
using UnityEngine;



public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("Data source")]

    public PlayerInventory playerInventory;

    public PlayerShooter playerShooter;

    public WeaponManager weaponManager;

    private WeaponController weaponController;

    [Header("UI setting")]
    public GameObject interactPromptPanel;

    public GameObject dialoguePanel;
    
    public TextMeshProUGUI interactText;

    public TextMeshProUGUI dialogueText;

    public float timebetweenDialogue = 5f;

    public TextMeshProUGUI goldText; 

    public TextMeshProUGUI ammoText;

    public GameObject goldPanel;

    public CanvasGroup goldCanvasGroup;

    public KeyCode showGoldPanel = KeyCode.Z;


    public bool isInDialogue { get; private set; }

    private bool isInEvent;

    private Action onDialogueCompleteCallback;

    private float fadeDuration = 0.5f;

    private float duration = 4.0f;

    private Coroutine fadeCoroutine;

    
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

        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += RefreshUI;

        }

        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged += HandleWeaponSwitch;
        }

        if (playerShooter != null && playerShooter.currentWeaponController != null)
        {
            HandleWeaponSwitch(playerShooter.currentWeaponController);
        }
        else
        {
            RefreshUI(false);
        }

        goldPanel.SetActive(false);

    }

    void Update()
    {
        if (Input.GetKeyDown(showGoldPanel))
        {
            ShowGoldPanle();
        }
    }

    IEnumerator GoldPanelCoroutine()
    {
        float time = 0;

        goldPanel.SetActive(true);

        goldCanvasGroup.alpha = 0;

        while (time < fadeDuration)
        {
            goldCanvasGroup.alpha = Mathf.Lerp(0, 1, time / fadeDuration);
            
            time += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(duration);

        time = 0;

        while (time < fadeDuration)
        {
            goldCanvasGroup.alpha = Mathf.Lerp(1, 0, time / fadeDuration);
            
            time += Time.deltaTime;

            yield return null;
        }

        goldCanvasGroup.alpha = 0;

        goldPanel.SetActive(false);
    }

    void ShowGoldPanle()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(GoldPanelCoroutine());
    }

    private void HandleWeaponSwitch(WeaponController newWeapon)
    {
        if (weaponController != null)
        {
            weaponController.OnAmmoChanged -= RefreshUI;
        }

        weaponController = newWeapon;

        if (weaponController != null)
        {
            weaponController.OnAmmoChanged += RefreshUI;

        }

        RefreshUI(false);
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
        if (dialogueData != null)
        {
             for (int i = 0; i < dialogueData.dialogues.Length; i++)
            {
                ShowDialoguePanel(dialogueData.dialogues[i]);

                yield return new WaitForSeconds(timebetweenDialogue);
            }    
        }        
        
       

        HideDialoguePanle();

        EndDialogue();
    }
    
    private void RefreshUI(bool gold)
    {
        if (goldText != null)
        {
            goldText.text = $"{playerInventory.gold}";

            if (gold)
            {

                ShowGoldPanle();
            }
        }


        if (playerInventory != null && weaponController != null && ammoText != null)
        {
            ammoText.text = $"{playerInventory.ammo[(int)playerInventory.ammoType]}/{weaponController.CurrentAmmo}";
        }

    
    }
}
