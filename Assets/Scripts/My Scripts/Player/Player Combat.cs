using UnityEngine;
using UnityEngine.UI;
using System.Collections;
[RequireComponent(typeof(PlayerShooter))]

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    // Start is called before the first frame update
    public bool bAiming;
    
    public bool bShooting;

    public int weaponType;
    
    private bool bDeadEye;
    
    public float worldTimeScale = 0.35f;
    
    private float defaultFixedDeltaTime;


    public float playerTimeScale = 0.65f;

    private float maxDeadEyeTime;
    
    private float currentTime;

    private PlayerShooter shooter;

    public Animator playerAnimator;
    
    [Tooltip("Remain of the Dead Eye")]
    
    public Image DeadEyeMeter;
    
    [Tooltip("Material used for DeadEye PostScreen")]
    
    public Material DeadEyeMaterial;
    
    private Coroutine scanCoroutine;

    
    [Tooltip("Existing time for the scanLine")]
    
    public float scanDuration = 0.2f;

    public float DeadEyeEnergyRecover = 60f;
    
    void Start()
    {
        bDeadEye = false;
        
        defaultFixedDeltaTime = Time.fixedDeltaTime;
        
        maxDeadEyeTime = 10f;
        
        currentTime = 0;

        shooter = GetComponent<PlayerShooter>();

        playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        
        DeadEyeMaterial.SetFloat("_ScanLine", 0);

    }

    // Update is called once per frame
    void Update()
    {
        bAiming = Input.GetMouseButton(1);
        
        bShooting = Input.GetMouseButton(0) && Time.unscaledTime >= shooter.NextFireTime;

        
        
        if (DeadEyeMeter.fillAmount > 0 && Input.GetKeyDown(KeyCode.CapsLock))
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

        if (!bDeadEye && DeadEyeMeter.fillAmount != 1.0f)
        {

            DeadEyeMeter.fillAmount += Time.deltaTime / DeadEyeEnergyRecover;
            currentTime -= Time.deltaTime * maxDeadEyeTime / DeadEyeEnergyRecover;
        }

    }

    void DeadEye()
    {
        bDeadEye = !bDeadEye;

        if (bDeadEye)
        {
            Time.timeScale = worldTimeScale;
            Time.fixedDeltaTime = defaultFixedDeltaTime * worldTimeScale;
            playerAnimator.speed = playerTimeScale;


            if (scanCoroutine != null) StopCoroutine(scanCoroutine);
            scanCoroutine = StartCoroutine(AnimateScanLine(0, 1.0f));
        }
        else
        {
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = defaultFixedDeltaTime;
            playerAnimator.speed = 1.0f;

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

    public float GetPlayerDeltaTime()
    {
        if (bDeadEye)
        {
            return playerTimeScale * Time.unscaledDeltaTime;
        }
        else
        {
            return Time.deltaTime;
        }
    }

    public float GetCurrentPlayerTimeScale()
    {
        return bDeadEye ? playerTimeScale : 1.0f;
    }

}
