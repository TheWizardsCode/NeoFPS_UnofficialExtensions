#if NODE_CANVAS_PRESENT
using NeoFPS;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;
using WizardsCode.AI.Animation;

namespace WizardsCode.AI.BehaviourTree
{

    [Category("NeoFPS")]
    [Description("Attack a character, with the currently equipped weapon if within range.")]
    public class Attack : ActionTask<AiBaseCharacter>
    {
        [Tooltip("The target that the agent is to attack.")]
        public BBParameter<ICharacter> target;
        [Tooltip("Should the agent stop moving in order to attack?")]
        public BBParameter<bool> stop = true;
        [Tooltip("The speed of the attack.")]
        public BBParameter<float> attackSpeed = 1;

        [Header("Editor Only")]
        [Tooltip("Set to true to display debug information in the editor.")]
        public bool m_IsDebug = true;

        private Animator m_Animator;
        float originalAnimatorSpeed;
        private IAiWeapon m_Weapon;

        protected override string info
        {
            get
            {
                return string.Format("Attack {0}\n(currently {1} at a distance of {2})", target, target.value != null ? target.value.gameObject.name : "None", distance);
            }
        }

        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit()
        {
            m_NavMeshAgent = agent.GetComponent<NavMeshAgent>();
            m_Animator = agent.GetComponent<Animator>();
            originalAnimatorSpeed = m_Animator.speed;

            return null;
        }

        //This is called once each time the task is enabled.
        //Call EndAction() to mark the action as finished, either in success or failure.
        //EndAction can be called from anywhere.
        protected override void OnExecute()
        {
            if (m_CooldownCompleteTime > Time.timeSinceLevelLoad) {
                EndAction(false);
                return;
            }
            if (stop.value && m_NavMeshAgent != null)
            {
                m_NavMeshAgent.isStopped = true;
            }

            IQuickSlotItem selected = agent.quickSlots.selected;
            m_Weapon = selected.GetComponent<IAiWeapon>();

            originalAnimatorSpeed = m_Animator.speed;
            m_Animator.speed = attackSpeed.value;
            m_Animator.SetTrigger(AnimParams.ATTACK);
            m_CooldownCompleteTime = Time.timeSinceLevelLoad + m_Weapon.recoveryTime;
        }

        private float m_CooldownCompleteTime;
        private NavMeshAgent m_NavMeshAgent;
        private float distance;

        //Called once per frame while the action is active.
        protected override void OnUpdate()
        {
            if (m_CooldownCompleteTime <= Time.timeSinceLevelLoad)
            {
                EndAction(true);
                return;
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            m_Animator.speed = originalAnimatorSpeed;
        }
    }
}
#endif