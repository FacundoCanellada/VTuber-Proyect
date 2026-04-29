using UnityEngine;

namespace UndertaleEncounter
{
    [System.Serializable]
    public class EnemyInstance
    {
        public EnemyData Data { get; private set; }
        public float currentHp;
        public float maxHp;

        public string enemyName => Data != null ? Data.enemyName : "Enemy";

        public EnemyInstance(EnemyData data)
        {
            Data = data;
            currentHp = data.maxHp;
            maxHp = data.maxHp;
        }
    }
}
