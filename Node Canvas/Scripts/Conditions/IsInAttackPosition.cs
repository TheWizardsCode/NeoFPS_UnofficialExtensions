#if NODE_CANVAS_PRESENT
using NeoFPS;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace WizardsCode.NeoFPS.BehaviourTree
{

	[Category("NeoFPS")]
	[Description("Test if this agent is in a good position to attack with the currently equipped weapon.")]
	public class IsInAttackPosition : ConditionTask<AiBaseCharacter>
	{
		[Tooltip("The target that the agent is to attack.")]
		public BBParameter<ICharacter> target;

		private float distance;
        private IAiWeapon m_Weapon;

        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit(){
			return null;
		}

		//Called whenever the condition gets enabled.
		protected override void OnEnable(){
			
		}

		//Called whenever the condition gets disabled.
		protected override void OnDisable(){
			
		}

		//Called once per frame while the condition is active.
		//Return whether the condition is success or failure.
		protected override bool OnCheck(){
			IQuickSlotItem selected = agent.quickSlots.selected;
			m_Weapon = selected.GetComponent<IAiWeapon>();

			bool isReady = HasWeaponEquipped();
			isReady &= InSuitablePosition();

			return isReady;
		}

		private bool InSuitablePosition()
		{
			Transform t = target.value.transform;

			distance = Vector3.Distance(agent.transform.position, t.position);
			if (distance > m_Weapon.maximumRange || distance < m_Weapon.minimumRange)
			{
				return false;
			}

			float angle = Vector3.Angle(t.position - agent.transform.position, agent.transform.forward);
			if (angle > m_Weapon.maximumAttackAngle)
			{
				return false;
			}

			return true;
		}

		private bool HasWeaponEquipped()
		{
			// TODO handle situation when character does not have a weapon equipped
			if (m_Weapon == null)
			{
				return false;
			}

			return true;
		}
	}
}
#endif