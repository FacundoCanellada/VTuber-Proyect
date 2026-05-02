using UnityEngine;
using System.Collections.Generic;

namespace UndertaleEncounter
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Initial Configuration")]
        [SerializeField] private BattleState initialState = BattleState.PLAYER_MENU;
        [SerializeField] private BattleBox battleBox;

        [Header("Player Stats")]
        [SerializeField] private float playerMaxHp = 20.0f;
        [SerializeField] private float playerCurrentHp = 20.0f;
        
        [Header("References")]
        [SerializeField] private SoulController soul;
        [SerializeField] private EncounterData currentEncounter;
        [SerializeField] private BattleCanvasController canvasController;
        [SerializeField] private PlayerData playerData;


        [Header("Scene References")]
        [SerializeField] private Transform enemySpawnParent;
        
        public EnemyInstance SelectedEnemy { get; set; }

        private List<EnemyInstance> _activeEnemies = new List<EnemyInstance>();
        private Dictionary<BattleState, BattleStateBase> _states = new Dictionary<BattleState, BattleStateBase>();
        private BattleStateBase _currentState;
        private BattleState _currentStateEnum;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Sincronizar UI inicial
            if (playerData != null && canvasController != null)
            {
                canvasController.UpdateHP(playerData.currentHp, playerData.maxHp, false);
            }

            // Cargar Enemigos del Encuentro
            SpawnEnemies();

            // Inicializar estados
            foreach (var state in GetComponentsInChildren<BattleStateBase>())
            {
                state.manager = this;
                _states[state.GetStateID()] = state;
            }
            if (_states.ContainsKey(initialState)) ChangeState(initialState);
        }

        private void SpawnEnemies()
        {
            if (currentEncounter == null) return;

            _activeEnemies.Clear();
            foreach (var enemyData in currentEncounter.enemies)
            {
                Debug.Log($"[BattleManager] Aparece: {enemyData.enemyName}");
                _activeEnemies.Add(new EnemyInstance(enemyData));
            }
        }

        private void Update()
        {
            if (_currentState != null) _currentState.OnUpdate(Time.deltaTime);
        }

        public void ChangeState(BattleState newState)
        {
            if (!_states.ContainsKey(newState)) return;

            if (_currentState != null) _currentState.OnExit();

            _currentStateEnum = newState;
            _currentState = _states[newState];
            _currentState.OnEnter();

            Debug.Log("--- Battle State Changed to: " + newState + " ---");
        }

        public void DamagePlayer(float amount)
        {
            playerData.Damage(amount);
            if (canvasController != null) canvasController.UpdateHP(playerData.currentHp, playerData.maxHp);
            if (playerData.currentHp <= 0) Debug.Log("--- GAME OVER ---");
        }

        public void HealPlayer(float amount)
        {
            playerData.Heal(amount);
            if (canvasController != null) canvasController.UpdateHP(playerData.currentHp, playerData.maxHp);
        }

        public SoulController GetSoul() => soul;
        public BattleBox GetBattleBox() => battleBox;
        public BattleCanvasController GetCanvasController() => canvasController;
        public List<ItemData> GetInventory() => playerData.inventory;
        public List<EnemyInstance> GetEnemies() => _activeEnemies;

        public EncounterData GetCurrentEncounter() => currentEncounter;

        public void RemoveItemFromInventory(ItemData item)
        {
            if (playerData != null && playerData.inventory.Contains(item))
            {
                playerData.inventory.Remove(item);
            }
        }

        public Transform GetEnemySpawnParent() => enemySpawnParent;
    }

    public abstract class BattleStateBase : MonoBehaviour
    {
        [HideInInspector] public BattleManager manager;
        public abstract BattleState GetStateID();
        public virtual void OnEnter() { }
        public virtual void OnUpdate(float delta) { }
        public virtual void OnExit() { }
    }
}
