using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace UndertaleEncounter
{
    public enum MenuState { MAIN, ENEMY_SELECT, ACT_SELECT, GRID_MENU, DIALOGUE_ONLY }

    public class BattleCanvasController : MonoBehaviour
    {
        [Header("Main Containers")]
        public GameObject actionButtonsContainer;
        public GameObject menuBox;
        
        [Header("Sub-Menu Layouts")]
        public Transform vSelectContainer;
        public Transform gridSelectContainer;
        public TextMeshProUGUI battleText;

        [Header("Prefabs")]
        public GameObject enemySelectPrefab;
        public GameObject actPrefab;
        public GameObject itemPrefab;

        [Header("Stats UI")]
        public TextMeshProUGUI nameText; // Nuevo para PlayerName y LV
        public Image hpBarImage;         // Cambiado a Image para usar el Shader
        public TextMeshProUGUI currentHpText;
        public TextMeshProUGUI maxHpText;

        private List<BattleButton> _mainButtons = new List<BattleButton>();
        private List<BattleButton> _currentMenuButtons = new List<BattleButton>();
        
        // Data mapping for sub-menus
        private List<ItemData> _currentItems = new List<ItemData>();
        private List<EnemyInstance> _currentEnemies = new List<EnemyInstance>();
        private List<string> _currentActs = new List<string>();

        private int _currentIndex = 0;
        private int _savedMainIndex = 0;
        private MenuState _state = MenuState.DIALOGUE_ONLY;
        private int _gridColumns = 2;
        
        private Material _hpMaterialInstance;
        private Coroutine _hpCoroutine;

        public System.Action<ActionType> OnActionSelected;
        public System.Action<EnemyInstance> OnEnemySelected;
        public System.Action<string> OnActSelected;
        public System.Action<ItemData> OnItemSelected;
        public System.Action OnCancel;

        private void Awake()
        {
            foreach (Transform child in actionButtonsContainer.transform)
            {
                var b = child.GetComponent<BattleButton>();
                if (b != null) _mainButtons.Add(b);
            }

            if (hpBarImage != null)
            {
                _hpMaterialInstance = new Material(hpBarImage.material);
                hpBarImage.material = _hpMaterialInstance;
            }
        }

        public void InitUI(PlayerData data)
        {
            if (nameText != null) nameText.text = $"{data.playerName}   LV {data.level}";
            UpdateHP(data.currentHp, data.maxHp, false);
        }

        public void SetNavigationActive(bool active)
        {
            if (active) OpenMainMenu();
            else CloseAllMenus();
        }

        private void Update()
        {
            if (_state == MenuState.DIALOGUE_ONLY || InputReader.Instance == null) return;

            HandleNavigationInput();
            HandleConfirmationInput();
            HandleCancelInput();
        }

        #region Menu Management

        public void OpenMainMenu()
        {
            actionButtonsContainer.SetActive(true);
            vSelectContainer.gameObject.SetActive(false);
            gridSelectContainer.gameObject.SetActive(false);
            if (battleText != null) battleText.gameObject.SetActive(true);
            
            _state = MenuState.MAIN;
            _currentMenuButtons = _mainButtons;
            
            UpdateSelection(_savedMainIndex);
        }

        public void OpenEnemySelect(List<EnemyInstance> enemies)
        {
            if (_state == MenuState.MAIN) _savedMainIndex = _currentIndex;
            _currentEnemies = enemies;
            
            // Usamos una versión personalizada de PrepareSubMenu o la modificamos
            PrepareEnemySelectMenu(enemies);
        }

        private void PrepareEnemySelectMenu(List<EnemyInstance> enemies)
        {
            foreach (var b in _mainButtons) b.SetFocused(false);

            vSelectContainer.gameObject.SetActive(true);
            gridSelectContainer.gameObject.SetActive(false);
            if (battleText != null) battleText.gameObject.SetActive(false);

            _state = MenuState.ENEMY_SELECT;
            
            foreach (Transform child in vSelectContainer) Destroy(child.gameObject);
            _currentMenuButtons = new List<BattleButton>();

            foreach (var enemy in enemies)
            {
                GameObject obj = Instantiate(enemySelectPrefab, vSelectContainer);
                var selectBtn = obj.GetComponent<EnemySelectButton>();
                if (selectBtn != null)
                {
                    selectBtn.Setup(enemy);
                    _currentMenuButtons.Add(selectBtn);
                }
                else
                {
                    // Fallback for generic buttons
                    var bbtn = obj.GetComponent<BattleButton>();
                    var txt = obj.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null) txt.text = "* " + enemy.enemyName;
                    if (bbtn != null) _currentMenuButtons.Add(bbtn);
                }
            }

            UpdateSelection(0);
        }

        public void OpenActMenu(EnemyInstance enemy)
        {
            _savedMainIndex = _currentIndex;
            _currentActs = enemy.Data.availableActs;
            PrepareSubMenu(MenuState.ACT_SELECT, vSelectContainer, actPrefab, _currentActs.ConvertAll(a => "* " + a));
        }

        public void OpenItemMenu(List<ItemData> items)
        {
            _savedMainIndex = _currentIndex;
            _currentItems = items;
            PrepareSubMenu(MenuState.GRID_MENU, gridSelectContainer, itemPrefab, items.ConvertAll(i => "* " + i.itemName));
        }

        private void PrepareSubMenu(MenuState nextState, Transform container, GameObject prefab, List<string> labels)
        {
            foreach (var b in _mainButtons) b.SetFocused(false);

            vSelectContainer.gameObject.SetActive(false);
            gridSelectContainer.gameObject.SetActive(false);
            if (battleText != null) battleText.gameObject.SetActive(false);

            _state = nextState;
            container.gameObject.SetActive(true);
            
            foreach (Transform child in container) Destroy(child.gameObject);
            _currentMenuButtons = new List<BattleButton>();

            foreach (var label in labels)
            {
                GameObject obj = Instantiate(prefab, container);
                var bbtn = obj.GetComponent<BattleButton>();
                var txt = obj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = label;
                if (bbtn != null) _currentMenuButtons.Add(bbtn);
            }

            UpdateSelection(0);
        }

        public void CloseAllMenus()
        {
            // actionButtonsContainer stays active as per user request
            vSelectContainer.gameObject.SetActive(false);
            gridSelectContainer.gameObject.SetActive(false);
            foreach (var b in _mainButtons) b.SetFocused(false);
            _state = MenuState.DIALOGUE_ONLY;
        }

        #endregion

        #region Input Handling

        private void HandleNavigationInput()
        {
            var navAction = InputReader.Instance.UI.Navigate;
            if (!navAction.triggered) return;

            Vector2 val = navAction.ReadValue<Vector2>();
            int dir = 0;

            if (_state == MenuState.MAIN)
            {
                if (Mathf.Abs(val.x) > 0.5f) dir = val.x > 0 ? 1 : -1;
            }
            else if (_state == MenuState.ENEMY_SELECT || _state == MenuState.ACT_SELECT)
            {
                if (Mathf.Abs(val.y) > 0.5f) dir = val.y > 0 ? -1 : 1;
            }
            else if (_state == MenuState.GRID_MENU)
            {
                if (Mathf.Abs(val.x) > 0.5f) dir = val.x > 0 ? 1 : -1;
                if (Mathf.Abs(val.y) > 0.5f) dir = val.y > 0 ? -_gridColumns : _gridColumns;
            }

            if (dir != 0)
            {
                int next = Mathf.Clamp(_currentIndex + dir, 0, _currentMenuButtons.Count - 1);
                if (next != _currentIndex) UpdateSelection(next);
            }
        }

        private void HandleConfirmationInput()
        {
            if (!InputReader.Instance.UI.Confirm.triggered) return;

            if (_state == MenuState.MAIN)
            {
                OnActionSelected?.Invoke((ActionType)_currentIndex);
            }
            else if (_state == MenuState.ENEMY_SELECT)
            {
                if (_currentIndex < _currentEnemies.Count)
                    OnEnemySelected?.Invoke(_currentEnemies[_currentIndex]);
            }
            else if (_state == MenuState.ACT_SELECT)
            {
                if (_currentIndex < _currentActs.Count)
                    OnActSelected?.Invoke(_currentActs[_currentIndex]);
            }
            else if (_state == MenuState.GRID_MENU)
            {
                if (_currentIndex < _currentItems.Count)
                    OnItemSelected?.Invoke(_currentItems[_currentIndex]);
            }
        }

        private void HandleCancelInput()
        {
            if (InputReader.Instance.UI.Cancel.triggered)
            {
                if (_state != MenuState.MAIN)
                {
                    OnCancel?.Invoke();
                }
            }
        }

        #endregion

        private void UpdateSelection(int index)
        {
            _currentIndex = index;
            for (int i = 0; i < _currentMenuButtons.Count; i++)
            {
                _currentMenuButtons[i].SetFocused(i == index);
            }
        }

        public void UpdateHP(float current, float max, bool animate = true)
        {
            if (currentHpText) currentHpText.text = Mathf.CeilToInt(current).ToString();
            if (maxHpText) maxHpText.text = Mathf.CeilToInt(max).ToString();

            float targetFill = Mathf.Clamp01(current / max);

            if (!animate || _hpMaterialInstance == null)
            {
                if (_hpMaterialInstance != null) _hpMaterialInstance.SetFloat("_health", targetFill);
                return;
            }

            if (_hpCoroutine != null) StopCoroutine(_hpCoroutine);
            _hpCoroutine = StartCoroutine(AnimateHPRoutine(targetFill));
        }

        private IEnumerator AnimateHPRoutine(float targetFill)
        {
            float currentFill = _hpMaterialInstance.GetFloat("_health");
            float elapsed = 0f;
            float duration = 0.3f; // Duración suave del tween

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float newFill = Mathf.Lerp(currentFill, targetFill, elapsed / duration);
                _hpMaterialInstance.SetFloat("_health", newFill);
                yield return null;
            }
            
            _hpMaterialInstance.SetFloat("_health", targetFill);
        }
    }
}