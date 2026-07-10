using UnityEngine;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]

[RequireComponent(typeof(PlayerCombat))]
public class PlayerMovement : MonoBehaviour
{
    public Transform cam; 
    
    public float Speed = 2f;

    public Transform spine;

    public KeyCode Vision = KeyCode.T;
    
    [Header("The speed turning to the pos of the cam")]
    
    public float turnSpeed = 2f; 
    
    public float gravity = -19.8f;
    
    public float jumpHeight = 2f;
    
    public bool isGrounded;
    
    public bool bJumping;
    
    private CharacterController controller;
    
    public float h = 0;
    
    public float v = 0;
    
    private float hRaw;
    
    private float vRaw;
    
    private float hTarget;
    
    private float vTarget;
    
    private float Speedy;
    
    public Vector3 velocity;

    public PlayerCombat combatScript;
    
    
    [SerializeField]
    
    [Header("The speed for transforming walk to run")]
    
    private float transitionSpeed = 5f;

    private float deltaTime = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        combatScript = GetComponent<PlayerCombat>();
    
        isGrounded = true;

        bJumping = false;
    }

    void Update()
    {
        hRaw = Input.GetAxisRaw("Horizontal");
        vRaw = Input.GetAxisRaw("Vertical");
        isGrounded = controller.isGrounded;


        if (isGrounded) bJumping = false;

        hTarget = Input.GetKey(KeyCode.LeftShift) ? hRaw * 2.0f : hRaw;
        vTarget = Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1) ? vRaw * 2.0f : vRaw;

        if (Input.GetMouseButton(1)) hTarget = 0;

        deltaTime = combatScript.GetPlayerDeltaTime();

        h = Mathf.Lerp(h, hTarget, transitionSpeed * deltaTime);
        v = Mathf.Lerp(v, vTarget, transitionSpeed * deltaTime);

        Vector3 camForward = cam.forward;
        camForward.y = 0f; 
        // camForward.Normalize(); 

        Vector3 playerForward = transform.forward;
        playerForward.y = 0;
        // playerForward.Normalize();
        // float check = Vector3.Dot(playerForward, camForward);

        Quaternion targetRot = Quaternion.LookRotation(camForward);

        if (!Input.GetKey(Vision) && ((vRaw != 0 || transform.forward.y != 0) || (vRaw == 0 && Input.GetMouseButton(1))))
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * deltaTime);
        }

        Vector3 moveDir = transform.forward * vRaw + transform.right * hRaw;

        Speedy = velocity.y;

        velocity = Input.GetKey(KeyCode.LeftShift) ? 2.4f * Speed * moveDir : moveDir * Speed;

        velocity.y = Speedy;

        if (isGrounded && !Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = -0.1f;
        }
        else if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            bJumping = true;

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * deltaTime;

        controller.Move(velocity * deltaTime);
    }

    void LateUpdate()
    {
        if (!Input.GetKey(Vision) && Input.GetMouseButton(1))
        {
    
            float pitchAngle = cam.eulerAngles.x;

            if (pitchAngle > 180f)
            {
                pitchAngle -= 360f;
            }

            spine.Rotate(cam.right, pitchAngle, Space.World);
        }
    }
}