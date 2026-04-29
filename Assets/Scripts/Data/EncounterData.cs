using UnityEngine;
using System.Collections.Generic;

namespace UndertaleEncounter
{
    [CreateAssetMenu(fileName = "NewEncounterData", menuName = "Undertale/Encounter Data")]
    public class EncounterData : ScriptableObject
    {
        public List<EnemyData> enemies = new List<EnemyData>();
        public List<string> mainMenuMessages = new List<string> { "* Te encuentras con un enemigo.", "* Huele a peligro." };
    }
}
