using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WizardsCode.AI
{
    /// <summary>
    /// An AI Group Controller manages an AI's group membership(s).
    /// Add this to an AI if you want it to form groups and act 
    /// together with that group.
    /// 
    /// </summary>
    public class AiGroupController : MonoBehaviour
    {

        [SerializeField, Tooltip("Maximum number of AIs in a group of this type, including the leader.")]
        private int m_MaxMembers = 4;
        
        [Tooltip("Radius within which an AI should attempt to detect other AIs that may join this group.")]
        private float m_DetectionRadius = 10;
        [SerializeField, Tooltip("That layers within which to look for group members.")]
        private LayerMask m_LayerMask;

        [Header("Flocking")]
        [SerializeField, Tooltip("The distance at which members of this group will start to take evasive action when flocking.")]
        public float AvoidanceDistance = 1;
        [SerializeField, Tooltip("The distance that the agent is willing to go from the optimal position in relation to the flock leader. Higher values will result in a more randomly dispersed flock.")]
        public float MaxWanderRadius = 10;
        [SerializeField, Tooltip("The degree to which the agent will deviate from the ideal flocking path.")]
        public float WanderJitter = 10;

        [HideInInspector, SerializeField]
        private List<AiGroupController> m_Members = new List<AiGroupController>(); // NOTE: this will be null if this is not the leader

        public bool IsLeader
        {
            get { return m_Members != null && m_Members.Count != 0 && Leader == this; }
        }

        public bool IsFull
        {
            get { return m_Members.Count >= m_MaxMembers; }
        }

        public AiGroupController Leader
        {
            get; set;
        }

        /// <summary>
        /// The current count of members in this group.
        /// </summary>
        public int Count
        {
            get { return m_Members.Count; }
        }

        public List<Transform> MemberTransforms {
            get {
                if (!IsLeader)
                {
                    return Leader.MemberTransforms;
                } else {
                    List<Transform> result = new List<Transform>();
                    for (int i = 0; i < m_Members.Count; i++) {
                        result.Add(m_Members[i].transform);
                    }
                    return result;
                }
            } 
        }

        private void Awake()
        {
            Leader = this;
            m_Members.Add(this);
        }

        private void Update()
        {
            if (!IsLeader || IsFull) return;

            FormGroup();
        }

        /// <summary>
        /// Look for other AI that are potential members of this AIs group.
        /// If any are found invite them into the group.
        /// </summary>
        private void FormGroup()
        {
#if UNITY_EDITOR
            if (!IsLeader)
            {
                Debug.LogWarning("OPTIMIZATION: we shouldn't be calling FormGroup if the character is not the leader.");
                return;
            }
            if (IsFull)
            {
                Debug.LogWarning("OPTIMIZATION: we shouldn't be calling FormGroup if the group is already full.");
                return;
            }
#endif

            Collider[] colliders = Physics.OverlapSphere(transform.position, m_DetectionRadius, m_LayerMask);
            for (int i = 0; i < colliders.Length && !IsFull; i++)
            {
                if (colliders[i].gameObject == gameObject) continue;

                AiGroupController other = colliders[i].GetComponentInParent<AiGroupController>();
                if (other == null || other.Count > Count) continue;

                if (m_Members.Contains(other)) continue;

                other.LeaveGroup();

                m_Members.Add(other);
                other.Leader = this;
            }
        }

        public void LeaveGroup()
        {
            Leader.Remove(this);
            Leader = null;
        }

        /// <summary>
        /// Remove a member fromt the group this AI is a member of.
        /// </summary>
        /// <param name="member"></param>
        public void Remove(AiGroupController member)
        {
            m_Members.Remove(member);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (IsLeader)
            {
                Gizmos.color = Color.green;
            } else
            {
                Gizmos.color = Color.white;
            }
            Gizmos.DrawSphere(transform.position, 0.5f);

            if (Leader != null)
            {
                for (int i = 0; i < Leader.m_Members.Count; i++)
                {
                    Gizmos.DrawLine(transform.position, Leader.m_Members[i].transform.position);
                }
            }
        }
#endif
    }
}
