#if EMERALD_AI_PRESENT
using EmeraldAI;
using NeoFPS;
using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.NeoFPS.Unofficial.ScoreSystem;

namespace WizardsCode.NeoFPS.Unofficial.EmeraldAI
{
    public class NeoFPSEmeraldAI_ShotEvents : MonoBehaviour
    {
        [SerializeField, Tooltip("How many points does the player loose for each shot?")]
        private int m_ScoreLossPerShot = 25;

        Transform m_Player = null;
        private FpsInventoryBase m_Inventory;
        private ITrigger currentTrigger;
        private ScoreManager m_ScoreManager;

        private void Awake()
        {
            m_ScoreManager = GameObject.FindObjectOfType<ScoreManager>();
        }

        private void OnEnable()
        {
            SpawnPoint spawn = FindObjectOfType<SpawnPoint>();
            spawn.onSpawn += OnPlayerSpawn;
        }

        private void OnDisable()
        {
            SpawnPoint spawn = FindObjectOfType<SpawnPoint>();
            if (spawn != null)
            {
                spawn.onSpawn -= OnPlayerSpawn;
            }
        }

        private void OnPlayerSpawn()
        {
            m_Player = GameObject.FindGameObjectWithTag("Player").transform;
            m_Inventory = m_Player.GetComponent<FpsInventoryBase>();
            m_Inventory.onSelectionChanged += OnInventorySelectionChanged;
            OnInventorySelectionChanged(m_Inventory.selected);
        }

        private void OnInventorySelectionChanged(IQuickSlotItem item)
        {
            if (item == null) return;

            ITrigger trigger = item.GetComponent<ITrigger>();

            if (trigger != null)
            {
                if (currentTrigger != null)
                {
                    currentTrigger.onShoot -= OnShoot;
                }
                currentTrigger = trigger;
                currentTrigger.onShoot += OnShoot;
            }
        }

        private void OnShoot()
        {
            m_ScoreManager.OnShot(m_ScoreLossPerShot);

            // Find all AIs within x metres and make them flee
            Collider[] colliders = Physics.OverlapSphere(m_Player.position, 150);
            for (int i = 0; i < colliders.Length; i++)
            {
                EmeraldAIEventsManager events = colliders[i].GetComponent<EmeraldAIEventsManager>();
                if (events != null)
                {
                    events.FleeFromTarget(m_Player);
                }
            }
        }
    }
}
#endif