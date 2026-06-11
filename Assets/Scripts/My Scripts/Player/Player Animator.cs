using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int TDrawHash = Animator.StringToHash("tDraw");
    
    private static readonly int TShootHash = Animator.StringToHash("tShoot");
    
    private static readonly int TReloadHash = Animator.StringToHash("tReload");
    
    private static readonly int WeaponTypeHash = Animator.StringToHash("weaponType");
    
    private static readonly int SpeedYHash = Animator.StringToHash("SpeedY");
    
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    
    private static readonly int BJumpingHash = Animator.StringToHash("bJumping");
    
    private static readonly int BShootingHash = Animator.StringToHash("bShooting");
    
    private static readonly int BAimingHash = Animator.StringToHash("bAiming");
    
    private static readonly int InputYHash = Animator.StringToHash("InputY");
    
    private static readonly int InputXHash = Animator.StringToHash("InputX");
    
    private Animator animator;
    
    private PlayerCombat combatScript;
    
    private PlayerMovement movementScript;
    
    [SerializeField]
    
    private Transform spineBone;
    
    [SerializeField]
    
    private Transform Camera;
    
    private float maxUpAngle = -75f;
    
    private float maxDownAngle = 75f;
    
    public Vector3 rotationOffset;
    
    public GameObject Aim;
    
    void Start()
    {
        
        animator = GetComponent<Animator>();
        
        combatScript = GetComponent<PlayerCombat>();
        
        movementScript = GetComponent<PlayerMovement>();
        
        Aim.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
        animator.SetFloat(InputXHash, movementScript.h);
        
        animator.SetFloat(InputYHash, movementScript.v);
        
        animator.SetBool(BAimingHash, combatScript.bAiming);
        
        animator.SetBool(BShootingHash, combatScript.bShooting);
        
        animator.SetBool(BJumpingHash, movementScript.bJumping);
        
        animator.SetBool(IsGroundedHash, movementScript.isGrounded);
        
        animator.SetFloat(SpeedYHash, movementScript.velocity.y);

        animator.SetInteger(WeaponTypeHash, combatScript.weaponType);

        
        if (Aim != null)
        {
            Aim.SetActive(combatScript.bAiming);
        }
        
    }

    public void TriggerReloadAnimation()
    {
        animator.SetTrigger(TReloadHash);
    }

    public void TriggerShootAnimation()
    {
        animator.SetTrigger(TDrawHash);
    }

    /*void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            float camAngle = Camera.localEulerAngles.x;
            if (camAngle > 180f)
            {
                camAngle -= 360f;
            }

            camAngle = Mathf.Clamp(camAngle, maxUpAngle, maxDownAngle);

            Quaternion aimRotation = Quaternion.Euler(camAngle + rotationOffset.x, rotationOffset.y, rotationOffset.z);

            spineBone.localRotation *= aimRotation;
        }
    }*/
}
