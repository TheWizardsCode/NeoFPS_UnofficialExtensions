﻿using System;
using UnityEngine;
using UnityEngine.AI;

namespace WizardsCode.NeoFPS.Animation
{
    /// <summary>
    /// Converts NavMesh movement to animation controller parameters.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(LookAt))]
    public class SimpleLocomotionController : MonoBehaviour
    {
        [Header("Character Setup")]
        [SerializeField, Tooltip("The maximum speed of this character, this will usually be a full sprint.")]
        private float m_MaxSpeed = 8f;
        [SerializeField, Tooltip("The factor used to calculate the normal (usually walking) speed of the character relative to the maximum speed. Normally you won't want to change this, but if your character is sliding when walking this can help.")]
        float m_NormalSpeedFactor = 0.4375f;

        [Header("Animation Parameters")]
        [SerializeField, Tooltip("The normalized speed of the character. This does not take into account the direction of travel.")]
        private string m_SpeedParameterName = "Speed";
        [SerializeField, Tooltip("The normalized turning speed (-1 to 1) of the character (-1 to 1).")]
        private string m_TurnParameterName = "Turn";

        [Header("Debug")]
        [SerializeField, Tooltip("Show gizmos for this connector when the agent is selected.")]
        private bool m_ShowGizmosWhenSelected = true;
        [SerializeField, Tooltip("Enable click to move when running in the editor (ignored in the application).")]
        private bool m_EnableClickToMove = true;

        float m_NormalSpeed; // typically this will be the walking speed


        RaycastHit hitInfo = new RaycastHit();
        Animator m_Animator;
        NavMeshAgent m_Agent;
        private LookAt lookAt;

        void Start()
        {
            m_NormalSpeed = m_MaxSpeed * m_NormalSpeedFactor;
            m_NormalSpeed += 0.01f; // increase the walking speed a little to avoid rounding errors affecting comparisons

            m_Animator = GetComponent<Animator>();
            m_Agent = GetComponent<NavMeshAgent>();
            lookAt = GetComponent<LookAt>();
        }

        void Update()
        {
#if UNITY_EDITOR
            if (m_EnableClickToMove && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray.origin, ray.direction, out hitInfo))
                    m_Agent.destination = hitInfo.point;
            }
#endif

            float magVelocity = m_Agent.velocity.magnitude;
            float animSpeed = 1;
            float speedParam = 0;
            if (!Mathf.Approximately(magVelocity, 0))
            {
                if (magVelocity <= m_NormalSpeed)
                {
                    speedParam = magVelocity / (m_NormalSpeed + m_MaxSpeed);
                    animSpeed = magVelocity / m_NormalSpeed;
                }
                else
                {
                    speedParam = magVelocity / m_MaxSpeed;
                    animSpeed = speedParam;
                }
            }

            Vector3 s = m_Agent.transform.InverseTransformDirection(m_Agent.velocity).normalized;
            float turn = s.x;

            m_Animator.SetFloat(m_SpeedParameterName, speedParam);
            m_Animator.speed = Math.Abs(animSpeed);
            m_Animator.SetFloat(m_TurnParameterName, turn);

            if (lookAt != null)
            {
                lookAt.LookAtPosition(m_Agent.destination);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (m_ShowGizmosWhenSelected && m_Agent != null)
            {
                Gizmos.DrawSphere(m_Agent.destination, .25f);
            }
        }
    }
}
