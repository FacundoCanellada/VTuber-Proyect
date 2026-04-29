using UnityEngine;
using System.Collections.Generic;

namespace UndertaleEncounter
{
    [CreateAssetMenu(fileName = "NewPlayerData", menuName = "Undertale/Player Data")]
    public class PlayerData : ScriptableObject
    {
        public string playerName = "FRISK";
        public int level = 1;
        public float maxHp = 20.0f;
        public float currentHp = 20.0f;
        public List<ItemData> inventory = new List<ItemData>();

        public void Heal(float amount) => currentHp = Mathf.Clamp(currentHp + amount, 0, maxHp);
        public void Damage(float amount) => currentHp = Mathf.Clamp(currentHp - amount, 0, maxHp);
    }
}