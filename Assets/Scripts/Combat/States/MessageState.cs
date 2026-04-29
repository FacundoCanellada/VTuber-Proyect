using UnityEngine;
using TMPro;

namespace UndertaleEncounter
{
    public class MessageState : BattleStateBase
    {
        public override BattleState GetStateID() => BattleState.MESSAGE;

        private string _message;
        private BattleState _nextState;
        private BattleCanvasController _canvas;
        private float _inputDelayTimer;

        public void SetMessage(string msg, BattleState next)
        {
            _message = msg;
            _nextState = next;
        }

        public override void OnEnter()
        {
            _inputDelayTimer = 0f; // Reiniciamos el timer
            _canvas = manager.GetCanvasController();
            
            if (_canvas != null)
            {
                _canvas.CloseAllMenus();
                _canvas.battleText.gameObject.SetActive(true);
                _canvas.battleText.text = _message;
            }
        }

        public override void OnUpdate(float delta)
        {
            _inputDelayTimer += delta;

            // Esperamos 0.1s antes de permitir que el jugador cierre el texto
            if (_inputDelayTimer > 0.1f && InputReader.Instance != null && InputReader.Instance.UI.Confirm.triggered)
            {
                manager.ChangeState(_nextState);
            }
        }

        public override void OnExit()
        {
            if (_canvas != null)
            {
                _canvas.battleText.gameObject.SetActive(false);
            }
        }
    }
}