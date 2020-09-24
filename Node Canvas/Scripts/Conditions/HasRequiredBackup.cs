#if NODE_CANVAS_PRESENT
using NeoFPS;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace WizardsCode.NeoFPS.BehaviourTree{

	[Category("NeoFPS")]
	[Description("Does the agent believe that if they attack there are enough friendlies that will attack with them?")]
	public class HasRequiredBackup : ConditionTask<AiBaseCharacter>
	{
		[Tooltip("The target that the agent is to attack.")]
		public BBParameter<ICharacter> target;
		[Tooltip("The number of friendly characters that will attack with this agent should they decide to attack.")]
		public BBParameter<int> m_RequiredBackupCharacters = 2;

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
			// check for agents within a distance of the target
			float range = 10;
			int maxAgents = m_RequiredBackupCharacters.value * 4; // there may be many dead around
			Collider[] agentColliders = new Collider[maxAgents];
			int numOfAgents = Physics.OverlapSphereNonAlloc(agent.transform.position, range, agentColliders, PhysicsFilter.LayerFilter.CharacterControllers);

			int backupCount = 0;
			for (int i = 0; i < numOfAgents; i++)
            {
				AiBaseCharacter ai = agentColliders[i].GetComponent<AiBaseCharacter>();
				if (ai != null && ai.isAlive) backupCount++;
            }

			if (backupCount > m_RequiredBackupCharacters.value)
			{
				return true;
			} else
            {
				return false;
            }
		}
	}
}
#endif