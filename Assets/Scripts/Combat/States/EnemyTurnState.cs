using UnityEngine;
using System.Collections.Generic;

namespace UndertaleEncounter
{
    public class EnemyTurnState : BattleStateBase
    {
        [SerializeField] private GameObject patternRunnerPrefab;
        
        public override BattleState GetStateID() => BattleState.ENEMY_TURN;

        public override void OnEnter()
        {
            Debug.Log("--- Enemy Turn Started ---");

            // Enable Soul movement inputs
            if (InputReader.Instance != null)
            {
                InputReader.Instance.EnableSoulMode();
            }

            StartEnemyPattern();
        }

        private void StartEnemyPattern()
        {
            if (patternRunnerPrefab == null)
            {
                Debug.LogWarning("[EnemyTurnState] No patternRunnerPrefab assigned.");
                Invoke(nameof(FinishTurn), 2.0f);
                return;
            }

            // Get active enemies and pick one that can attack
            var enemies = manager.GetEnemies();
            var aliveEnemiesWithPatterns = enemies.FindAll(e => e.currentHp > 0 && e.Data != null && e.Data.attackPatterns.Count > 0);

            if (aliveEnemiesWithPatterns.Count == 0)
            {
                Debug.Log("[EnemyTurnState] No enemies available to attack.");
                Invoke(nameof(FinishTurn), 1.0f);
                return;
            }

            // Pick a random enemy and a random pattern
            var selectedEnemy = aliveEnemiesWithPatterns[Random.Range(0, aliveEnemiesWithPatterns.Count)];
            var selectedPattern = selectedEnemy.Data.attackPatterns[Random.Range(0, selectedEnemy.Data.attackPatterns.Count)];

            Debug.Log($"[EnemyTurnState] Running pattern from {selectedEnemy.enemyName}: {selectedPattern.name}");

            // Resize and move the battle box according to the pattern
            var box = manager.GetBattleBox();
            if (box != null)
            {
                box.SetTransform(selectedPattern.targetBoxSize, selectedPattern.targetBoxPosition, true);
            }

            var soul = manager.GetSoul();
            if (soul != null && box != null)
            {
                soul.gameObject.SetActive(true);
                box.CenterSoul();
            }

            Vector3 boxPos = box != null ? box.transform.position : Vector3.zero;
            GameObject runnerObj = Instantiate(patternRunnerPrefab, boxPos, Quaternion.identity);
            var runner = runnerObj.GetComponent<PatternRunner>();
            
            if (runner != null)
            {
                runner.Initialize(selectedPattern);
                runner.OnPatternFinished += FinishTurn;
            }
            else
            {
                Debug.LogError("[EnemyTurnState] PatternRunner component missing on prefab.");
                Invoke(nameof(FinishTurn), 1.0f);
            }
        }

        private void FinishTurn()
        {
            manager.ChangeState(BattleState.PLAYER_MENU);
        }

        public override void OnExit()
        {
            Debug.Log("--- Enemy Turn Finished ---");
            
            var soul = manager.GetSoul();
            if (soul != null)
            {
                soul.gameObject.SetActive(false);
            }

            // Clear projectiles logic
            var projectiles = GameObject.FindGameObjectsWithTag("Projectile");
            foreach (var p in projectiles)
            {
                Destroy(p);
            }
        }
    }
}
