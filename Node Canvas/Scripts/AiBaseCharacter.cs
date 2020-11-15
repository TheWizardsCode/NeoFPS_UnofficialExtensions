#if NODE_CANVAS_PRESENT
using NeoCC;
using NeoFPS;
using NeoFPS.CharacterMotion;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.StateMachines;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using WizardsCode.AI;
using WizardsCode.AI.Animation;
using WizardsCode.AI.BehaviourTree;
using Vector3 = UnityEngine.Vector3;

namespace WizardsCode.AI
{
    /// <summary>
    /// Handles the core setup of the AI Character and manages whether the character is alive or not.
    /// </summary>
    public class AiBaseCharacter : MonoBehaviour, IAiCharacter
    {
        [Header("Movement")]
        [SerializeField, Tooltip("The normal walking speed of this agent.")]
        float m_WalkingSpeed = 3.5f;
        [SerializeField, Tooltip("The normal running speed of this agent.")]
        float m_RunningSpeed = 8;

        [Header("Senses")]
        [SerializeField, Tooltip("The distance the agent will usually sense the presence of another character or animal behaving normally (i.e. not sneaking)without directly seeing, hearing or smelling them.")]
        float m_AwarenessDistance = 6;
        [SerializeField, Range(0, 1), Tooltip("The awareness level the agent will tend towards if no other stimulus is provided.")]
        float baselineAwarenessLevel = 0.4f;
        [SerializeField, Range(0.01f, 10f), Tooltip("The amount of awareness to gain/lose per second as the agent returns to the baseline.")]
        float awarenessBaseliningStep = 0.1f;
        [SerializeField, Tooltip("The distance the agent can see in normal conditions.")]
        float m_SightDistance = 50;
        [SerializeField, Tooltip("The field of view for this agent in normal conditions.")]
        float m_FieldOfView = 120;

        [Header("Seeking")]
        [SerializeField, Tooltip("The maximum distance the agent will seek at peak levels of awareness.")]
        float m_MaxSeekDistance = 30;
        [SerializeField, Tooltip("When seeking a target or a currentPOI how many seconds should they spend seeking?")]
        public float seekDuration = 5;

        [Header("Attacking")]
        [SerializeField, Tooltip("The speed of movement when in combat.")]
        float m_CombatMovementSpeed = 8;
        [SerializeField, Tooltip("The speed of the attack.")]
        internal float attackSpeed = 1;
        [SerializeField, Tooltip("Minimum number of backup AI before this character will approach to attack.")]
        int m_RequiredBackupCount = 2;

        [Header("Animation Settings")]
        [SerializeField, Tooltip("The name of an animation parameter that will trigger the death animation transition")]
        string m_DeathParameterName = "Die";

        [Header("Death")]
        [SerializeField, Tooltip("Use ragdoll (set to true) or use animations (set to false) for death.")]
        bool m_UseRagdoll = false;
        [SerializeField, Tooltip("Characters main collider that is used when not a ragdoll.")]
        Collider m_MainCollider = null;
        [SerializeField, Tooltip("The time until the character comes back from the dead. Set this to 0 if you don't want them to return.")]
        float m_RecoverFromDeathTime = 5f;

        [Header("Controllers")]
        [SerializeField, Tooltip("The AI Sounds to use for this character.")]
        AiSounds m_Sounds;

        public float awarenessLevel { get; set; }

        Rigidbody[] m_Rigidbodies;
        private float mass;
        bool m_IsRagDoll = false;
        NavMeshAgent m_agent;
        Animator m_animator;
        BehaviourTreeOwner m_BehaviourTreeOwner;
        private FSMOwner m_FsmOwner;
        private Transform m_Target;
        /// <summary>
        /// Get the current target character that this AI is focusing on.
        /// </summary>
        public Transform target
        {
            get { return m_Target; }
            set { m_Target = value; }
        }

        public float walkingSpeed
        {
            get { return m_WalkingSpeed; }
        }
        public float runningSpeed
        {
            get { return m_RunningSpeed; }
        }

        AiGroupController group;
        /// <summary>
        /// Test to see if this agent is classed as a leader.
        /// Leaders are either alone or a part of a group that
        /// follows their lead.
        /// </summary>
        public bool IsLeader
        {
            get
            {
                if (group == null) return true;
                return group.IsLeader;
            }
        }

        private float m_DistanceToTarget;

        public event CharacterDelegates.OnControllerChange onControllerChanged;
        public event CharacterDelegates.OnIsAliveChange onIsAliveChanged;

