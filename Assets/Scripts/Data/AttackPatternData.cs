using UnityEngine;
using System.Collections.Generic;

namespace UndertaleEncounter
{
    [CreateAssetMenu(fileName = "NewAttackPattern", menuName = "Undertale/Attack Pattern")]
    public class AttackPatternData : ScriptableObject
    {
        public float patternDuration = 5.0f;
        public bool clearProjectilesOnEnd = true;
        public List<AttackWaveData> waves = new List<AttackWaveData>();
    }
}
