using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Transform cam; 
    public float speed = 2f;
    [Tooltip("The speed turning to the pos of the cam")]
    public float turnSpeed = 2f; 
    public float gravity = -9.8f;
    public float jumpHeight = 2f;
    public bool isGrounded;
    public bool bJumping;
    private CharacterController controller;
    private float verticalVelocity;
    public float h = 0;
    public float v = 0;
    private float hRaw;
    private float vRaw;
    private float hTarget;
    private float vTarget;
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
        hRaw = Input.GetAxis("Horizontal");
        vRaw = Input.GetAxis("Vertical");
        isGrounded = controller.isGrounded;

        Debug.Log(controller.isGrounded);

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

        if ((check > 0.3 && (vRaw != 0 || transform.forward.y != 0)) || (vRaw == 0 && Input.GetMouseButton(1)))
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        Vector3 moveDir = transform.forward * vRaw + transform.right * hRaw;

        float tmp = Input.GetKey(KeyCode.LeftShift) ? speed * 2.4f : speed;

        Vector3 velocity = moveDir * tmp;

        if (isGrounded && !Input.GetKey(KeyCode.Space))
        {
            velocity.y = -0.5f;
        }
        else if (isGrounded && Input.GetKey(KeyCode.Space))
        {
            bJumping = true;

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}