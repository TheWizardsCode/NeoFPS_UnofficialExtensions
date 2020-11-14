#if NODE_CANVAS_PRESENT
using NeoFPS;
using SharpNav.Crowds;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using WizardsCode.AI;

namespace WizardsCode.AI
{
    /// <summary>
    /// A sound source handles the creation of sound. Attach it to an object and fire sounds
    /// via this component to scan for, and process, agents that might hear the sound.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundSource : MonoBehaviour
    {
        [SerializeField, Tooltip("The default 'noticeability' of sounds being emitted by this object. 0 means will almost never be noticed without aid. 1 means will almost always be noticed by anyone in the vicinity.")]
        private float defaultNoticeability = 0.5f;
        [SerializeField, Tooltip("Notify of all sounds produced or just specific sounds. If true, all sounds produced by the Audio Source identified will be broadcast to all listeners in the area, regardless of how the sound is triggered. If false only sounds triggered by calling a Play(...) method on this component will be notified.")]
        private bool notifyOfAllSounds = true;
        [SerializeField, Tooltip("The audio source to use to play sounds and to monitor for sounds if notifyOfAllSounds is true. If no Audio Source is supplied  it is assumed we should use one on the same game object as this component.")]
        private AudioSource m_AudioSource;
        
        private AiBaseCharacter m_Self;

        bool isNotifyingOfSound = false;

        private void Awake()
        {
            m_Self = GetComponentInParent<AiBaseCharacter>();
        }
        /// <summary>
        /// Check to see if agents heard a sound form this audio source.
        /// </summary>
        /// <param name="noticability">A value between 0 and 1 that represents how noticable a sound is. 0 means no agents will notice it without some other means of awareness. 1 means almost all agents within range will notice it.</param>
        private IEnumerator NotifyOfSound(float noticability)
        {
            isNotifyingOfSound = true;

            float range = 30;
            int maxAgents = 200;
            Collider[] agentColliders = new Collider[maxAgents];
            int numOfAgents = Physics.OverlapSphereNonAlloc(transform.position, range, agentColliders, PhysicsFilter.LayerFilter.CharacterControllers);

            for (int i = 0; i < numOfAgents; i++)
            {
                AiBaseCharacter character = agentColliders[i].GetComponent<AiBaseCharacter>();
                if (character != null && character != m_Self)
                {
                    character.hearSound(transform, noticability);
                }
                yield return new WaitForEndOfFrame();
            }

            isNotifyingOfSound = false;
        }

        /// <summary>
        /// Play the clip supplied and if noticability > 0 start notifying 
        /// characters within range of the sound.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="noticability"></param>
        /// <param name="volume">The volume at which to play the sound.</param>
        public void Play(AudioClip clip, float noticeability, float volume = 1)
        {
            m_AudioSource.volume = volume;
            m_AudioSource.clip = clip;
            m_AudioSource.Play();

            if (isNotifyingOfSound && noticeability < defaultNoticeability) return;

            if (noticeability > 0)
            {
                StartCoroutine(NotifyOfSound(noticeability));
            }
        }

        void Update()
        {
            if (!notifyOfAllSounds || m_AudioSource == null || isNotifyingOfSound) return;

            if (m_AudioSource.isPlaying)
            {
                StartCoroutine(NotifyOfSound(defaultNoticeability));
            }
        }

        public void OnValidate()
        {
            if (m_AudioSource == null)
            {
                m_AudioSource = GetComponent<AudioSource>();
            }
        }

    }
}
#endif