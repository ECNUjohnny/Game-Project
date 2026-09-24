using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;
using System.Diagnostics;
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

    [Header("Dead Eye AudioSources")]

    public AudioSource deadeyeOneShotSource;

    public AudioSource deadeyeLoopSource;

    [Header("Dead Eye AudioClips")]

    public AudioClip DeadEyeStart;
    
    public AudioClip DeadEyeLoop;
    
    public AudioClip DeadEyeEnd;

    [Header("Audio Settings")]

    public float loopDuration = 0.1f;

    private Coroutine loop;
    
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

    void Update()
    {
        bAiming = Input.GetMouseButton(1);
        
        bShooting = Time.unscaledTime >= shooter.NextFireTime && Input.GetMouseButton(0);
        
        // Debug.Log($"{shooter.NextFireTime} / {Time.unscaledTime}");

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

    void DeadEyeAudioStart()
    {
        if (DeadEyeStart != null && deadeyeOneShotSource != null)
        {
            deadeyeOneShotSource.PlayOneShot(DeadEyeStart);
        }

        if (DeadEyeLoop == null || deadeyeLoopSource == null) return;

        if (loop != null) StopCoroutine(loop);
        
        deadeyeLoopSource.clip = DeadEyeLoop;
        deadeyeLoopSource.loop = true;
        deadeyeLoopSource.volume = 0;

        deadeyeLoopSource.Play();

        loop = StartCoroutine(FadeDeadEyeLoop(deadeyeLoopSource, 1f));

    }

    void DeadEyeAudioEnd()
    {
        if (deadeyeOneShotSource != null && DeadEyeEnd != null)
        {
            deadeyeLoopSource.PlayOneShot(DeadEyeEnd);
        }        

        if (deadeyeLoopSource == null || !deadeyeLoopSource.isPlaying) return;

        if (loop != null) StopCoroutine(loop);
        loop = StartCoroutine(FadeDeadEyeLoop(deadeyeLoopSource, 0f));
    }

    IEnumerator FadeDeadEyeLoop(AudioSource source, float targetVolume)
    {
        float startVolume = source.volume;
        float elapsedTime = 0f;

        while (elapsedTime < loopDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / loopDuration);
        
            yield return null;
        }
        
        source.volume = targetVolume;

        if (targetVolume == 0) source.Stop();
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
