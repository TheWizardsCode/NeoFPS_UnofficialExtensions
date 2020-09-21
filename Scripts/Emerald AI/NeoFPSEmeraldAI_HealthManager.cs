using EmeraldAI;
using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoFPSEmeraldAI_HealthManager : BasicHealthManager
{
    [SerializeField, Tooltip("The number of points for getting a kill of this creature.")]
    int m_PointsForKill = 100;

    ScoreManager m_ScoreManager = null;
    EmeraldAISystem m_EmeraldAI = null;

    private void Awake()
    {
        m_ScoreManager = GameObject.FindObjectOfType<ScoreManager>();
        m_EmeraldAI = GetComponent<EmeraldAISystem>();
        if (m_EmeraldAI.CurrentHealth != (int)health)
        {
            Debug.LogWarning("NeoFPS health manager on " + this.gameObject.name + " has a different health value to the EmeraldAISystem. Using NeoFPS value.");
            m_EmeraldAI.CurrentHealth = (int)health;
            m_EmeraldAI.StartingHealth = (int)health;
        }
    }

    private void OnEnable()
    {
        onIsAliveChanged += OnIsAliveChanged;
    }

    private void Disable()
    {
        onIsAliveChanged -= OnIsAliveChanged;
    }

    private void OnIsAliveChanged(bool alive)
    {
        if (!alive && m_ScoreManager != null)
        {
            m_ScoreManager.OnKill(m_PointsForKill);
        }
    }
}