        /// <summary>
        /// Get the current target character that this AI is focusing on.
        /// </summary>
        public float distanceToTarget
        {
            get { return m_DistanceToTarget; }
            set { m_DistanceToTarget = value; }
        }

        /// <summary>
        /// The field of view for this agent in normal conditions.
        /// </summary>
        public float fieldOfView
        {
            get { return m_FieldOfView; }
        }

        /// <summary>
        /// The distance within which a target normally needs to be for the agent to consider them important.
        /// </summary>
        public float awarenessDistance
        {
            get { return m_AwarenessDistance; }
        }

        /// <summary>
        /// The distance the agent can see in normal conditions.
        /// </summary>
        public float sightDistance
        {
            get { return m_SightDistance; }
        }

        /// <summary>
        /// The distance within which a target normally needs to be for the agent to consider them important.
        /// </summary>
        public float seekDistance
        {
            get { return m_MaxSeekDistance * awarenessLevel; }
        }

        /// <summary>
        /// The speed of movement when in combat.
        /// </summary>
        public float combatMovementSpeed
        {
            get { return m_CombatMovementSpeed; }
        }

        /// <summary>
        /// The required number of ally characters required in a group with this agent before
        /// it feels confident enough to attack.
        /// </summary>
        public float requiredBackupCount
        {
            get { return m_RequiredBackupCount; }
        }

