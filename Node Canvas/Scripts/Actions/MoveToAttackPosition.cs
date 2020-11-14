#if NODE_CANVAS_PRESENT
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace WizardsCode.AI.BehaviourTree 
{

	[Category("NeoFPS")]
	[Description("Find an optimal position from which to attack a target and move towards it. If the agent is already in a good position to attack then no movement is made and the action fails.")]
	public class MoveToAttackPosition : ActionTask<AiBaseCharacter>
	{
		[BlackboardOnly, RequiredField, Tooltip("The target the agent wants to attack.")]
		public BBParameter<Transform> target;
		[Tooltip("Maximum duration that an AI will attempt to reach a point before aborting and trying to find another, better location.")]
		public BBParameter<float> maxDuration = 2;

		[Space]
		[Header("Editor Only")]
		[Tooltip("Set to true to display debug information in the editor.")]
		public bool m_IsDebug = true;

		AiBaseInventory m_Inventory;
		AiBaseWeapon m_Weapon;
		NavMeshAgent m_NavMeshAgent;

		float m_TimeToChooseAnotherPosition; 

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit()
		{
			m_Inventory = agent.GetComponent<AiBaseInventory>();
			m_NavMeshAgent = agent.GetComponent<NavMeshAgent>();
			return null;
		}

		protected override void OnExecute()
        {
            m_Weapon = m_Inventory.selected.GetComponent<AiBaseWeapon>();
            if (m_Weapon == null)
            {
#if UNITY_EDITOR
                if (m_IsDebug)
                    Debug.LogError("No weapon is equipped so cannot find an optimal attack position, support for fists is not present at this time.");
#endif

                EndAction(false);
                return;
            }

			float distanceToTarget = Vector3.Distance(agent.transform.position, target.value.position);
			if (distanceToTarget > m_Weapon.maximumRange && distanceToTarget < m_Weapon.minimumRange)
            {
#if UNITY_EDITOR
                if (m_IsDebug)
                    Debug.Log("Already within attack range so not moving again.");
#endif

                EndAction(false);
                return;
            }

            SetTargetPosition();
		}

        private void SetTargetPosition()
        {
			// TODO not all types of agent should come directly to the player, some might go via cover for example
            // TODO optimal attack position should rarely be directly in front of the target
            Vector3 m_AttackPosition = target.value.position + (m_Weapon.optimalRange  * target.value.forward);

#if UNITY_EDITOR
            if (m_IsDebug)
                Debug.Log("Chosen an attack position " + m_Weapon.optimalRange + " from the target.");
#endif

            NavMeshHit hit;
            if (NavMesh.SamplePosition(m_AttackPosition, out hit, 0.5f, NavMesh.AllAreas))
			{
				m_NavMeshAgent.isStopped = false;
				m_NavMeshAgent.SetDestination(hit.position);
                m_NavMeshAgent.speed = agent.runningSpeed;
                m_TimeToChooseAnotherPosition = Time.time + maxDuration.value;
#if UNITY_EDITOR
                if (m_IsDebug)
                    Debug.Log("Set a position on the NavMesh which is " + Vector3.Distance(m_AttackPosition, hit.position) + " from the chosen position");
#endif
            }
            else
            {
#if UNITY_EDITOR
                if (m_IsDebug)
                    Debug.Log("Unable to find a position on the NavMesh at the chosen position.");
#endif
                EndAction(false);
                return;
            }
        }

        protected override void OnUpdate()
		{
			if (Time.time > m_TimeToChooseAnotherPosition)
            {
#if UNITY_EDITOR
				if (m_IsDebug)
					Debug.Log("Choosing a new attack position, the current one is too old.");
#endif
				SetTargetPosition();
            }

			if (!m_NavMeshAgent.pathPending && m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
			{
#if UNITY_EDITOR
				if (m_IsDebug)
					Debug.Log("We are within the stopping distance, returning success");
#endif
				EndAction(true);
			}
		}

		protected override void OnPause()
		{
			OnStop();
		}
		protected override void OnStop()
		{
			if (agent != null && agent.gameObject.activeSelf)
			{
				m_NavMeshAgent.ResetPath();
			}
		}
	}
}
#endif
