using UnityEngine;
using System.Collections.Generic;

namespace UndertaleEncounter
{
    public class PatternRunner : MonoBehaviour
    {
        public System.Action OnPatternFinished;
        public AttackPatternData patternData;

        private float _patternDuration = 10.0f;
        private float _timer = 0.0f;
        private bool _isFinished = false;

        private class SpawnerState
        {
            public AttackWaveData wave;
            public string state; // "waiting_initial", "spawning", "waiting_burst"
            public float timer;
            public int spawnedInBurst;
            public int burstsDone;
        }

        private List<SpawnerState> _activeSpawners = new List<SpawnerState>();

        public void Initialize(AttackPatternData data)
        {
            patternData = data;
            _patternDuration = patternData.patternDuration;
            
            foreach (var wave in patternData.waves)
            {
                _activeSpawners.Add(new SpawnerState
                {
                    wave = wave,
                    state = "waiting_initial",
                    timer = wave.initialDelay,
                    spawnedInBurst = 0,
                    burstsDone = 0
                });
            }
        }

        private void Update()
        {
            if (_isFinished) return;

            _timer += Time.deltaTime;
            if (_timer >= _patternDuration)
            {
                _isFinished = true;
                OnPatternFinished?.Invoke();
                Destroy(gameObject);
                return;
            }

            // Process spawners
            for (int i = _activeSpawners.Count - 1; i >= 0; i--)
            {
                var spawner = _activeSpawners[i];
                var wave = spawner.wave;

                if (spawner.state == "waiting_initial")
                {
                    spawner.timer -= Time.deltaTime;
                    if (spawner.timer <= 0)
                    {
                        spawner.state = "spawning";
                        spawner.timer = 0;
                    }
                }
                else if (spawner.state == "spawning")
                {
                    spawner.timer -= Time.deltaTime;
                    if (spawner.timer <= 0)
                    {
                        SpawnProjectile(wave);
                        spawner.spawnedInBurst++;

                        if (spawner.spawnedInBurst >= wave.spawnCount)
                        {
                            spawner.burstsDone++;
                            if (spawner.burstsDone >= wave.burstCount)
                            {
                                _activeSpawners.RemoveAt(i);
                                continue;
                            }
                            else
                            {
                                spawner.state = "waiting_burst";
                                spawner.timer = wave.burstInterval;
                                spawner.spawnedInBurst = 0;
                            }
                        }
                        else
                        {
                            spawner.timer = wave.spawnInterval;
                        }
                    }
                }
                else if (spawner.state == "waiting_burst")
                {
                    spawner.timer -= Time.deltaTime;
                    if (spawner.timer <= 0)
                    {
                        spawner.state = "spawning";
                        spawner.timer = 0;
                    }
                }
            }
        }

        private void SpawnProjectile(AttackWaveData wave)
        {
            if (wave.projectilePrefab == null) return;

            Vector2 boxCenter = BattleBox.Instance != null ? (Vector2)BattleBox.Instance.transform.position : Vector2.zero;
            Vector2 spawnPos = wave.fixedPosition;
            bool isOutside = false;

            // Get current box dimensions for relative positioning
            Vector2 bSize = BattleBox.Instance != null ? BattleBox.Instance.GetSize() : new Vector2(4, 4);
            float unitScale = 0.64f; // Standard scale for the sprites used in this project
            float halfW = (bSize.x / 2f) * unitScale;
            float halfH = (bSize.y / 2f) * unitScale;

            switch (wave.spawnType)
            {
                case AttackWaveData.SpawnType.RANDOM_RING:
                    float angle = Random.Range(0, Mathf.PI * 2);
                    spawnPos = wave.fixedPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * wave.spawnRadius;
                    isOutside = true;
                    break;
                case AttackWaveData.SpawnType.RANDOM_BOX_EDGE_TOP:
                    spawnPos = new Vector2(Random.Range(-halfW, halfW), halfH + 0.5f);
                    isOutside = true;
                    break;
                case AttackWaveData.SpawnType.RANDOM_BOX_EDGE_BOTTOM:
                    spawnPos = new Vector2(Random.Range(-halfW, halfW), -halfH - 0.5f);
                    isOutside = true;
                    break;
                case AttackWaveData.SpawnType.RANDOM_BOX_EDGE_LEFT:
                    spawnPos = new Vector2(-halfW - 0.5f, Random.Range(-halfH, halfH));
                    isOutside = true;
                    break;
                case AttackWaveData.SpawnType.RANDOM_BOX_EDGE_RIGHT:
                    spawnPos = new Vector2(halfW + 0.5f, Random.Range(-halfH, halfH));
                    isOutside = true;
                    break;
                case AttackWaveData.SpawnType.RANDOM_BOX_AREA_OUTSIDE:
                    float dist = wave.spawnRadius > 0 ? wave.spawnRadius : 1.5f;
                    int side = Random.Range(0, 4);
                    if (side == 0) spawnPos = new Vector2(Random.Range(-halfW - dist, halfW + dist), halfH + dist);
                    else if (side == 1) spawnPos = new Vector2(Random.Range(-halfW - dist, halfW + dist), -halfH - dist);
                    else if (side == 2) spawnPos = new Vector2(-halfW - dist, Random.Range(-halfH - dist, halfH + dist));
                    else spawnPos = new Vector2(halfW + dist, Random.Range(-halfH - dist, halfH + dist));
                    isOutside = true;
                    break;
            }

            GameObject projObj = Instantiate(wave.projectilePrefab, (Vector3)boxCenter + (Vector3)spawnPos, Quaternion.identity);
            
            // If it's a "local" spawn like Godot's add_child(proj), we parent it
            if (!isOutside)
            {
                projObj.transform.SetParent(transform);
            }

            var proj = projObj.GetComponent<ProjectileBase>();
            if (proj != null)
            {
                proj.InjectData(wave);
            }
        }
    }
}
