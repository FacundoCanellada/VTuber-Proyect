using UnityEngine;
using System.Collections.Generic;

namespace UndertaleEncounter
{
    [CreateAssetMenu(fileName = "NewAttackPattern", menuName = "Undertale/Attack Pattern")]
    public class AttackPatternData : ScriptableObject
    {
        public float patternDuration = 5.0f;
        public bool clearProjectilesOnEnd = true;
        
        [Header("Battle Box Settings")]
        public Vector2 targetBoxSize = new Vector2(4, 4);
        public Vector3 targetBoxPosition = Vector3.zero;

        public List<AttackWaveData> waves = new List<AttackWaveData>();
    }
}
