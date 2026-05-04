using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    // Start is called before the first frame update
    public bool bAiming;
    
    public bool bShooting;
    
    private bool bDeadEye;
    
    private float TimeScale = 0.35f;
    
    private float defaultFixedDeltaTime;
    
    private float maxDeadEyeTime;
    
    private float currentTime;
    
    [Tooltip("Remain of the Dead Eye")]
    
    public Image DeadEyeMeter;
    
    [Tooltip("Material used for DeadEye PostScreen")]
    
    public Material DeadEyeMaterial;
    
    private Coroutine scanCoroutine;
    
    [Tooltip("Existing time for the scanLine")]
    
    public float scanDuration = 0.2f;
    
    void Start()
    {
        bDeadEye = false;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
        maxDeadEyeTime = 10f;
        currentTime = 0;
        DeadEyeMaterial.SetFloat("_ScanLine", 0);
    }

    // Update is called once per frame
    void Update()
    {
        bAiming = Input.GetMouseButton(1);
        bShooting = Input.GetMouseButton(0);
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            DeadEye();
        }
        
        if (bDeadEye)
        {
            currentTime += Time.unscaledDeltaTime;

            DeadEyeMeter.fillAmount = 1.0f - currentTime / maxDeadEyeTime; 

            if (currentTime > maxDeadEyeTime)
            {
                DeadEye();
            }    
        }

        if (!bDeadEye && DeadEyeMeter.fillAmount != 1.0f) DeadEyeMeter.fillAmount += Time.deltaTime / 10;
    }

    void DeadEye()
    {
        bDeadEye = !bDeadEye;
        if (bDeadEye)
        {
            Time.timeScale = TimeScale;
            Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
            
            currentTime = 0;
            DeadEyeMeter.fillAmount = 1.0f;

            if (scanCoroutine != null) StopCoroutine(scanCoroutine);
            scanCoroutine = StartCoroutine(AnimateScanLine(0, 1.0f));
        }
        else
        {
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = defaultFixedDeltaTime;
        
            if (scanCoroutine != null) StopCoroutine(scanCoroutine);
            scanCoroutine = StartCoroutine(AnimateScanLine(1f, 0f));
        }
    }

    IEnumerator AnimateScanLine(float stVal, float enVal)
    {
        float elapsedTime = 0;

        while (elapsedTime < scanDuration)
        {
            elapsedTime += Time.unscaledDeltaTime * 2f;
            float ratio = elapsedTime / scanDuration;
            float currentVal = Mathf.Lerp(stVal, enVal, ratio);

            DeadEyeMaterial.SetFloat("_ScanLine", currentVal);

            yield return null;
        }

        DeadEyeMaterial.SetFloat("_ScanLine", enVal);
    }   
}
