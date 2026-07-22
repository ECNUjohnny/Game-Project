using System.Collections;
using UnityEngine;

public class StoreManager : MonoBehaviour
{

    public static StoreManager Instance { get; private set; }

    [Header("UI referance")]

    public GameObject storeUIContainer;

    public Transform contentPanel;

    public GameObject itemSlotPrefab;


    [Header("Camera Transition")]

    public Camera mainCamera;

    public float transitionDuration = 1.0f;

    public MonoBehaviour playerCameraController;

    private Vector3 originalCameraPos;

    private Quaternion originalCameraRot;

    private bool isStoreOpen = false;

    public bool isStoreTime = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        storeUIContainer.SetActive(false);
    }

    public void OpenStore(StoreInventory inventory, Transform focusPoint)
    {
        if (isStoreOpen) return;
        isStoreOpen = true;

        if (playerCameraController != null) playerCameraController.enabled = false;

        originalCameraPos = mainCamera.transform.position;
        originalCameraRot = mainCamera.transform.rotation;

        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (ItemData item in inventory.inventory )
        {
            GameObject slotGO = Instantiate(itemSlotPrefab, contentPanel);

            slotGO.GetComponent<StoreItemSlot>().Setup(item);
        } 

        StartCoroutine(CameraTransition(focusPoint.position, focusPoint.rotation, true)); 
    }

    public void CloseStore()
    {
        if (!isStoreOpen) return;

        storeUIContainer.SetActive(false);

        StartCoroutine(CameraTransition(originalCameraPos, originalCameraRot, false));
    }

    private IEnumerator CameraTransition(Vector3 targetPos, Quaternion targetRot, bool isOpen)
    {
        float elapsedTime = 0f;

        mainCamera.transform.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float t = Mathf.SmoothStep(0, 1, elapsedTime / transitionDuration);

            mainCamera.transform.SetPositionAndRotation(Vector3.Lerp(startPos, targetPos, t), Quaternion.Lerp(startRot, targetRot, t));
            
            yield return null;
        }

        
        mainCamera.transform.SetPositionAndRotation(targetPos, targetRot);
        
        if (isOpen)
        {
            storeUIContainer.SetActive(true);   
        }
        else
        {
            if (playerCameraController != null) playerCameraController.enabled = true;

            isStoreOpen = false;            
        }
    }
}
