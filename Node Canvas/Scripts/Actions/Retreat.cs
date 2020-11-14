#if NODE_CANVAS_PRESENT
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;
using WizardsCode.AI;

namespace WizardsCode.AI{

	[Category("NeoFPS")]
	[Description("Agent will retreat away from the target trying to seek safety.")]
	public class Retreat : ActionTask<AiBaseCharacter>{
		[Tooltip("The minimum distance the agent should retreat.")]
		public BBParameter<float> m_MinDistance = 4f;
		[Tooltip("The maximum distance the agent should retreat.")]
		public BBParameter<float> m_MaxDistance = 8f;

		private NavMeshAgent m_NavMeshAgent;
        private bool originalUpdateRotation;

        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit(){
			m_NavMeshAgent = agent.GetComponent<NavMeshAgent>();
            originalUpdateRotation = m_NavMeshAgent.updateRotation;
            return null;
		}

		//This is called once each time the task is enabled.
		//Call EndAction() to mark the action as finished, either in success or failure.
		//EndAction can be called from anywhere.
		protected override void OnExecute() {
			Vector3 pos = agent.transform.position;
			pos += agent.transform.forward * -Random.Range(m_MinDistance.value, m_MaxDistance.value);

			NavMeshHit hit;
			if (NavMesh.SamplePosition(pos, out hit, 3, NavMesh.AllAreas))
			{
				m_NavMeshAgent.updateRotation = false;
				m_NavMeshAgent.SetDestination(hit.position);	
			}
		}

		//Called once per frame while the action is active.
		protected override void OnUpdate(){
			if (m_NavMeshAgent.remainingDistance < m_NavMeshAgent.stoppingDistance)
            {
				EndAction(true);
            }
		}

		//Called when the task is disabled.
		protected override void OnStop(){
			m_NavMeshAgent.updateRotation = originalUpdateRotation;
		}

		//Called when the task is paused.
		protected override void OnPause(){
			
		}
	}
}
#endif