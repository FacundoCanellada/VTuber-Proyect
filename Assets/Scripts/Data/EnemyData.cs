using UnityEngine;
using System.Collections.Generic;

namespace UndertaleEncounter
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Undertale/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName = "Enemy";
        public float maxHp = 100.0f;
        public float currentHp = 100.0f;
        public List<string> availableActs = new List<string> { "Check" };
        public List<AttackPatternData> attackPatterns = new List<AttackPatternData>();
    }
}
