using UnityEngine;
using UnityEngine.UI;

namespace UndertaleEncounter
{
    public class BattleButton : MonoBehaviour
    {
        [Header("Sprites")]
        public Sprite normalSprite;
        public Sprite focusedSprite;

        [Header("Heart Position")]
        public RectTransform heartAnchor; // The "heart child" empty or icon

        protected Image _buttonImage;

        protected virtual void Awake()
        {
            _buttonImage = GetComponent<Image>();
        }

        public virtual void SetFocused(bool isFocused, bool instant = false)
        {
            if (_buttonImage != null)
            {
                _buttonImage.sprite = isFocused ? focusedSprite : normalSprite;
                Debug.Log($"[BattleButton] {gameObject.name} focused: {isFocused}. Sprite set to: {(_buttonImage.sprite != null ? _buttonImage.sprite.name : "null")}");
            }

            if (heartAnchor != null)
            {
                heartAnchor.gameObject.SetActive(isFocused);
            }
        }
    }
}
