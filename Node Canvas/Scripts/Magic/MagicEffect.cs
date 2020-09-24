#if NEOFPS
using NeoFPS;
using UnityEngine;

namespace WizardsCode.AI.Magic
{
    /// <summary>
    /// Represents the core configuration of a magic effect that can be 
    /// created by an AI using the `MagicEffectController`.
    /// </summary>
    public class MagicEffect : MonoBehaviour, IDamageSource
    {
        [Header("Movement")]
        [SerializeField, Tooltip("The speed the effect should move in units per second")]
        float speed = 20;
        [SerializeField, Tooltip("The duration of the effect before it destroys itself.")]
        float duration = 10f;

        [Header("Collision Detection")]
        [SerializeField, Tooltip("The layers that this effect should collide with.")]
        LayerMask collisionLayers = ~0;
        [SerializeField, Tooltip("Effects to be fired when this effect collides with something.")]
        MagicEffect[] collisionEffects;
        [SerializeField, Tooltip("If set to true this effect and all children will be destroyed upon collision.")]
        bool destroyOnCollision = true;
        [SerializeField, Tooltip("The distance to raycast forward when checking for a collision.")]
        float collisionDetectionRange = 0.15f;

        [Header("Damage")]
        [SerializeField, Tooltip("The damage this magic does when colliding with anything that has a Damage Handler.")]
        private float damageAmount = 50f;
        [SerializeField, Tooltip("The force to impart on the impacted object. Requires either an impact handler on the hit object.")]
        private float impactForce = 15f;

        private GameObject instance;

        Transform m_Target;
        public Transform target
        {
            get { return m_Target; }
            set { 
                m_Target = value;
            }
        }

        Vector3 aimPosition
        {
            get
            {
                Vector3 pos = m_Target.position;
                pos.y = m_Target.GetComponent<CapsuleCollider>().center.y;
                return pos;
            }
        }

#region Neo FPS IDamageSource
        public DamageFilter outDamageFilter {
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
#endregion

        public string description => throw new System.NotImplementedException();

        private void Awake()
        {
            instance = this.gameObject;
            if (duration > 0.001f)
            {
                Destroy(instance, duration);
            }
        }

        private void Update()
        {
            Vector3 frameMoveOffset = Vector3.zero;

            if (target == null)
            {
                Vector3 currentForwardVector = Vector3.forward * speed * Time.deltaTime;
                frameMoveOffset = transform.localRotation * currentForwardVector;
            }
            else
            {
                Vector3 forward = (aimPosition - transform.position).normalized;
                Vector3 currentForwardVector = forward * speed * Time.deltaTime;
                frameMoveOffset = currentForwardVector;
            }

            transform.position += frameMoveOffset;

            Debug.DrawRay(transform.position, frameMoveOffset.normalized * collisionDetectionRange);

            RaycastHit hit;
            if (Physics.Raycast(transform.position, frameMoveOffset.normalized, out hit, collisionDetectionRange, collisionLayers))
            {
                OnCollision(hit);
                if (destroyOnCollision)
                {
                    Destroy(gameObject, 0.01f);
                }
                return;
            }
        }

        /// <summary>
        /// Called when the effect collides with something. This will
        /// spawn the collision effects, impart damage and destroy the
        /// the attached object (if required).
        /// </summary>
        /// <param name="hit">The raycast that registered the hit.</param>
        void OnCollision(RaycastHit hit)
        {
            for (int i = 0; i < collisionEffects.Length; i++)
            {
                MagicEffect instance = Instantiate(collisionEffects[i], hit.point + hit.normal, new Quaternion());
                instance.transform.LookAt(hit.point + hit.normal + hit.normal);

                IDamageHandler damageHandler = hit.collider.GetComponent<IDamageHandler>();
                if (damageHandler != null)
                {
                    damageHandler.AddDamage(damageAmount, hit, this);
                }

                IImpactHandler impactHandler = hit.collider.GetComponent<IImpactHandler>();
                if (impactHandler != null)
                {
                    impactHandler.HandlePointImpact(hit.point, transform.forward * impactForce);
                }

                Destroy(instance.gameObject, instance.duration);
            }
        }
    }
}
#endif