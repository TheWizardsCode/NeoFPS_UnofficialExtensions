using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace WizardsCode.AI.Unofficial.ScoreSystem
{
    public class HudShotFeedback : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The text readout for the most recent shot taken.")]
        private Text m_ShotText = null;
        [SerializeField, Tooltip("The number of log lines to show in the HUD.")]
        private int m_NumberOfLines = 4;

        ScoreManager m_ScoreManager = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from old character
            if (m_ScoreManager != null)
                m_ScoreManager.onShotTaken -= OnShotTaken;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            if (m_ScoreManager != null)
                m_ScoreManager.onShotTaken -= OnShotTaken;

            if (character as Component != null)
                m_ScoreManager = GameObject.FindObjectOfType<ScoreManager>();
            else
                m_ScoreManager = null;

            if (m_ScoreManager != null)
            {
                m_ScoreManager.onShotTaken += OnShotTaken;
                OnShotTaken("");
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnShotTaken(string log)
        {
            string original = m_ShotText.text;
            m_ShotText.text = log;
            string[] lines = Regex.Split(m_ShotText.text, "\r\n|\r|\n");

            int lineCount = 0;
            string resultLog = "";

            for (int i = lines.Length - 1; i >= 0 && lineCount < m_NumberOfLines; i--)
            {
                if (!lines[i].StartsWith("Bloodloss") && lines[i] != "")
                {
                    lineCount++;
                    resultLog = lines[i] + "\n" + resultLog;
                }
            }
            if (lineCount == 0)
            {
                resultLog = original;
            }
            m_ShotText.text = resultLog;
        }
    }
}