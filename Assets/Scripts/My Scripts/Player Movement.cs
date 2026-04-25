using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Transform cam; 
    public float Speed = 2f;
    [Tooltip("The speed turning to the pos of the cam")]
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
    [SerializeField]
    [Tooltip("The speed for transforming walk to run")]
    private float transitionSpeed = 5f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    
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

        h = Mathf.Lerp(h, hTarget, Time.deltaTime * transitionSpeed);
        v = Mathf.Lerp(v, vTarget, Time.deltaTime * transitionSpeed);

        Vector3 camForward = cam.forward;
        if (!Input.GetMouseButton(1)) camForward.y = 0f; 
        camForward.Normalize(); 

        Vector3 playerForward = transform.forward;
        playerForward.y = 0;
        playerForward.Normalize();
        float check = Vector3.Dot(playerForward, camForward);

        Quaternion targetRot = Quaternion.LookRotation(camForward);

        if (!Input.GetKey(KeyCode.T) && ((check > 0.01 && (vRaw != 0 || transform.forward.y != 0)) || (vRaw == 0 && Input.GetMouseButton(1))))
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
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

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}