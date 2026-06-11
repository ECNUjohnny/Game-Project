using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoReturnEmptyAnimator : StateMachineBehaviour
{
    [Header("Time to wait")]

    public float time2Wait = 4.0f;

    private float time = 0;

    private PlayerCombat playerCombat;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        time = 0f;
        
        if (playerCombat == null)
        {
            playerCombat = animator.GetComponent<PlayerCombat>();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        time += Time.deltaTime;

        if (time >= time2Wait && playerCombat != null)
        {
            
        }
    }
}
