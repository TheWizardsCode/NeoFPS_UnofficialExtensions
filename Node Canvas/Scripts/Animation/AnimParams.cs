using UnityEngine;

namespace WizardsCode.AI.Animation
{
    /// <summary>
    /// A collection of constants representing animatorcontroler states
    /// </summary>
    public class AnimParams
    {
        public static readonly int UPPER_BODY_LAYER = 2;
        public static readonly string UPPER_BODY_INACTIVE_STATE_NAME = "EmptyState";

        // Combat
        public static readonly int ATTACK = Animator.StringToHash("Attack");
        public static readonly int DRAW_WEAPON = Animator.StringToHash("PullOutWeapon");
        public static readonly int THROW = Animator.StringToHash("Throw");
        public static readonly int DIE = Animator.StringToHash("Die");
        public static readonly int GET_HIT = Animator.StringToHash("GetHit");

        // Move
        public static readonly int IDLE = Animator.StringToHash("Idle");
        public static readonly int SPEED = Animator.StringToHash("Speed");
        public static readonly int TURN = Animator.StringToHash("Turn");

        // Actions        
        public static readonly int EAT    = Animator.StringToHash("Eat");
        public static readonly int WARN   = Animator.StringToHash("Warn");
        
        // Items
        public static readonly int IS_ONE_HANDED = Animator.StringToHash("IsOneHanded");
        public static readonly int EQUIPPED_ITEM = Animator.StringToHash("EquippedItem");
    }
}