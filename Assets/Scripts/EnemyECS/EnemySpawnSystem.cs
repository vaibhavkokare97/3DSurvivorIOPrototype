using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

partial struct EnemySpawnSystem : ISystem
{
    public EntityManager _entityManager;

    private Entity _enemySpawnerEntity;
    private EnemySpawnComponent _enemySpawnComponent;

    private Entity _playerEntity;
    private Random _random;
    private LocalTransform _playerTransform;

    public void OnCreate(ref SystemState state)
    {
        _random = Random.CreateFromIndex((uint)_enemySpawnComponent.GetHashCode());

    }

    public void OnUpdate(ref SystemState state)
    {
        _entityManager = state.EntityManager;
        if (SystemAPI.HasSingleton<PlayerComponent>())
        {
            _playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        }
        else
        {
            return;
        }

        if (SystemAPI.HasSingleton<EnemySpawnComponent>())
        {
            _enemySpawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawnComponent>();
        }
        else
        {
            return;
        }
        _enemySpawnComponent = _entityManager.GetComponentData<EnemySpawnComponent>(_enemySpawnerEntity);

        SpawnEnemies(ref state);
    }

    private void SpawnEnemies(ref SystemState state)
    {
        _enemySpawnComponent.currentTimeBeforeSpawn -= SystemAPI.Time.DeltaTime;
        
        if (_enemySpawnComponent.currentTimeBeforeSpawn <= 0f)
        {
            _playerTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);
            for (int i = 0; i < _enemySpawnComponent.enemiesSpawnCountPerSecond; i++)
            {
                EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);

                Entity enemyEntity = new Entity();
                int randomNum = _random.NextInt(0, 100);
                enemyEntity = _entityManager.Instantiate((randomNum < 15f) ? _enemySpawnComponent.specialEnemyPrefab : _enemySpawnComponent.enemyPrefab);
                LocalTransform enemyTransform = _entityManager.GetComponentData<LocalTransform>(enemyEntity);
                ECB.AddComponent(enemyEntity, new EnemyComponent
                {
                    incrementalCheckForPlayerInterval = 2f,
                    isSpecial = randomNum < 15f,
                    currentHealth = (randomNum < 15f) ? 160f : 100f,
                    speed = (randomNum < 15f) ? 0f : 1f,
                    damage = (randomNum < 15f) ? 1f : 5f
                });

                ECB.AddComponent(enemyEntity, new LifeTimeComponent
                {
                    RemainingLife = 10f
                });

                float minDistanceSquared = _enemySpawnComponent.minDistanceFromPlayer * _enemySpawnComponent.minDistanceFromPlayer;
                float2 randomOffset = _random.NextFloat2Direction() *
                    _random.NextFloat(_enemySpawnComponent.minDistanceFromPlayer, _enemySpawnComponent.enemySpawnRadius);
                float2 playerPosition = new float2(_playerTransform.Position.x, _playerTransform.Position.z);
                float2 spawnPosition = playerPosition + randomOffset;
                float distanceSquared = math.lengthsq(spawnPosition - playerPosition);

                if (distanceSquared < minDistanceSquared)
                {
                    spawnPosition = playerPosition + math.normalize(randomOffset) * math.sqrt(minDistanceSquared);
                }
                enemyTransform.Position = new float3(spawnPosition.x, 0f, spawnPosition.y);

                ECB.SetComponent(enemyEntity, enemyTransform);


                ECB.Playback(_entityManager);
                ECB.Dispose();
            }

            _enemySpawnComponent.enemiesSpawnCountPerSecond = math.min(_enemySpawnComponent.enemiesSpawnCountPerSecond + _enemySpawnComponent.enemiesSpawnIncrementAmount,
                _enemySpawnComponent.maxEnemiesSpawnPerSecond);

            _enemySpawnComponent.currentTimeBeforeSpawn = _enemySpawnComponent.timeBeforeNextSpawn;
        }

        _entityManager.SetComponentData(_enemySpawnerEntity, _enemySpawnComponent);
    }
}
