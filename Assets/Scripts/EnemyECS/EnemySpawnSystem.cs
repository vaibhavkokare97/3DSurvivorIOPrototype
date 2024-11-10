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


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _random = Random.CreateFromIndex((uint)_enemySpawnComponent.GetHashCode());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _entityManager = state.EntityManager;

        _enemySpawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawnComponent>();
        _enemySpawnComponent = _entityManager.GetComponentData<EnemySpawnComponent>(_enemySpawnerEntity);

        _playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();

        SpawnEnemies(ref state);
    }

    private void SpawnEnemies(ref SystemState state)
    {
        _enemySpawnComponent.CurrentTimeBeforeSpawn -= SystemAPI.Time.DeltaTime;
        if (_enemySpawnComponent.CurrentTimeBeforeSpawn <= 0f)
        {
            for (int i = 0; i < _enemySpawnComponent.EnemiesSpawnCountPerSecond; i++)
            {
                EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);

                Entity enemyEntity = _entityManager.Instantiate(_enemySpawnComponent.enemyPrefab);

                LocalTransform enemyTransform = _entityManager.GetComponentData<LocalTransform>(enemyEntity);
                LocalTransform playerTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);

                float minDistanceSquared = _enemySpawnComponent.MinDistanceFromPlayer * _enemySpawnComponent.MinDistanceFromPlayer;
                float2 randomOffset = _random.NextFloat2Direction() *
                    _random.NextFloat(_enemySpawnComponent.MinDistanceFromPlayer, _enemySpawnComponent.EnemySpawnRadius);
                float2 playerPosition = new float2(playerTransform.Position.x, playerTransform.Position.z);
                float2 spawnPosition = playerPosition + randomOffset;
                float distanceSquared = math.lengthsq(spawnPosition - playerPosition);

                if (distanceSquared < minDistanceSquared)
                {
                    spawnPosition = playerPosition + math.normalize(randomOffset) * math.sqrt(minDistanceSquared);
                }
                enemyTransform.Position = new float3(spawnPosition.x, 0f, spawnPosition.y);

                ECB.SetComponent(enemyEntity, enemyTransform);
                ECB.AddComponent(enemyEntity, new EnemyComponent
                {
                    CurrentHealth = 100f,
                    Speed = 1f,
                    Damage = 5f
                });

                ECB.Playback(_entityManager);
                ECB.Dispose();
            }

            _enemySpawnComponent.EnemiesSpawnCountPerSecond = math.min(_enemySpawnComponent.EnemiesSpawnCountPerSecond + _enemySpawnComponent.EnemiesSpawnIncrementAmount,
                _enemySpawnComponent.MaxEnemiesSpawnPerSecond);

            _enemySpawnComponent.CurrentTimeBeforeSpawn = _enemySpawnComponent.TimeBeforeNextSpawn;
        }

        _entityManager.SetComponentData(_enemySpawnerEntity, _enemySpawnComponent);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
