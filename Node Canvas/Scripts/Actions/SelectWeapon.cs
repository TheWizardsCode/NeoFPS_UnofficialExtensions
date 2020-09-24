#if NODE_CANVAS_PRESENT
using NeoFPS;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace WizardsCode.NeoFPS.BehaviourTree
{

	[Category("NeoFPS")]
	[Description("Given the characters proximity and position relative to the target select the best weapon. Returns the optimal position, relative to the targets forward location to attack from.")]
	public class SelectWeapon : ActionTask<FpsInventoryBase>
    {
        [BlackboardOnly, RequiredField, Tooltip("The target the agent wants to attack.")]
        public BBParameter<Transform> target;
		[BlackboardOnly, RequiredField, Tooltip("The current distance to the target.")]
		public BBParameter<float> distanceToTarget;

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit(){
			return null;
		}

		//This is called once each time the task is enabled.
		//Call EndAction() to mark the action as finished, either in success or failure.
		//EndAction can be called from anywhere.
		protected override void OnExecute()
        {
            // TODO cache the inventory items and subscribe to changes
            // TODO don't consider changing weapon every cycle, only every x seconds
            int optimalSlot = -1;
            AiBaseWeapon optimalWeapon = null;
            float currentDeviationFromMidPoint = float.PositiveInfinity;
            for (int i = 0; i < agent.numSlots; i++)
            {
                IQuickSlotItem item = agent.GetSlotItem(i);
                if (item == null) continue;

                AiBaseWeapon weapon = item.GetComponent<AiBaseWeapon>();
                if (optimalWeapon == null || (distanceToTarget.value >= weapon.minimumRange && distanceToTarget.value <= weapon.maximumRange))
                {
                    // TODO add a preference for higher damage weapons
                    float deviation = Mathf.Abs(distanceToTarget.value - weapon.optimalRange);
                    if (deviation < currentDeviationFromMidPoint)
                    {
                        // TODO add a penalty for changing weapons (that is only chang weapons when it is truly worthwhile)
                        currentDeviationFromMidPoint = deviation;
                        optimalSlot = i;
                        optimalWeapon = weapon;
                    }
                }
            }

            if (optimalWeapon != agent.selected.GetComponent<AiBaseWeapon>())
            {
                agent.SelectSlot(optimalSlot);
            } 

            EndAction(true);
        }

        //Called once per frame while the action is active.
        protected override void OnUpdate(){
			
		}

		//Called when the task is disabled.
		protected override void OnStop(){
			
		}

		//Called when the task is paused.
		protected override void OnPause(){
			
		}
	}
}
#endif