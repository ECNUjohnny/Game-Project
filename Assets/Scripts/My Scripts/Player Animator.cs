using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimator : MonoBehaviour
{
    // Start is called before the first frame update
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
    void Start()
    {
        animator = GetComponent<Animator>();
        combatScript = GetComponent<PlayerCombat>();
        movementScript = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("InputX", movementScript.h);
        animator.SetFloat("InputY", movementScript.v);
        animator.SetBool("bAiming", combatScript.bAiming);
        animator.SetBool("bShooting", combatScript.bShooting);
        //transform.position += new Vector3(0.1f, 0, 0);
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
