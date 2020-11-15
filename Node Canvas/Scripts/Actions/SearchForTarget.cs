#if NODE_CANVAS_PRESENT
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace WizardsCode.AI.BehaviourTree 
{
	[Category("NeoFPS")]
	[Description("Conduct a search for a target known to be in the area. They will search for a defined duration and return success at the end of the search. If the character goes beyond a given distance then return failure.")]
	public class SearchForTarget : ActionTask<AiBaseCharacter>
	{
		[Tooltip("The current POI is the source of the trigger that makes the agent want to search an area.")]
		public BBParameter<Transform> currentPOI;
		[Tooltip("If the agent goes outside this range the search will be called off.")]
		public BBParameter<float> searchDistance = 20;
		[Tooltip("If the agent comes within this distance they will instantly become aware of the character and return success.")]
		public BBParameter<float> awarenessDistance = 7;
		[Tooltip("The speed at which the character should move while searching.")]
		public BBParameter<float> speed = 2;
		[Tooltip("Maximum duration of search.")]
		public BBParameter<float> maxDuration = 2;
		[Tooltip("The distance to keep between the agent and the target.")]
		public BBParameter<float> keepDistance = 5;
		[Tooltip("How certain the agent is of the location of their target, represented by the CurrentPOI. The more certain the more direct the agent will be in their search."), Range(0, 1)]
		public BBParameter<float> locationCertainty = 0.4f;

		[Space]
		[Header("Debug")]
		[Tooltip("Set to true to display debug information in the editor.")]
		public bool m_IsDebug = true;

		private NavMeshAgent m_NavMeshAgent;
		private float m_EndSearchTime;
		public Vector3 currentSearchPosition;
        private Transform lastPosition;

        protected override string info
		{
			get
			{
				string location = currentPOI.value != null ? currentPOI.value.gameObject.name : "TBC";
				return "Search for target at " + location;
			}
		}

        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit(){
			m_NavMeshAgent = agent.GetComponent<NavMeshAgent>();
			if (m_NavMeshAgent == null) return "A NavMeshAgent is required on " + agent;

			return null;
		}

		//This is called once each time the task is enabled.
		//Call EndAction() to mark the action as finished, either in success or failure.
		//EndAction can be called from anywhere.
		protected override void OnExecute(){
			if (currentPOI.value == null)
			{
				EndAction(false);
				return;
			}

			if (currentPOI.value != lastPosition)
            {
				lastPosition = currentPOI.value;
				currentSearchPosition = currentPOI.value.position;
				PerformSearchStep();
			} else if (m_NavMeshAgent.remainingDistance < 0.25)
			{
				PerformSearchStep();
			}
		}

		//Called once per frame while the action is active.
		protected override void OnUpdate(){
			// Been searching too long, call it off
			if (Time.time > m_EndSearchTime)
			{
				m_NavMeshAgent.ResetPath();
				currentPOI.value = null;
				lastPosition = null;
				EndAction(true);
				return;
			}

			if (currentPOI.value == null)
			{
				m_NavMeshAgent.isStopped = true;
				EndAction(false);
				return; 
            }

			// Within awareness distance, no need to search anymore
			if (Vector3.Distance(agent.transform.position, currentPOI.value.position) <= awarenessDistance.value)
			{
				m_NavMeshAgent.isStopped = true;
				EndAction(true);
				return;
			}

			// Already got a destination, working out the path
			if (m_NavMeshAgent.pathPending)
			{
				return;
			}

			// We've got close enough now, stop advancing
			if (m_NavMeshAgent.remainingDistance <= keepDistance.value)
			{
				m_NavMeshAgent.isStopped = true;
				EndAction(true);
				return;
			}
		}

		/// <summary>
		/// Pick a location near the CurrentPOI. The more certain the agent is
		/// of the accuracy of their POI location the less randomness there is
		/// in the point chosen.
		/// </summary>
        private void PerformSearchStep()
        {
#if UNITY_EDITOR
			if (locationCertainty.value < 0)
            {
				Debug.LogWarning("Location Awareness cannot be < 0");
				locationCertainty.value = 0;
			}
			else
			if (locationCertainty.value > 1)
			{
				Debug.LogWarning("Location Awareness cannot be > 1");
				locationCertainty.value = 1;
			}
#endif
			float searchAreaRadius = 5 * locationCertainty.value;

			var goalPos = currentSearchPosition;
			goalPos.x += Random.value * searchAreaRadius;
			goalPos.z += Random.value * searchAreaRadius;

			NavMeshHit hit;
			if (NavMesh.SamplePosition(goalPos, out hit, 3, NavMesh.AllAreas))
			{
				m_NavMeshAgent.SetDestination(hit.position);
				m_NavMeshAgent.isStopped = false;
				m_EndSearchTime = Time.time + maxDuration.value;
			}
		}

		protected override void OnPause() { OnStop(); }
		protected override void OnStop()
		{
			if (m_NavMeshAgent != null && m_NavMeshAgent.gameObject.activeSelf && m_NavMeshAgent.isOnNavMesh)
			{
				m_NavMeshAgent.ResetPath();
			}
		}
	}
}
#endif