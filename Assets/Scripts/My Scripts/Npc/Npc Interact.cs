using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(BoxCollider))]

[RequireComponent(typeof(StoreInventory))]
public class NpcInteract : MonoBehaviour
{
    [Header("NPC Roles (可多选开关)")]
    [Tooltip("勾选则对话后可以接任务")]

    public bool isMissionGiver = false;

    [Tooltip("勾选则对话后可以打开商店")]
    
    public bool isMerchant = false;



    [Header("NPC Info")]

    public string npcName;

    public DialogueData dialogueData;

    [Header("Interaction Keys")]

    public KeyCode interactKey = KeyCode.E;

    public KeyCode missionKey = KeyCode.F; // 专属任务键

    public KeyCode storeKey = KeyCode.G;   // 专属商店键

    [Header("Events")]

    public UnityEvent onTaskAccepted;

    public UnityEvent onStoreOpened;

    [Header("Camera Setting")]


    public Transform cameraFocusPoint;

    // --- 私有状态变量 ---

    private bool isPlayerInRange = false;

    private bool isInteract = false;


    private bool isWaitingForAction = false;

    private BoxCollider col;

    void Start()
    {
        col = GetComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0, 1.4f, 1.25f);
        col.size = new Vector3(2f, 2f, 2.5f);

    }

    void Update()
    {
        if (!isPlayerInRange) return;
        
        // 阶段一：等待玩家触发对话
        if (!isInteract && !isWaitingForAction)
        {
            UIManager.Instance.ShowInteractionPrompt($"Press {interactKey} to talk with {npcName}");
        
            if (Input.GetKeyDown(interactKey))
            {
                UIManager.Instance.HideInteractionPrompt();
                isInteract = true;

                UIManager.Instance.StartDialogue(dialogueData, () => {
                    // 对话结束回调
                    HandleDialogueEnd();
                });
            }
        }
        // 阶段二：对话结束，等待玩家执行后续动作
        else if (isWaitingForAction)
        {
            // 如果是任务NPC，且按下了任务键
            if (isMissionGiver && Input.GetKeyDown(missionKey))
            {
                onTaskAccepted?.Invoke();
                EndInteraction();
            }
            
            // 如果是商人，且按下了商店键
            if (isMerchant && Input.GetKeyDown(storeKey))
            {
                onStoreOpened?.Invoke();
                EndInteraction();
            }
        }
    }

    public void TriggerStore()
    {
        StoreInventory myInventory = GetComponent<StoreInventory>();

        StoreManager.Instance.OpenStore(myInventory, cameraFocusPoint);
    }

    // 核心逻辑：动态生成交互提示
    private void HandleDialogueEnd()
    {
        // 如果两个都没勾，说明是纯聊天 NPC
        if (!isMissionGiver && !isMerchant)
        {
            isInteract = false; 
            return;
        }

        isWaitingForAction = true;

        // 动态拼接 UI 提示文本
        string promptMsg = "";
        
        if (isMissionGiver) 
        {
            promptMsg += $"Press {missionKey} for Mission   ";
        }
        
        if (isMerchant) 
        {
            promptMsg += $"Press {storeKey} for Store";
        }

        UIManager.Instance.ShowInteractionPrompt(promptMsg.Trim());
    }

    private void EndInteraction()
    {
        isWaitingForAction = false;
        isInteract = false; 
        UIManager.Instance.HideInteractionPrompt();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            isInteract = false;
        }    
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            isWaitingForAction = false;
            isInteract = false;
            
            UIManager.Instance.HideInteractionPrompt();
        
            if (UIManager.Instance.isInDialogue)
            {
                UIManager.Instance.EndDialogue(); 
            }
        }
    }  
}
