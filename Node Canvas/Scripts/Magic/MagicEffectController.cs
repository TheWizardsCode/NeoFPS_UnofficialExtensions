#if NODE_CANVAS_PRESENT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.AI;

namespace WizardsCode.AI.Magic
{
    /// <summary>
    /// Allows An `AiBaseCharacter` the conrol of a magic effect through Animation Events.
    /// </summary>
    public class MagicEffectController : MonoBehaviour
    {
        [SerializeField, Tooltip("The list off effects that can be fired by the `ActivateEffect(id)` method / event")]
        MagicEffect[] m_EffectTemplates;
        [SerializeField, Tooltip("The transformt that holds the position and rotation of the effect at the point of spawning.")]
        Transform m_SpawnPoint;

        AiBaseCharacter character = null;

        private void Awake()
        {
            character = GetComponent<AiBaseCharacter>();
        }

        /// <summary>
        /// Activate a specific effect.
        /// You can call this method from, for example, an animation event.
        /// </summary>
        /// <param name="id"></param>
        public void ActivateEffect(int id)
        {   
            MagicEffect effect = Instantiate(m_EffectTemplates[id], m_SpawnPoint.position, m_SpawnPoint.rotation);
            effect.target = character.target;
        }
    }
}
#endif