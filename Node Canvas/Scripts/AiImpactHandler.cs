#if NODE_CANVAS_PRESENT
using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.NeoFPS.BehaviourTree;

namespace WizardsCode.NeoFPS
{
    public class AiImpactHandler : MonoBehaviour, IImpactHandler
    {
        [SerializeField, Tooltip("Use this to exagerate the effects of force being handled.")]
        private float m_ForceMultiplier = 25f;
        [SerializeField, Tooltip("Use this to limit the forces applied.")]
        private float m_MaxForce = 2000f;

        Rigidbody m_Rigidbody = null;

        void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        public void HandlePointImpact(Vector3 position, Vector3 force)
        {
            if (m_Rigidbody != null)
            {
                m_Rigidbody.AddForce(Vector3.ClampMagnitude(force * m_ForceMultiplier, m_MaxForce), ForceMode.Force);
            }
        }
    }
}
#endif