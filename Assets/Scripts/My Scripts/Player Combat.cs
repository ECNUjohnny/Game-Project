using UnityEditor.Media;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    // Start is called before the first frame update
    public bool bAiming;
    public bool bShooting;
    private bool bDeadEye;
    private float TimeScale = 0.5f;
    private float defaultFixedDeltaTime;
    private float maxDeadEyeTime;
    private float currentTime;
    public Image DeadEyeMeter;
    public Material DeadEyeMaterial;
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

            float ratio = 1.0f - currentTime / maxDeadEyeTime;
            DeadEyeMeter.fillAmount = ratio; 

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
        }
        else
        {
            Time.timeScale = 1.0f;

            Time.fixedDeltaTime = defaultFixedDeltaTime;
        }
    }
}
