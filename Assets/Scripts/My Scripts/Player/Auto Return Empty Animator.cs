using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoReturnEmptyAnimator : StateMachineBehaviour
{
    private static readonly int TDrawHash = Animator.StringToHash("tDraw");
    private static readonly int TReturnEmptyHash = Animator.StringToHash("tReturnEmpty");
    [Header("Time to wait")]

    public float time2Wait = 4.0f;

    private float time = 0;

    private PlayerCombat playerCombat;

    private bool hasTriggered = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        time = 0f;
        hasTriggered = false;
        
        if (playerCombat == null)
        {
            playerCombat = animator.GetComponent<PlayerCombat>();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hasTriggered) return;
        
        if (playerCombat.bAiming || playerCombat.bShooting)
        {
            time = 0f;
            return;
        }
        
        
        time += Time.deltaTime;

        if (time >= time2Wait && playerCombat != null)
        {
            animator.ResetTrigger(TDrawHash);
            
            animator.SetTrigger(TReturnEmptyHash);

            hasTriggered = true;
        }
    }
}
