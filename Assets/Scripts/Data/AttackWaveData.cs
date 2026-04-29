using UnityEngine;

namespace UndertaleEncounter
{
    [CreateAssetMenu(fileName = "NewAttackWave", menuName = "Undertale/Attack Wave")]
    public class AttackWaveData : ScriptableObject
    {
        public enum SpawnType { FIXED_POSITION, RANDOM_RING, RANDOM_BOX_EDGE_TOP, RANDOM_BOX_EDGE_BOTTOM, RANDOM_BOX_EDGE_LEFT, RANDOM_BOX_EDGE_RIGHT, RANDOM_BOX_AREA_OUTSIDE }
        public enum SpawnAnim { NONE, FADE_IN, FADE_IN_AND_SPIN }
        public enum WaitBehavior { DO_NOTHING, LOOK_AT_SOUL, LOOK_AT_CENTER }
        public enum AimType { FIXED_DIRECTION, AIM_AT_SOUL, AIM_AT_CENTER }
        public enum MoveType { LINEAR, HOMING, SINE_WAVE, BOOMERANG }

        [Header("1. Identity")]
        public GameObject projectilePrefab;

        [Header("2. Generation Rules")]
        public float initialDelay = 0.0f;
        public int spawnCount = 1;
        public float spawnInterval = 0.0f;
        public int burstCount = 1;
        public float burstInterval = 0.0f;

        [Header("3. Appearance")]
        public SpawnType spawnType = SpawnType.FIXED_POSITION;
        public Vector2 fixedPosition = Vector2.zero;
        public float spawnRadius = 4.0f; // Unity meters
        public SpawnAnim spawnAnim = SpawnAnim.NONE;
        public float spawnAnimDuration = 0.4f;
        public int spawnExtraSpins = 2;

        [Header("4. Wait Behavior")]
        public float waitDuration = 0.0f;
        public WaitBehavior waitBehavior = WaitBehavior.DO_NOTHING;

        [Header("5. Warning")]
        public float warningDuration = 0.0f;
        public Color warningColor1 = new Color(1.0f, 0.5f, 0.0f);
        public Color warningColor2 = new Color(0.0f, 1.0f, 1.0f);

        [Header("6. Shooting and Movement")]
        public AimType aimType = AimType.FIXED_DIRECTION;
        public Vector2 fixedDirection = Vector2.right;
        public MoveType moveType = MoveType.LINEAR;
        public float moveSpeed = 3.0f; // Unity meters/sec
        public bool rotateTowardsVelocity = true;

        [Header("7. Advanced Movement")]
        public float homingTurnSpeed = 5.0f;
        public float sineAmplitude = 0.5f;
        public float sineFrequency = 5.0f;
        public float boomerangAcceleration = 8.0f;

        [Header("8. Sounds")]
        public SoundData spawnSound;
        public SoundData warningSound;
        public SoundData shootSound;
    }
}
