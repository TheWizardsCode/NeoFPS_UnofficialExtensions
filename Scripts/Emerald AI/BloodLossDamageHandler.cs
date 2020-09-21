using NeoFPS;
using NeoFPS.EmeraldAI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static NeoFPS.BasicHealthManager;
using Random = UnityEngine.Random;

public class BloodLossDamageHandler : NeoFpsEmeraldAI_DamageHandler
{
    [Header("Bloodloss")]
    [SerializeField, Tooltip("The minimum multiplier for bloodloss from a hit.")]
    float m_MinBloodlossMultiplier = 0.01f;
    [SerializeField, Tooltip("The maximum multiplier for bloodloss from a hit.")]
    float m_MaxBloodlossMultiplier = 0.05f;

    [Header("Scoring")]
    int m_ScoreForCritical = 100;

    [Header("Events")]
    [SerializeField, Tooltip("Fired whenever damage is handled by this behaviour.")]
    FloatEvent m_OnRecievedDamageEvent;
    [SerializeField, Tooltip("Fired whenever a critical is handled by this behaviour.")]
    IntEvent m_OnRecievedCriticalHitEvent;

    float m_DamagePerSecond = 0;
    float m_CumulativeDamage = 0;
    private ScoreManager m_ScoreManager;


    [Serializable]
    public class IntEvent : UnityEvent<int>
    {
    }

    protected override void Awake() {
        base.Awake();
        m_ScoreManager = GameObject.FindObjectOfType<ScoreManager>();
    }

    private void OnIsAliveChanged(bool alive)
    {
        Destroy(this, 0.25f);
    }

    public override DamageResult AddDamage(float damage, IDamageSource source)
    {
        
        // TODO: This assume that this method is only called when damage comes from a weapon
        DamageResult result = base.AddDamage(damage, source);
        if (m_HealthManager.isAlive)
        {
            AddBloodloss(damage);
        } else
        {
            m_DamagePerSecond = 0;
            m_CumulativeDamage = 0;
        }
        CalculateScore(result, damage);
        return result;
    }

    protected void AddBleeding(float damagePerSecond)
    {
        // TODO: No Score added here as it assumed that if there is no Damage Source there is no weapon and thus it is bloodloss - is this a good assumption
        m_DamagePerSecond += damagePerSecond;
    }

    public override DamageResult AddDamage(float damage, RaycastHit hit)
    {
        // TODO: No Score added here as it assumed that if there is no Damage Source there is no weapon and thus it is bloodloss - is this a good assumption
        return base.AddDamage(damage, hit);
    }

    public override DamageResult AddDamage(float damage, RaycastHit hit, IDamageSource source)
    {
        return base.AddDamage(damage, hit, source);
    }

    void AddBloodloss(float damage)
    {
        m_DamagePerSecond = damage * Random.Range(m_MinBloodlossMultiplier, m_MaxBloodlossMultiplier);
    }

    private void CalculateScore(DamageResult result, float damage)
    {
        if (result == DamageResult.Ignored)
        {
            return;
        }

        m_OnRecievedDamageEvent.Invoke(damage);

        if (result == DamageResult.Critical)
        {
            m_OnRecievedCriticalHitEvent.Invoke(m_ScoreForCritical);
        }
    }

    private void OnEnable()
    {
        ScoreManager scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        m_OnRecievedCriticalHitEvent.AddListener(scoreManager.OnLandedCriticalHit);
        m_OnRecievedDamageEvent.AddListener(scoreManager.OnLandedDamage);

        m_HealthManager.onIsAliveChanged += OnIsAliveChanged;
    }

    private void OnDisable()
    {
        m_OnRecievedCriticalHitEvent.RemoveAllListeners();
        m_OnRecievedDamageEvent.RemoveAllListeners();

        m_HealthManager.onIsAliveChanged -= OnIsAliveChanged;
    }

    void Update()
    {
        m_CumulativeDamage += m_DamagePerSecond * Time.deltaTime;

        if (m_CumulativeDamage > 3) { 
            AddDamage(m_CumulativeDamage);
            m_ScoreManager.OnBloodloss(m_CumulativeDamage);
            m_CumulativeDamage = 0;
        }
    }
}
