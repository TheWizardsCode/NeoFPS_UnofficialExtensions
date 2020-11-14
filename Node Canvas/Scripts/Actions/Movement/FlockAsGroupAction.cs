#if NODE_CANVAS_PRESENT
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace WizardsCode.AI.NodeCanvas
{
    [Category("Wizards Code/Movement")]
    [Description("Flock around the leader of the group.")]
    [Name("Flock as Group")]
    public class FlockAsGroupAction : ActionTask<NavMeshAgent>
    {
        private Vector3 moveTarget;

        private NavMeshAgent prevAgent;
        private AiGroupController groupController;

        protected override string info
        {
            get { return "Flock with the group this agent belongs to."; }
        }

        protected override void OnExecute()
        {
            if (!GameObject.ReferenceEquals(prevAgent, agent))
            {

                prevAgent = agent;
                groupController = agent.GetComponent<AiGroupController>();
            }
        }

        protected override void OnUpdate()
        {
            if (groupController == null)
            {
                EndAction(false);
                return;
            }

            if (groupController.IsLeader)
            {
                EndAction(false);
                return;
            }

            ApplyFlockingRules();
            EndAction(true);
        }

        /// <summary>
        /// Set a new target based on the movement of the pack.
        /// </summary>
        /// <returns>True if a target was set, otherwise false.</returns>
        private bool ApplyFlockingRules()
        {
            List<Transform> _packMembers = groupController.MemberTransforms;

            Vector3 centerOfPack = Vector3.zero;
            Vector3 averageAvoidance = Vector3.zero;
            float packSpeed = 0;
            int packSize = 0;

            float neighborDistance = 0;
            foreach (var member in _packMembers)
            {
                // TODO: if the member has been killed it is not always removed from the pack correctly this guards against it
                if (member)
                {
                    if (member == agent.transform) continue;
                    neighborDistance = Vector3.Distance(member.position, agent.transform.position);
                    centerOfPack += member.transform.position;
                    packSize++;
                    if (neighborDistance < groupController.AvoidanceDistance)
                    {
                        averageAvoidance += (agent.transform.position - member.position);
                    }
                    NavMeshAgent memberAgent = member.GetComponent<NavMeshAgent>();
                    packSpeed += memberAgent.velocity.z;
                }
            }

            if (packSize == 0)
            {
                return false;
            }

            centerOfPack = centerOfPack / packSize;
            NavMeshAgent leaderNavMeshAgent = groupController.Leader.GetComponent<NavMeshAgent>();
            centerOfPack += (leaderNavMeshAgent.destination - agent.transform.position);

            moveTarget = (centerOfPack + averageAvoidance);

            // add some randomness
            Vector3 variance = new Vector3(Random.Range(-groupController.WanderJitter, groupController.WanderJitter), 0, Random.Range(-groupController.WanderJitter, groupController.WanderJitter));
            variance.Normalize();
            variance *= Random.Range(0, groupController.MaxWanderRadius);
            moveTarget += variance;

            agent.speed = leaderNavMeshAgent.speed * Random.Range(0.9f, 1.1f);
            agent.SetDestination(moveTarget);

            return true;
        }

        /// <summary>
        /// Set the destination to the nearest valid point on the NavMesh.
        /// If not valid point found near the target point return false;
        /// </summary>
        /// <param name="targetDestination">The point we want to move to</param>
        /// <returns>True if a valid on mesh point is found and set, otherwise false</returns>
        private bool SetDestination(Vector3 targetDestination)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetDestination, out hit, 1f, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
                return true;
            }
            return false;
        }
    }
}
#endif