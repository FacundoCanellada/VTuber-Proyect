using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace UndertaleEncounter
{
    public class AnimatedBattleButton : BattleButton
    {
        [Header("Animated Features")]
        [Tooltip("The GameObject to hide when the button is focused (e.g., an icon or text).")]
        public GameObject childToHide;
        
        [Tooltip("The global UI heart that will move to the anchor.")]
        public RectTransform globalHeart;
        
        [Tooltip("Duration of the heart movement.")]
        public float moveDuration = 0.15f;
        
        [Tooltip("The DoTween ease type for the movement.")]
        public Ease moveEase = Ease.OutQuad;

        [Header("Layout Spacer Settings")]
        [Tooltip("An optional RectTransform (like an empty image or LayoutElement) placed before the text. Its width will be animated to push the text.")]
        public RectTransform spacerToAnimate;
        public float focusedSpacerWidth = 20f;
        public float shiftDuration = 0.15f;
        
        private float _originalSpacerWidth;
        private LayoutElement _spacerLayoutElement;

        protected override void Awake()
        {
            base.Awake();
            
            if (globalHeart == null)
            {
                GameObject heartObj = GameObject.FindGameObjectWithTag("GlobalHeart");
                if (heartObj != null)
                {
                    globalHeart = heartObj.GetComponent<RectTransform>();
                }
            }

            if (spacerToAnimate != null)
            {
                _spacerLayoutElement = spacerToAnimate.GetComponent<LayoutElement>();
                if (_spacerLayoutElement != null)
                {
                    _originalSpacerWidth = _spacerLayoutElement.minWidth >= 0 ? _spacerLayoutElement.minWidth : 0f;
                }
                else
                {
                    _originalSpacerWidth = spacerToAnimate.sizeDelta.x;
                }
            }
        }

        public override void SetFocused(bool isFocused, bool instant = false)
        {
            if (_buttonImage != null && normalSprite != null && focusedSprite != null)
            {
                _buttonImage.sprite = isFocused ? focusedSprite : normalSprite;
            }

            if (childToHide != null)
            {
                childToHide.SetActive(!isFocused);
            }

            if (globalHeart == null)
            {
                GameObject heartObj = GameObject.FindGameObjectWithTag("GlobalHeart");
                if (heartObj != null) globalHeart = heartObj.GetComponent<RectTransform>();
            }

            if (isFocused && globalHeart != null && heartAnchor != null)
            {
                if (!globalHeart.gameObject.activeSelf)
                {
                    globalHeart.gameObject.SetActive(true);
                }

                globalHeart.DOKill();
                
                if (instant)
                {
                    globalHeart.position = heartAnchor.position;
                }
                else
                {
                    globalHeart.DOMove(heartAnchor.position, moveDuration).SetEase(moveEase);
                }
            }

            if (spacerToAnimate != null)
            {
                float targetWidth = isFocused ? focusedSpacerWidth : _originalSpacerWidth;
                spacerToAnimate.DOKill();
                
                if (instant)
                {
                    if (_spacerLayoutElement != null) _spacerLayoutElement.minWidth = targetWidth;
                    else spacerToAnimate.sizeDelta = new Vector2(targetWidth, spacerToAnimate.sizeDelta.y);
                }
                else
                {
                    if (_spacerLayoutElement != null)
                    {
                        DOTween.To(() => _spacerLayoutElement.minWidth, x => _spacerLayoutElement.minWidth = x, targetWidth, shiftDuration)
                            .SetEase(moveEase)
                            .SetTarget(spacerToAnimate);
                    }
                    else
                    {
                        spacerToAnimate.DOSizeDelta(new Vector2(targetWidth, spacerToAnimate.sizeDelta.y), shiftDuration)
                            .SetEase(moveEase);
                    }
                }
            }
        }
    }
}
