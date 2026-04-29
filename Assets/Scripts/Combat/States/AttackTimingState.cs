using UnityEngine;
using System.Collections;

namespace UndertaleEncounter
{
    public class AttackTimingState : BattleStateBase
    {
        public override BattleState GetStateID() => BattleState.ATTACK_TIMING;

        [SerializeField] private AttackBarController attackBar;
        [SerializeField] private DamagePopup damagePopupPrefab;
        [SerializeField] private float baseDamage = 10f;

        public override void OnEnter()
        {
            Debug.Log("--- Iniciando Minijuego de Ataque ---");
            manager.GetCanvasController().CloseAllMenus();
            
            if (attackBar != null)
            {
                attackBar.gameObject.SetActive(true);
                float boxWidth = manager.GetBattleBox().GetSize().x;
                attackBar.Initialize(boxWidth);
                
                // Clear previous listeners just in case
                attackBar.OnAttackComplete = null; 
                attackBar.OnAttackComplete += HandleAttackComplete;
            }
            else
            {
                Debug.LogWarning("[AttackTimingState] attackBar no asignado en el inspector.");
                StartCoroutine(DelayedTransition());
            }
        }

        private void HandleAttackComplete(float multiplier, bool isPerfect)
        {
            float totalDamage = baseDamage * multiplier;
            var targetData = manager.SelectedEnemy;

            if (targetData != null)
            {
                targetData.currentHp = Mathf.Max(0, targetData.currentHp - totalDamage);
                
                if (damagePopupPrefab != null)
                {
                    // Obtener el Canvas y el contenedor de UI
                    var canvas = manager.GetCanvasController();
                    Transform parent = canvas.transform.Find("UI") ?? canvas.transform;

                    // Instanciar el popup
                    var popup = Instantiate(damagePopupPrefab, parent);
                    
                    // Posicionamiento: Convertir posición del enemigo a Screen Space
                    Vector2 screenPos = GetEnemyScreenPosition(targetData);
                    
                    // Ajustar a posición local del Canvas
                    RectTransform rect = popup.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        // Asumimos que el Canvas es Overlay o Camera y el RectTransform está en el centro
                        rect.position = screenPos;
                    }

                    popup.Setup(totalDamage, targetData.currentHp, targetData.maxHp);
                }
            }

            StartCoroutine(DelayedTransition());
        }

        private Vector2 GetEnemyScreenPosition(EnemyInstance enemy)
        {
            Transform spawnParent = manager.GetEnemySpawnParent();
            if (spawnParent != null)
            {
                // Buscar por nombre (o podrías implementar una referencia directa)
                foreach (Transform child in spawnParent)
                {
                    if (child.name.Contains(enemy.enemyName))
                    {
                        return Camera.main.WorldToScreenPoint(child.position);
                    }
                }
            }
            
            // Si no se encuentra, usar el centro de la pantalla (offseteado arriba)
            return new Vector2(Screen.width / 2f, Screen.height / 2f + 150f);
        }

        private IEnumerator DelayedTransition()
        {
            yield return new WaitForSeconds(1.5f);
            manager.ChangeState(BattleState.ENEMY_TURN);
        }

        private void FinishAttack()
        {
            manager.ChangeState(BattleState.ENEMY_TURN);
        }
    }
}