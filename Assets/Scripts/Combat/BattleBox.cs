using UnityEngine;
using System.Collections;

namespace UndertaleEncounter
{
    [ExecuteAlways]
    public class BattleBox : MonoBehaviour
    {
        public static BattleBox Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private Vector2 defaultSize = new Vector2(17, 4);
        [SerializeField] private Vector3 defaultPosition = Vector3.zero;
        [SerializeField] private Vector2 boxSize = new Vector2(17, 4); 
        [SerializeField] private float resizeSpeed = 10.0f;
        [SerializeField] private float thickness = 0.05f;

        [Header("2D References")]
        public Transform topWall;
        public Transform bottomWall;
        public Transform leftWall;
        public Transform rightWall;
        public SpriteRenderer border;
        public Transform mask;

        public bool IsResizing => _resizeCoroutine != null;
        private Coroutine _resizeCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                UpdateBoxLayout();
            }
        }

        public void SetTransform(Vector2 newSize, Vector3 newPos, bool animate = true)
        {
            if (!animate)
            {
                boxSize = newSize;
                transform.position = newPos;
                UpdateBoxLayout();
                return;
            }

            if (_resizeCoroutine != null) StopCoroutine(_resizeCoroutine);
            _resizeCoroutine = StartCoroutine(ResizeRoutine(newSize, newPos));
        }

        public void SetSize(Vector2 newSize, bool animate = true)
        {
            SetTransform(newSize, transform.position, animate);
        }

        private IEnumerator ResizeRoutine(Vector2 targetSize, Vector3 targetPos)
        {
            while (Vector2.Distance(boxSize, targetSize) > 0.001f || Vector3.Distance(transform.position, targetPos) > 0.001f)
            {
                boxSize = Vector2.Lerp(boxSize, targetSize, Time.deltaTime * resizeSpeed);
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * resizeSpeed);
                UpdateBoxLayout();
                yield return null;
            }
            boxSize = targetSize;
            transform.position = targetPos;
            UpdateBoxLayout();
            _resizeCoroutine = null;
        }

        private void UpdateBoxLayout()
        {
            if (topWall == null || bottomWall == null || leftWall == null || rightWall == null) return;

            float w = boxSize.x;
            float h = boxSize.y;

            // Get the actual size of the sprite in units
            float spriteUnitWidth = 1f;
            float spriteUnitHeight = 1f;
            if (border != null && border.sprite != null)
            {
                spriteUnitWidth = border.sprite.bounds.size.x;
                spriteUnitHeight = border.sprite.bounds.size.y;
            }

            // Keep Transform scale at "box units" (17, 4) as user expects
            topWall.localScale = new Vector3(w, thickness, 1);
            bottomWall.localScale = new Vector3(w, thickness, 1);
            leftWall.localScale = new Vector3(thickness, h, 1);
            rightWall.localScale = new Vector3(thickness, h, 1);

            // Position walls accounting for sprite unit size (e.g. 17 * 0.64 / 2)
            topWall.localPosition = new Vector3(0, (h / 2f) * spriteUnitHeight, 0);
            bottomWall.localPosition = new Vector3(0, -(h / 2f) * spriteUnitHeight, 0);
            leftWall.localPosition = new Vector3(-(w / 2f) * spriteUnitWidth, 0, 0);
            rightWall.localPosition = new Vector3((w / 2f) * spriteUnitWidth, 0, 0);

            // Adjust colliders to match the visual size in units (localScale * 0.64)
            // Since localScale is w, and we want world size to be w * 0.64, 
            // and world size = localScale * collider.size, then collider.size = 0.64.
            AdjustCollider(topWall, spriteUnitWidth, spriteUnitHeight);
            AdjustCollider(bottomWall, spriteUnitWidth, spriteUnitHeight);
            AdjustCollider(leftWall, spriteUnitWidth, spriteUnitHeight);
            AdjustCollider(rightWall, spriteUnitWidth, spriteUnitHeight);

            // Ensure walls are on the correct layer
            int boxLayer = LayerMask.NameToLayer("BoxBorder");
            if (boxLayer != -1)
            {
                topWall.gameObject.layer = boxLayer;
                bottomWall.gameObject.layer = boxLayer;
                leftWall.gameObject.layer = boxLayer;
                rightWall.gameObject.layer = boxLayer;
            }

            // Border (the black background fill)
            if (border != null)
            {
                border.transform.localScale = new Vector3(w, h, 1);
                border.transform.localPosition = Vector3.zero;
            }

            // Mask (clipping area)
            if (mask != null)
            {
                mask.localScale = new Vector3(w - thickness, h - thickness, 1);
                mask.localPosition = Vector3.zero;
            }
        }

        private void AdjustCollider(Transform wall, float sw, float sh)
        {
            BoxCollider2D col = wall.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(sw, sh);
        }

        public void CenterSoul()
        {
            if (BattleManager.Instance != null)
            {
                SoulController soul = BattleManager.Instance.GetSoul();
                if (soul != null)
                {
                    soul.transform.position = transform.position;
                    
                    // Reset velocity if it has a Rigidbody2D
                    Rigidbody2D rb = soul.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.position = transform.position;
                        rb.linearVelocity = Vector2.zero;
                    }
                }
            }
        }

        private void OnValidate()
        {
            if (topWall != null) UpdateBoxLayout();
        }

        public void ResetToDefault(bool animate = true)
        {
            SetTransform(defaultSize, defaultPosition, animate);
        }

        public Vector2 GetSize() => boxSize;

        public Rect GetInnerBounds()
        {
            float spriteUnitWidth = 1f;
            float spriteUnitHeight = 1f;
            if (border != null && border.sprite != null)
            {
                spriteUnitWidth = border.sprite.bounds.size.x;
                spriteUnitHeight = border.sprite.bounds.size.y;
            }

            float w = (boxSize.x - thickness) * spriteUnitWidth;
            float h = (boxSize.y - thickness) * spriteUnitHeight;
            
            return new Rect(
                transform.position.x - w / 2f,
                transform.position.y - h / 2f,
                w,
                h
            );
        }
    }
}
