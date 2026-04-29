using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UndertaleEncounter
{
    public class EnemySelectButton : BattleButton
    {
        [Header("Enemy UI")]
        public TextMeshProUGUI nameText;
        public Image healthBar;

        private Material _materialInstance;

        public void Setup(EnemyInstance enemy)
        {
            if (nameText != null) nameText.text = "* " + enemy.enemyName;
            
            if (healthBar != null)
            {
                if (_materialInstance == null)
                {
                    _materialInstance = new Material(healthBar.material);
                    healthBar.material = _materialInstance;
                }
                
                float fill = Mathf.Clamp01(enemy.currentHp / enemy.maxHp);
                _materialInstance.SetFloat("_health", fill);
            }
        }
    }
}
