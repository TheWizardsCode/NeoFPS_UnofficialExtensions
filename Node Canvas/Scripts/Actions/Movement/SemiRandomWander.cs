using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace WizardsCode.AI.NodeCanvas{

	[Category("WizardsCode/Movement/Pathfinding")]
	[Description("Wander byt moving towards a target and then continues heading in roughly the same direction, " +
		"within defined constraints (max deviation, range etc.), when the edge of the range is reached turn around.")]
	public class SemiRandomWander : ActionTask<UnityEngine.AI.NavMeshAgent>
	{
		[Tooltip("How close does the agent need to be to a target to be deemed to have reached that target.")]
		public BBParameter<float> minimumReachDistance = 0.25f;
		[Tooltip("Unless interupted, or the target is reached, what is the minimum time the agent stick with its current target? " +
			"When a new targt is selected a duration between this and the max time is selected. " +
			"Once this time has elepased a new target will be selected.")]
		public BBParameter<float> minTimeBetweenRandomPathChanges = 2f;
		[Tooltip("Unless interupted, or the target is reached, what is the maximum time the agent stick with its current target? " +
			"When a new targt is selected a duration between this and the min time is selected. " +
			"Once this time has elepased a new target will be selected.")]
		public BBParameter<float> maxTimeBetweenRandomPathChanges = 5f;
		[Tooltip("What is the minimum distance the agent should be prepared to wander, time allowing? " +
			"When a new targt is selected a target at a distance between this and the max distance is selected. ")]
		public BBParameter<float> minDistanceOfRandomPathChange = 10f;
		[Tooltip("What is the maximum distance the agent should be prepared to wander, time allowing? " +
			"When a new targt is selected a target at a distance between this and the min distance is selected. ")]
		public BBParameter<float> maxDistanceOfRandomPathChange = 20f;
		[Tooltip("What is the minimum angle from the current path the agent should be prepared to wander? " +
			"When a new targt is selected a target at an angle between this and the max angle is selected. ")]
		public BBParameter<float> minAngleOfRandomPathChange = -30;
		[Tooltip("What is the minimum angle from the current path the agent should be prepared to wander? " +
			"When a new targt is selected a target at an angle between this and the max angle is selected. ")]
		public BBParameter<float> maxAngleOfRandomPathChange = 30;
		[Tooltip("How far from the Home position (the starting position of this agent will the agent be " +
			"allowed to wander? If a destination cannot be found within this range the AI will turn around " +
			"and head back towards home.")]
		public BBParameter<float> maxRange = 100;

		private Vector3 homePosition;
		private float sqrMagnitudeRange;
		private float timeToNextWanderPathChange = 0;

		/// <summary>
		/// Test to see if this agent has reached their current target yet.
		/// </summary>
		public bool HasReachedTarget
		{
			get
			{
				if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit(){
			homePosition = agent.transform.position;
			return null;
		}

		//This is called once each time the task is enabled.
		//Call EndAction() to mark the action as finished, either in success or failure.
		//EndAction can be called from anywhere.
		protected override void OnExecute() {
			if (!HasReachedTarget)
            {
				EndAction(true);
			}

			timeToNextWanderPathChange -= Time.deltaTime;
			if (timeToNextWanderPathChange < 0)
			{
				sqrMagnitudeRange = maxRange.value * maxRange.value;
				Vector3 position = GetValidWanderPosition(agent.transform, 0);
				agent.SetDestination(position);
				agent.isStopped = false;

				timeToNextWanderPathChange = Random.Range(minTimeBetweenRandomPathChanges.value, maxTimeBetweenRandomPathChanges.value);
			}
			EndAction(true);
		}

		private Vector3 GetValidWanderPosition(Transform transform, int attemptCount)
		{
			int maxAttempts = 6;
			bool turnAround = false;

			attemptCount++;
			if (attemptCount > maxAttempts / 2)
			{
				turnAround = true;
			}
			else if (attemptCount > maxAttempts)
			{
				return homePosition;
			}
			
			Vector3 position;
			float minDistance = minDistanceOfRandomPathChange.value;
			float maxDistance = maxDistanceOfRandomPathChange.value;

			Quaternion randAng;
			if (!turnAround)
			{
				randAng = Quaternion.Euler(0, Random.Range(minAngleOfRandomPathChange.value, maxAngleOfRandomPathChange.value), 0);
			}
			else
			{
				randAng = Quaternion.Euler(0, Random.Range(180 - minAngleOfRandomPathChange.value, 180 + maxAngleOfRandomPathChange.value), 0);
				minDistance = maxDistance;
			}
			position = transform.position + randAng * transform.forward * Random.Range(minDistance, maxDistance);

			if (UnityEngine.Terrain.activeTerrain != null)
			{
				position.y = UnityEngine.Terrain.activeTerrain.SampleHeight(position);
			}

			NavMeshHit hit;
			if (NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas))
			{
				position = hit.position;
			} else
            {
				GetValidWanderPosition(transform, attemptCount);
			}

			if (Vector3.SqrMagnitude(homePosition - position) > sqrMagnitudeRange)
            {
				return homePosition;
			}
			return position;
		}

		//Called once per frame while the action is active.
		protected override void OnUpdate(){
			
		}

		protected override void OnPause() { OnStop(); }
		protected override void OnStop()
		{
		}

		public override void OnDrawGizmosSelected()
		{
			base.OnDrawGizmosSelected();
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(agent.destination, 0.5f);
			Gizmos.DrawWireSphere(homePosition, maxRange.value);
		}
	}
}