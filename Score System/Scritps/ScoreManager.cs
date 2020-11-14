using NeoFPS;
using NeoFPS.ModularFirearms;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace WizardsCode.AI.Unofficial.ScoreSystem
{
    public class ScoreManager : MonoBehaviour
    {
        public enum ScoreType { Shot, Hit, Bloodloss, Critical, Kill }

        [SerializeField, Tooltip("How many points does the player loose for each shot?")]
        private int m_ScoreLossPerShot = 25;
        public event ScoreDelegates.OnScoreChanged onScoreChanged;
        public event ScoreDelegates.OnShotTaken onShotTaken;

        private string m_ShotLog = "";

        int m_Score = 0;
        Transform m_Player = null;
        private FpsInventoryBase m_Inventory;
        private ITrigger currentTrigger;

        private void OnDestroy()
        {
            m_ShotLog += m_CurrentLog.ToString(); // ensure the last shot is recorded

            Debug.Log("Shot Log");
            Debug.Log("========");
            Debug.Log("Total Score: " + score);
            Debug.Log(m_ShotLog);
        }

        public int score
        {
            get { return m_Score; }
            private set
            {
                float old = m_Score;
                if (old != value)
                {
                    m_Score = value;
                    if (onScoreChanged != null)
                    {
                        onScoreChanged.Invoke(old, m_Score);
                    }
                }
            }
        }

        ShotLog m_CurrentLog = new ShotLog();
        private void AddToLog(ScoreType type, int pointsDelta)
        {
            if (m_CurrentLog.isLogComplete)
            {
                m_ShotLog += m_CurrentLog.ToString();
                m_ShotLog += "\n";

                m_CurrentLog = new ShotLog();
            }

            m_CurrentLog.AddLog(type, pointsDelta);

            if (onShotTaken != null)
            {
                onShotTaken.Invoke(m_CurrentLog.ToString());
            }
        }

        public void OnKill(int points)
        {
            score += points;
            AddToLog(ScoreType.Kill, points);
        }

        public void OnLandedCriticalHit(int points)
        {
            score += points;
            AddToLog(ScoreType.Critical, points);
        }

        public void OnLandedDamage(float damage)
        {
            score += (int)damage;
            AddToLog(ScoreType.Hit, (int)damage);
        }

        public void OnShot(int lostPoints)
        {
            score -= lostPoints;
            AddToLog(ScoreType.Shot, -lostPoints);
        }

        public void OnBloodloss(float damage)
        {
            int lostPoints = (int)damage;
            score -= lostPoints;
            AddToLog(ScoreType.Bloodloss, -lostPoints);
        }
    }

    public static class ScoreDelegates
    {
        public delegate void OnScoreChanged(float from, float to);
        public delegate void OnShotTaken(string log);
    }

    public class ShotLog
    {
        protected float m_Time = float.PositiveInfinity;
        private const float m_MaxTime = 0.25f;
        private LogEntry[] entries;
        public ShotLog()
        {
            entries = new LogEntry[Enum.GetNames(typeof(ScoreManager.ScoreType)).Length];
        }

        protected float time
        {
            get
            {
                return m_Time;
            }
            set
            {
                if (m_Time > value)
                {
                    m_Time = value;
                }
            }
        }

        public void AddLog(ScoreManager.ScoreType scoreType, float pointsDelta)
        {
            time = Time.time;
            entries[(int)scoreType] = new LogEntry(scoreType, pointsDelta);
        }

        public bool isLogComplete
        {
            get
            {
                if (Time.time > time + m_MaxTime)
                {
                    return true;
                }

                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].scoreType != ScoreManager.ScoreType.Bloodloss && entries[i].pointsDelta == 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < entries.Length; i++)
            {
                result += entries[i].ToString();
            }
            return result;
        }

        private struct LogEntry
        {
            internal ScoreManager.ScoreType scoreType;
            internal float pointsDelta;

            internal LogEntry(ScoreManager.ScoreType type, float points)
            {
                scoreType = type;
                pointsDelta = points;
            }

            override public string ToString()
            {
                if (pointsDelta != 0)
                {
                    return scoreType.ToString() + " for " + pointsDelta + "\n";
                }
                else
                {
                    return "";
                }
            }
        }
    }
}