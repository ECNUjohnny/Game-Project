using System.Collections;
using System.Collections.Generic;
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
    }
}
