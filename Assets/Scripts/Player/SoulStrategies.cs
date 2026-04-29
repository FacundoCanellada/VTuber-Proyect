using UnityEngine;

namespace UndertaleEncounter
{
    public abstract class SoulModeStrategy
    {
        protected SoulController soul;

        public SoulModeStrategy(SoulController soul)
        {
            this.soul = soul;
        }

        public abstract Vector2 ProcessMovement(float delta);
        public abstract Color GetColor();
    }

    public class RedMovementStrategy : SoulModeStrategy
    {
        public float moveSpeed = 3.0f; // Adjusted for Unity units (100px = 1m)

        public RedMovementStrategy(SoulController soul) : base(soul) { }

        public override Vector2 ProcessMovement(float delta)
        {
            if (InputReader.Instance == null) return Vector2.zero;

            // Reading Vector2 from Combat.Move (Specialized for combat movement)
            Vector2 inputDir = InputReader.Instance.Combat.Move.ReadValue<Vector2>();

            // Small deadzone for stick support if needed
            if (inputDir.magnitude < 0.1f) return Vector2.zero;

            return inputDir * moveSpeed;
        }

        public override Color GetColor() => Color.red;
    }
}
