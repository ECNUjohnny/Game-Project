using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Transform cam; 
    public float speed = 2f;
    public float turnSpeed = 2f; //转向摄像机方向的速度
    public float gravity = 9.8f;
    private CharacterController controller;
    private float verticalVelocity;
    private bool wasAiming;
    public float h = 0;
    public float v = 0;
    private float hRaw;
    private float vRaw;
    private float hTarget;
    private float vTarget;
    [SerializeField]
    private float transitionSpeed = 5f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        hRaw = Input.GetAxis("Horizontal");
        vRaw = Input.GetAxis("Vertical");

        hTarget = Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1) ? hRaw * 2.0f : hRaw;
        vTarget = Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1) ? vRaw * 2.0f : vRaw;

        h = Mathf.Lerp(h, hTarget, Time.deltaTime * transitionSpeed);
        v = Mathf.Lerp(v, vTarget, Time.deltaTime * transitionSpeed);

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

        Vector3 moveDir = transform.forward * vRaw + transform.right * hRaw;

        if (controller.isGrounded)
        {
            verticalVelocity = -0.5f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        float tmp = Input.GetKey(KeyCode.LeftShift) ? speed * 2.4f : speed;

        Vector3 velocity = moveDir * tmp;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        wasAiming = Input.GetMouseButton(1);
    }
}