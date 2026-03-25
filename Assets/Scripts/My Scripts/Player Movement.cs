using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Transform cam; 
    public float speed = 2f;
    public float turnSpeed = 2f;
    public float gravity = 9.8f;
    private CharacterController controller;
    private float verticalVelocity;
    private bool wasAiming;
    public float h = 0;
    public float v = 0;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        h = Input.GetKey(KeyCode.LeftShift) ? h * 2 : h;
        v = Input.GetKey(KeyCode.LeftShift) ? v * 2 : v;

        Vector3 camForward = cam.forward;
        if (!Input.GetMouseButton(1)) camForward.y = 0f; 
        camForward.Normalize(); 

        Vector3 playerForward = transform.forward;
        playerForward.Normalize();
        float check = Vector3.Dot(playerForward, camForward);

        if ((check > 0.3 && v != 0) || Input.GetMouseButton(1))
        {
            Quaternion targetRot = Quaternion.LookRotation(camForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        Vector3 moveDir = transform.forward * v + transform.right * h;

        if (controller.isGrounded)
        {
            verticalVelocity = -0.5f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        float tmp = Input.GetKey(KeyCode.LeftShift) ? speed * 1.4f : speed;

        Vector3 velocity = moveDir * tmp;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        wasAiming = Input.GetMouseButton(1);
    }
}