        public IQuickSlots quickSlots
        {
            get;
            private set;
        }
        public IController controller { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public FirstPersonCamera fpCamera => throw new NotImplementedException("AI Characters do not have cameras attached. This should not be called");

        public IMotionController motionController => throw new NotImplementedException("AI Characters do not motion controllers. This should not be called");

        public IAimController aimController => throw new NotImplementedException("AI Characters do not use aim controllers. This should not be called");

        public ICharacterAudioHandler audioHandler => throw new NotImplementedException();

        public AdditiveTransformHandler headTransformHandler => throw new NotImplementedException();

        public AdditiveTransformHandler bodyTransformHandler => throw new NotImplementedException();

        public IInventory inventory => throw new NotImplementedException();

        private IHealthManager m_HealthManager = null;
        public IHealthManager healthManager
        {
            get { return m_HealthManager; }
            private set
            {
                if (m_HealthManager != null)
                {
                    m_HealthManager.onIsAliveChanged -= OnIsAliveChanged;
                    m_HealthManager.onHealthChanged -= OnHealthChanged;
                }
                m_HealthManager = value;
                if (m_HealthManager != null)
                {
                    m_HealthManager.onIsAliveChanged += OnIsAliveChanged;
                    m_HealthManager.onHealthChanged += OnHealthChanged;
                }
            }
        }
        public bool isAlive
        {
            get
            {
                if (healthManager != null)
                    return healthManager.isAlive;
                else
                    return true;
            }
        }

        public bool isPlayerControlled { get { return false; } }

        public bool isLocalPlayerControlled { get { return false; } }

        public bool isRemotePlayerControlled { get { return false; } }

        void Awake()
        {
            awarenessLevel = baselineAwarenessLevel;

            healthManager = GetComponent<IHealthManager>();
            m_agent = GetComponent<NavMeshAgent>();
            m_Rigidbodies = GetComponentsInChildren<Rigidbody>();
            mass = 0;
            for (int i = 0; i < m_Rigidbodies.Length; i++)
            {
                mass += m_Rigidbodies[i].mass;
            }

            m_animator = GetComponent<Animator>();
            quickSlots = GetComponent<IQuickSlots>();
            m_BehaviourTreeOwner = GetComponent<BehaviourTreeOwner>();
            m_FsmOwner = GetComponent<FSMOwner>();
            ToggleRagdoll(true);

            group = GetComponent<AiGroupController>();
        }

        private void InventorySelectionChanged(IQuickSlotItem item)
        {
            IInventoryItem i = (IInventoryItem)item;
            AiBaseWeapon weapon = item.GetComponent<AiBaseWeapon>();
            // FIXME: need an ID for Equipped iteam, can we use the constants from Neo?
            m_animator.SetBool(AnimParams.IS_ONE_HANDED, weapon.isOneHanded);
            m_animator.SetInteger(AnimParams.EQUIPPED_ITEM, weapon.name.GetHashCode());
        }

        /// <summary>
        /// Called whenever the agent is within possible hearing range of a sound.
        /// The agent doesn't automatically hear the sound it depends on how well
        /// they hear, their current awareness level and other factors.
        /// </summary>
        /// <param name="sourceTransform">The position of the source of the sound at the time the sound was emitted.</param>
        /// <param name="noticability">A value between 0 and 1 that represents how noticable a sound is. 0 means almost no agents will notice it. 1 means almost all agents within range will notice it.</param>
        public void hearSound(Transform sourceTransform, float noticeability)
        {
            float hearingDistance = 30; // the maximum distance at which the agent would normally hear a sound with noticeability 1
            float distance = Vector3.Distance(sourceTransform.position, transform.position);
            float chance = (noticeability + awarenessLevel) * (1 - (distance / hearingDistance));

            // TODO use barriers between character and sound source to influence whether a sound is heard
            if (UnityEngine.Random.value <= chance)
            {
                //Debug.Log(gameObject.name + " heard a sound and will investigate. Chance: " + chance + " distance " + distance);
                awarenessLevel = Mathf.Clamp01(awarenessLevel * 2);
                currentPOI = sourceTransform;
            }
        }

        RaycastHit m_Hit;
        bool hitCheck = false;
        float hitCheckDuration;
        private Vector3 m_ExternalForceMove = Vector3.zero;
        private object m_Rigidbody;
        private Coroutine awarenessLerpingCoroutine;

        /// <summary>
        /// The current point of interest in the environment. If non-null
        /// this will be an area that the agent has some interest in. It
        /// may be that they believe their target is there, or that they
        /// heard a sound and should investigate.
        /// </summary>
        public Transform currentPOI { get; set; }

        /// <summary>
        /// This is typically called from an animation event to test whether
        /// the weapon has made contact with a target and, if it has, damage
        /// is inflicted.
        /// </summary>
        public void OnCheckForHit()
        {
            if (m_Sounds != null)
            {
                m_Sounds.PlayAttackSound();
            }
            hitCheck = true;
            hitCheckDuration = 1f;
        }

        void Update()
        {
            UpdateAwarenessLevel();

            if (hitCheck)
            {
                PerformHitCheck();
            }
        }

        /// <summary>
        /// Move the characters awareness level back towards the steady state.
        /// </summary>
        private void UpdateAwarenessLevel()
        {
            float step = awarenessBaseliningStep * Time.deltaTime;
            if (awarenessLevel < baselineAwarenessLevel - step)
            {
                awarenessLevel += step;
            }
            else if (awarenessLevel > baselineAwarenessLevel + step)
            {
                awarenessLevel -= step;
            }
            else
            {
                awarenessLevel = baselineAwarenessLevel;
                currentPOI = null; // lost interest in the current POI
            }
        }

        /// <summary>
        /// Test if this character has hit anything with its equipped weapon
        /// in this frame.
        /// </summary>
        private void PerformHitCheck()
        {
            hitCheckDuration -= Time.deltaTime;
            hitCheck = hitCheckDuration > 0;

            IAiWeapon weapon = quickSlots.selected.GetComponent<IAiWeapon>();

            // check for a hit in various angles from the hitDetector
            // if any land they will impart damage and remaining checks will be skipped
            Vector3 forward = weapon.hitDetector.forward;
            Vector3[] direction = { forward,
                                        Quaternion.AngleAxis(15, Vector3.up) * forward,
                                        Quaternion.AngleAxis(-15, Vector3.up) * forward,
                                        Quaternion.AngleAxis(15, Vector3.right) * forward,
                                        Quaternion.AngleAxis(-15, Vector3.right) * forward
                };
            for (int i = 0; i < direction.Length; i++)
            {
                Ray ray = new Ray(weapon.hitDetector.position, direction[i]);
                if (CheckForHit(weapon, ray))
                {
                    break;
                }
            }
        }

        private bool CheckForHit(IAiWeapon weapon, Ray ray)
        {
            bool isHit = false;
            float maxDistance = 1.5f;

            Debug.DrawLine(weapon.hitDetector.position, weapon.hitDetector.position + (1.5f * ray.direction), Color.red, 3);
            if (PhysicsExtensions.RaycastNonAllocSingle(
                    ray,
                    out m_Hit,
                    maxDistance,
                    PhysicsFilter.Masks.BulletBlockers
            ))
            {
                IDamageHandler damageHandler = m_Hit.collider.GetComponent<IDamageHandler>();
                if (damageHandler != null)
                {
                    Debug.Log("Hit something with a damage handler" + damageHandler);
                    hitCheck = false;

                    IImpactHandler impactHandler = m_Hit.collider.GetComponent<IImpactHandler>();
                    if (impactHandler != null)
                    {
                        impactHandler.HandlePointImpact(m_Hit.point, ray.direction * weapon.impactForce);
                    }

                    isHit = true;
                    damageHandler.AddDamage(weapon.damageAmount, m_Hit, weapon);
                }
            }

            return isHit;
        }

        private void OnEnable()
        {
            if (quickSlots != null)
            {
                quickSlots.onSelectionChanged += InventorySelectionChanged;
            }
        }
        private void OnDisable()
        {
            healthManager.onHealthChanged -= OnHealthChanged;
            healthManager.onIsAliveChanged -= OnIsAliveChanged;
            if (quickSlots != null)
            {
                quickSlots.onSelectionChanged -= InventorySelectionChanged;
            }
        }

        private void OnHealthChanged(float from, float to, bool critical, IDamageSource source)
        {
            if (to - from < 0)
            {
                m_agent.isStopped = true;
                m_animator.SetTrigger(AnimParams.GET_HIT);
            }
        }

        protected virtual void OnIsAliveChanged(bool isAlive)
        {
            if (!isAlive)
            {
                if (m_UseRagdoll)
                {
                    AiSounds sounds = GetComponent<AiSounds>();
                    if (sounds != null)
                    {
                        sounds.PlayDeathSound();
                    }

                    ToggleRagdoll(false);
                }
                else
                {
                    m_animator.SetTrigger(m_DeathParameterName);
                }

                if (m_agent != null)
                {
                    m_agent.isStopped = true;
                }

                if (m_BehaviourTreeOwner != null)
                {
                    m_BehaviourTreeOwner.StopBehaviour();
                }

                if (m_FsmOwner != null)
                {
                    m_FsmOwner.StopBehaviour();
                }

                StartCoroutine(ReturnFromDeath());
            }
            else
            {
                if (m_UseRagdoll)
                {
                    ToggleRagdoll(true);
                }

                if (m_BehaviourTreeOwner != null)
                {
                    m_BehaviourTreeOwner.StartBehaviour();
                }

                if (m_FsmOwner != null)
                {
                    m_FsmOwner.StartBehaviour();
                }

                m_animator.Play("Idle");

                if (m_agent != null)
                {
                    m_agent.isStopped = false;
                }
            }
        }

        void ToggleRagdoll(bool isAnimating)
        {
            m_IsRagDoll = !isAnimating;
            if (m_MainCollider != null)
                m_MainCollider.enabled = isAnimating;
            for (int i = 0; i < m_Rigidbodies.Length; i++)
            {
                m_Rigidbodies[i].isKinematic = isAnimating;
            }

            m_animator.enabled = isAnimating;
        }

        /// <summary>
        /// Ends the hit reaction animation. Typically this will be called
        /// from an animation event when the hit reaction is due to end.
        /// However, it can be called to override the hit reaction animation
        /// if necessary.
        /// </summary>
        public void EndHitReaction()
        {
            m_agent.isStopped = false;
        }

        IEnumerator ReturnFromDeath()
        {
            if (m_RecoverFromDeathTime > 0)
            {
                yield return new WaitForSeconds(m_RecoverFromDeathTime);
                GetComponent<IHealthManager>().AddHealth(GetComponent<IHealthManager>().healthMax);
            }
        }

        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force, bool disableGroundSnapping = false)
        {
            // Factor in time and mass as required
            float multiplier = 1f;
            switch (mode)
            {
                case ForceMode.Force:
                    multiplier = Time.fixedDeltaTime / mass;
                    break;
                case ForceMode.Acceleration:
                    multiplier = Time.fixedDeltaTime;
                    break;
                case ForceMode.Impulse:
                    multiplier = 1f / mass;
                    break;
            }
            force *= multiplier;
            m_ExternalForceMove += force;

            if (disableGroundSnapping)
            {
                Debug.LogWarning("Calling AddForce on " + gameObject.name + " with disableGroundSnapping == true, but this is not supported yet.");
            }
        }

        public void Kill()
        {
            throw new NotImplementedException();
        }

        private void OnValidate()
        {
            if (m_Sounds == null)
            {
                m_Sounds = GetComponent<AiSounds>();
            }
        }

#if UNITY_EDITOR

        public void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, seekDistance);
            NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                Gizmos.DrawWireCube(navMeshAgent.destination, new Vector3(0.25f, 0.25f, 0.25f));
            }
        }

        [Title("Debug Tooling")]
        [Button]
        public void IncreaseAwareness()
        {
            awarenessLevel += 0.25f;
        }

        [Button]
        public void DecreaseAwareness()
        {
            awarenessLevel -= 0.25f;
        }
#endif
    }
}
#endif