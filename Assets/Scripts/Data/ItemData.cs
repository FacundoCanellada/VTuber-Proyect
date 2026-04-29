using UnityEngine;
using System.Collections.Generic;

namespace UndertaleEncounter
{
    [CreateAssetMenu(fileName = "NewItemData", menuName = "Undertale/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemName = "Item";
        public float healAmount = 20.0f;
        [TextArea]
        public string description = "Cura vida.";
        public string consumeMessage = "* Te comiste el Item. Recuperaste 20 HP.";
    }
}
