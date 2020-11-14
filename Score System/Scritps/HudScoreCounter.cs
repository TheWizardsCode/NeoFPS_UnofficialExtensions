using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoFPS.SinglePlayer;
using NeoFPS;

namespace WizardsCode.AI.Unofficial.ScoreSystem
{
    public class HudScoreCounter : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The text readout for the current character score.")]
        private Text m_ScoreText = null;

        ScoreManager m_ScoreManager = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from old character
            if (m_ScoreManager != null)
                m_ScoreManager.onScoreChanged -= OnScoreChanged;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            if (m_ScoreManager != null)
                m_ScoreManager.onScoreChanged -= OnScoreChanged;

            if (character as Component != null)
                m_ScoreManager = GameObject.FindObjectOfType<ScoreManager>();
            else
                m_ScoreManager = null;

            if (m_ScoreManager != null)
            {
                m_ScoreManager.onScoreChanged += OnScoreChanged;
                OnScoreChanged(0f, m_ScoreManager.score);
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        protected virtual void OnScoreChanged(float from, float to)
        {
            m_ScoreText.text = ((int)to).ToString();
        }
    }
}