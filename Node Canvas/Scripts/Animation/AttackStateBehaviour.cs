#if NODE_CANVAS_PRESENT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.NeoFPS;

namespace WizardsCode.Animation
{
    /// <summary>
    /// Ensures that the speed of the animator is set to the
    /// correct speed for this characters current attack speed.
    /// This allows different characters to use the same
    /// animations at different speeds, or for a single character
    /// to get slower when damaged or similar.
    /// </summary>
    public class AttackStateBehaviour : StateMachineBehaviour
    {
        private float originalSpeed;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            AiBaseCharacter character = animator.GetComponent<AiBaseCharacter>();
            originalSpeed = animator.speed;
            animator.speed = character.attackSpeed;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            animator.speed = originalSpeed;
        }
    }
}
#endif
