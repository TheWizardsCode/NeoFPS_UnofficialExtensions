#if NODE_CANVAS_PRESENT
using NeoFPS;
using UnityEngine;


namespace WizardsCode.AI
{
    public class AiBaseWeapon : MonoBehaviour, IAiWeapon
    {
		[SerializeField, Tooltip("Is the weapon one handed, set to false if it is two handed.")]
		private bool m_IsOneHanded = true;
		[SerializeField, Tooltip("The damage the weapon does.")]
		private float m_Damage = 50f;
		[SerializeField, Tooltip("The force to impart on the hit object. Requires either an impact handler on the hit object.")]
		private float m_ImpactForce = 15f;
		[SerializeField, Tooltip("The minimum range that the melee weapon can reach.")]
		private float m_MinimumRange = 1f;
		[SerializeField, Tooltip("The maximum range that the melee weapon can reach.")]
		private float m_MaximumRange = 1.2f;
		[SerializeField, Tooltip("The maximum allowable angle between an agent and a target for this weapon to be used.")]
		private float m_MaximumAngle = 70f;
		[SerializeField, Tooltip("The recovery time required before the weapon can be used again.")]
		private float m_RecoveryTime = 1f;
		[SerializeField, Tooltip("A game object that represents the source and direction of a RayCast used to test if the AI hit something.")]
		internal Transform m_HitDetector;

        private float m_OptimalRange;

        public float minimumRange
        {
            get { return m_MinimumRange; }
            // If we ever add a setter must recalculate optimal range
        }

        public float maximumRange
        {
            get { return m_MaximumRange; }
			// If we ever add a setter must recalculate optimal range
		}

		public float maximumAttackAngle
		{
			get { return m_MaximumAngle; }
		}

		public float optimalRange
        {
            get { return m_OptimalRange; }
        }

        public float damageAmount
		{
			get { return m_Damage; }
		}

		public float impactForce
		{
			get { return m_ImpactForce; }
		}

		public float recoveryTime
		{
			get { return m_RecoveryTime; }
		}

		public Transform hitDetector
        {
			get { return m_HitDetector; }
        }

        private void Awake()
        {
            m_OptimalRange = (minimumRange + maximumRange) / 2;
        }

        public bool isOneHanded
        {
			get { return m_IsOneHanded; }
        }

#region Neo FPS IDamageSource
		public DamageFilter outDamageFilter
		{
			get { return DamageFilter.AllDamageAllTeams; }
			set { }
		}

		public IController controller
		{
			get { return null; }
		}

		public Transform damageSourceTransform
		{
			get { return transform; }
		}

		public string description
		{
			get { return name; }
		}
#endregion

    }
}
#endif