#if NODE_CANVAS_PRESENT
using NeoFPS.SinglePlayer;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace WizardsCode.AI.BehaviourTree
{
	[Category("NeoFPS")]
	[Description("Find the closest player to the agent.")]
	public class FindClosestPlayer : ActionTask<AiBaseCharacter>{

		[BlackboardOnly]
		public BBParameter<Transform> saveObjectAs;
		[BlackboardOnly]
		public BBParameter<float> saveDistanceAs;

		protected override void OnExecute(){
            GameObject[] found = GameObject.FindGameObjectsWithTag("Player");
            if (found.Length == 0)
            {
                saveObjectAs.value = null;
                saveDistanceAs.value = 0;
                EndAction(false);
                return;
            }

            GameObject closest = null;
            var dist = Mathf.Infinity;
            for (int i = 0; i < found.Length; i++)
            {
                GameObject go = found[i];
                if (go.transform == agent)
                {
                    continue;
                }

                if (go.transform.IsChildOf(agent.transform))
                {
                    continue;
                }

                var newDist = Vector3.Distance(go.transform.position, agent.transform.position);
                if (newDist < dist)
                {
                    dist = newDist;
                    closest = go;
                }
            }

            saveObjectAs.value = closest.GetComponent<FpsSoloCharacter>().bodyTransformHandler.transform;
            saveDistanceAs.value = dist;
            EndAction();
        }
	}
}
#endif