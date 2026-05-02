using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UndertaleEncounter
{
    public class MenuNavigationState : BattleStateBase
    {
        public override BattleState GetStateID() => BattleState.PLAYER_MENU;

        private BattleCanvasController _canvasController;
        private bool _isChoosingAct = false;
        private EnemyInstance _selectedEnemyForAct;
        
        public override void OnEnter()
        {
            _canvasController = manager.GetCanvasController();
            _isChoosingAct = false;
            
            if (InputReader.Instance != null)
            {
                InputReader.Instance.EnableUIMode();
            }

            var box = manager.GetBattleBox();
            if (box != null)
            {
                // Vuelve al tamaño y posición por defecto con animación
                box.ResetToDefault(true); 
                StartCoroutine(WaitAndActivateUI(box));
            }
            else
            {
                ActivateUI();
            }

            manager.GetSoul().gameObject.SetActive(false);
        }

        private IEnumerator WaitAndActivateUI(BattleBox box)
        {
            // Esperar a que termine de redimensionarse
            while (box.IsResizing)
            {
                yield return null;
            }
            ActivateUI();
        }

        private void ActivateUI()
        {
            string flavorText = "* El viento aulla...";
            var encounter = manager.GetCurrentEncounter();
            if (encounter != null && encounter.mainMenuMessages.Count > 0)
            {
                flavorText = encounter.mainMenuMessages[Random.Range(0, encounter.mainMenuMessages.Count)];
            }

            if (_canvasController != null)
            {
                _canvasController.battleText.text = flavorText;
                
                // Clear previous listeners to avoid double subscriptions
                _canvasController.OnActionSelected -= HandleMainAction;
                _canvasController.OnEnemySelected -= HandleEnemySelected;
                _canvasController.OnActSelected -= HandleActSelected;
                _canvasController.OnItemSelected -= HandleItemSelected;
                _canvasController.OnCancel -= HandleCancel;

                _canvasController.OnActionSelected += HandleMainAction;
                _canvasController.OnEnemySelected += HandleEnemySelected;
                _canvasController.OnActSelected += HandleActSelected;
                _canvasController.OnItemSelected += HandleItemSelected;
                _canvasController.OnCancel += HandleCancel;
                
                _canvasController.SetNavigationActive(true);
            }
        }

        private void HandleMainAction(ActionType action)
        {
            switch(action)
            {
                case ActionType.FIGHT:
                    _isChoosingAct = false;
                    _canvasController.OpenEnemySelect(manager.GetEnemies());
                    break;
                case ActionType.ACT:
                    _isChoosingAct = true;
                    _canvasController.OpenEnemySelect(manager.GetEnemies());
                    break;
                case ActionType.ITEM:
                    var inventory = manager.GetInventory();
                    if (inventory.Count > 0) _canvasController.OpenItemMenu(inventory);
                    else Debug.Log("Inventario Vacío");
                    break;
                case ActionType.MERCY:
                    manager.ChangeState(BattleState.ENEMY_TURN);
                    break;
            }
        }

        private void HandleEnemySelected(EnemyInstance enemy)
        {
            if (_isChoosingAct)
            {
                _selectedEnemyForAct = enemy;
                _canvasController.OpenActMenu(enemy);
            }
            else
            {
                Debug.Log($"[MenuState] Preparando ataque a: {enemy.enemyName}");
                manager.SelectedEnemy = enemy;
                manager.ChangeState(BattleState.ATTACK_TIMING);
            }
        }

        private void HandleActSelected(string actName)
        {
            Debug.Log($"[MenuState] Actuando en {_selectedEnemyForAct.enemyName}: {actName}");
            var msgState = manager.GetComponentInChildren<MessageState>();
            if (msgState != null)
            {
                msgState.SetMessage($"* Usaste {actName} en {_selectedEnemyForAct.enemyName}.", BattleState.ENEMY_TURN);
                manager.ChangeState(BattleState.MESSAGE);
            }
        }

        private void HandleItemSelected(ItemData item)
        {
            Debug.Log($"[MenuState] Item used: {item.itemName}");
            manager.HealPlayer(item.healAmount);
            manager.RemoveItemFromInventory(item);

            var msgState = manager.GetComponentInChildren<MessageState>();
            if (msgState != null)
            {
                msgState.SetMessage(item.consumeMessage, BattleState.ENEMY_TURN);
                manager.ChangeState(BattleState.MESSAGE);
            }
            else
            {
                manager.ChangeState(BattleState.ENEMY_TURN);
            }
        }

        private void HandleCancel()
        {
            // Si estábamos viendo los Actos y damos cancelar, regresamos a elegir enemigo
            if (_canvasController != null && _isChoosingAct && _selectedEnemyForAct != null)
            {
                _selectedEnemyForAct = null;
                _canvasController.OpenEnemySelect(manager.GetEnemies());
            }
            else
            {
                _canvasController.OpenMainMenu();
            }
        }

        public override void OnUpdate(float delta) { }

        public override void OnExit()
        {
            if (_canvasController != null)
            {
                _canvasController.OnActionSelected -= HandleMainAction;
                _canvasController.OnEnemySelected -= HandleEnemySelected;
                _canvasController.OnActSelected -= HandleActSelected;
                _canvasController.OnItemSelected -= HandleItemSelected;
                _canvasController.OnCancel -= HandleCancel;
                _canvasController.SetNavigationActive(false);
            }
        }
    }
}