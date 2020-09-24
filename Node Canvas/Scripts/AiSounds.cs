#if NODE_CANVAS_PRESENT
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WizardsCode.AI
{
    [RequireComponent(typeof(SoundSource))]
    public class AiSounds : MonoBehaviour
    {
        [Header("Idle")]
        [SerializeField, Tooltip("Idle Sounds to play at random whenever PlayIdleSound(...) is called")]
        AudioClip[] m_IdleSounds;
        [SerializeField, Range(0f, 1f), Tooltip("The chance of an idle sound being played each time PlayIdleSound is called.")]
        float m_IdleSoundChance = 0.3f;

        [Header("Attack")]
        [SerializeField, Tooltip("Attack Sounds to play at random whenever PlayAttackSound(...) is called")]
        AudioClip[] m_AttackSounds;
        [SerializeField, Range(0f, 1f), Tooltip("The volume of the attack sound.")]
        float m_AttackVolume = 1f;

        [Header("Hit")]
        [SerializeField, Tooltip("Hit Sounds to play at random whenever PlayhitSound(...) is called")]
        AudioClip[] m_HitSounds;
        [SerializeField, Range(0f, 1f), Tooltip("The volume of the hit sound.")]
        float m_HitVolume = 1f;

        [Header("Death")]
        [SerializeField, Tooltip("Death Sounds to play at random whenever PlayDeathSound(...) is called")]
        AudioClip[] m_DeathSounds;
        [SerializeField, Range(0f, 1f), Tooltip("The volume of the death sound.")]
        float m_DeathVolume = 0.7f;

        [Header("Warning")]
        [SerializeField, Tooltip("The sound to play when the agent issues a warning.")]
        AudioClip m_WarningClip;

        [Header("Sound Settings")]
        [SerializeField, Tooltip("The Sound Source through which to play sounds.")]
        SoundSource m_SoundSource;
        [SerializeField, Tooltip("The default volume to be used if not set by the specific sound settings.")]
        float m_DefaultVolume = 0.2f;

        public void PlayWarningSound()
        {
            if (m_SoundSource == null || m_WarningClip == null) return;

            m_SoundSource.Play(m_WarningClip, 0.6f);
        }

        /// <summary>
        /// Play a random idle sound.
        /// </summary>
        /// <param name="chance">The chance a sound will be played from 0 to 1, where 1 means always play, 0.5 means play half the time and 0 means never play.</param>
        [Obsolete("Chance is no longer provided by the method call see m_IdleSondChance, settable in the inspector. Chance will be overrwritten by this value.")]
        public void PlayIdleSound(float chance)
        {
            PlayIdleSound();
        }

        /// <summary>
        /// Attempt to play a random idle sound. A sound will only be played 
        /// `m_IdleSoundChance` % of the time.
        /// </summary>
        public void PlayIdleSound() {
            if (m_IdleSounds.Length == 0) return;

            if (Random.value <= m_IdleSoundChance)
            {
                PlayRandomSound(m_IdleSounds);
            }
        }

        /// <summary>
        /// Play a random hit sound.
        /// </summary>
        public void PlayHitSound()
        {
            if (m_HitSounds.Length == 0) return;

            PlayRandomSound(m_HitSounds, m_HitVolume);
        }

        /// <summary>
        /// Play a random death sound.
        /// </summary>
        public void PlayDeathSound()
        {
            if (m_DeathSounds.Length == 0) return;

            PlayRandomSound(m_DeathSounds, m_DeathVolume);
        }

        public void PlayAttackSound()
        {
            if (m_AttackSounds.Length == 0) return;

            PlayRandomSound(m_AttackSounds, m_AttackVolume);
        }

        /// <summary>
        /// Play a random sound from a collection of sounds.
        /// </summary>
        /// <param name="clips">The collection of sounds to pull the random sound from.</param>
        private void PlayRandomSound(AudioClip[] clips)
        {
            PlayRandomSound(clips, m_DefaultVolume);
        }

        /// <summary>
        /// Play a random sound from a collection of sounds.
        /// </summary>
        /// <param name="clips">The collection of sounds to pull the random sound from.</param>
        /// <param name="volume">The volume at whivh to play the sound</param>
        private void PlayRandomSound(AudioClip[] clips, float volume)
        {
            int idx = Random.Range(0, clips.Length);
            m_SoundSource.Play(clips[idx], 0, volume);
        }

        private void OnValidate()
        {
            if (m_SoundSource == null)
            {
                m_SoundSource = GetComponent<SoundSource>();
            }
        }
    }
}
#endif