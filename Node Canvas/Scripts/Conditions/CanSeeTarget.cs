#if NODE_CANVAS_PRESENT
using NeoFPS;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace WizardsCode.AI.BehaviourTree
{

	[Category("NeoFPS")]
	[Description("Tests to see if the agent can see a target.")]
	public class CanSeeTarget : ConditionTask<AiBaseCharacter>
    {
        [BlackboardOnly, RequiredField, Tooltip("The target the agent is trying to see.")]
        public BBParameter<Transform> target;
        [Tooltip("The maximum distance that the agent can see in normal conditions.")]
        public BBParameter<float> maxDistance = 50;
        [SliderField(1, 180), Tooltip("The field of view for each of the agents in normal conditions.")]
        public BBParameter<float> fieldOfView = 70f;
        [Tooltip("The name of a child transform representing the position and rotation of the eyes, if null the agent position will be used.")]
        public string eyes = "EyesRaycastPoint";
        [Tooltip("The layers that we should search within to find the target.")]
        public LayerMask layerMask;

        [Space]
        [Header("Editor Only")]
        [Tooltip("Set to true to display debug information in the editor.")]
        public bool m_IsDebug = true;

        private RaycastHit hit;
        private Transform eyesPosition;
        private Transform lastTarget;
        private IDamageHandler[] damageHandlers;

        protected override string info
        {
            get { return "Can See " + target; }
        }

        protected override string OnInit()
        {
            string result = base.OnInit();
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            eyesPosition = RecursiveFind(agent.transform, eyes);
            if (eyesPosition == null)
            {
                Debug.LogWarning("Didn't find an eyes transform with the name " + eyes + " for the CanSeeTarget condition, using the agents transform. This can cause problems in some setups.");
                eyesPosition = agent.transform;
            }
            return null;
        }

        Transform RecursiveFind(Transform parent, string name)
        {
            Transform child = null;
            for (int i = 0; i < parent.childCount; i++)
            {
                child = parent.GetChild(i);
                if (child.name == name)
                {
                    break;
                }
                else
                {
                    child = RecursiveFind(child, name);
                    if (child != null)
                    {
                        break;
                    }
                }
            }

            return child;
        }

        protected override bool OnCheck()
        {
            if (target.value == null) return false;
            if (target.value != lastTarget) {
                lastTarget = target.value;
                damageHandlers = lastTarget.GetComponentsInChildren<IDamageHandler>();
            }

            // TODO: curently we "see" on the first collider that matches, perhaps increase chance of sight for each collider
            bool canSee = false;
            for (int i = 0; i < damageHandlers.Length; i++)
            {
                Transform sightTarget = ((BasicDamageHandler)damageHandlers[i]).transform;
#if UNITY_EDITOR
                if (m_IsDebug)
                {
                    Debug.DrawRay(eyesPosition.position, sightTarget.position - eyesPosition.position, Color.white);
                }
#endif

                // TODO is it more efficient to do the raycast first?
                if (Vector3.Angle(sightTarget.position - eyesPosition.position, eyesPosition.forward) > fieldOfView.value)
                {
#if UNITY_EDITOR
                    if (m_IsDebug) Debug.Log("Target is outside the field of view of the agent.");
#endif
                    return false;
                }

                // TODO need to look for specific parts of the target rather than just 1 m above transform
                Vector3 targetPos = sightTarget.position;
                targetPos.y += 1;
                Ray ray = new Ray(eyesPosition.position, sightTarget.position - eyesPosition.position);
                if (Physics.Raycast(ray, out hit, maxDistance.value, layerMask))
                {
                    canSee = hit.collider.transform.root == sightTarget.transform.root;
                    if (canSee)
                    {
                        break;
                    }
                }
            }
            return canSee;
        }

        public override void OnDrawGizmosSelected()
        {
            if (agent != null)
            {
                Gizmos.DrawLine(agent.transform.position, eyesPosition.position);
                Gizmos.DrawLine(agent.transform.position, eyesPosition.position + (eyesPosition.forward * maxDistance.value));
                Gizmos.DrawWireSphere(eyesPosition.position + (eyesPosition.forward * maxDistance.value), 0.1f);
                Gizmos.matrix = Matrix4x4.TRS(eyesPosition.position, eyesPosition.rotation, Vector3.one);
                Gizmos.DrawFrustum(Vector3.zero, fieldOfView.value, 5, 0, 1f);
            }
        }
    }
}
#endif