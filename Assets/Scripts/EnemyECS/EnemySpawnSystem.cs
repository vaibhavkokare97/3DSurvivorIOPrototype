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


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _random = Random.CreateFromIndex((uint)_enemySpawnComponent.GetHashCode());

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _entityManager = state.EntityManager;
        _playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        _enemySpawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawnComponent>();
        _enemySpawnComponent = _entityManager.GetComponentData<EnemySpawnComponent>(_enemySpawnerEntity);

        SpawnEnemies(ref state);
    }

    private void SpawnEnemies(ref SystemState state)
    {
        _enemySpawnComponent.CurrentTimeBeforeSpawn -= SystemAPI.Time.DeltaTime;
        
        if (_enemySpawnComponent.CurrentTimeBeforeSpawn <= 0f)
        {
            _playerTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);
            for (int i = 0; i < _enemySpawnComponent.EnemiesSpawnCountPerSecond; i++)
            {
                EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);

                Entity enemyEntity = new Entity();
                int randomNum = _random.NextInt(0, 100);
                enemyEntity = _entityManager.Instantiate((randomNum < 15f) ? _enemySpawnComponent.specialEnemyPrefab : _enemySpawnComponent.enemyPrefab);
                LocalTransform enemyTransform = _entityManager.GetComponentData<LocalTransform>(enemyEntity);
                ECB.AddComponent(enemyEntity, new EnemyComponent
                {
                    incrementalCheckForPlayerInterval = 0f,
                    IsSpecial = randomNum < 15f,
                    CurrentHealth = (randomNum < 15f) ? 160f : 100f,
                    Speed = (randomNum < 15f) ? 0f : 1f,
                    Damage = (randomNum < 15f) ? 1f : 5f
                });

                float minDistanceSquared = _enemySpawnComponent.MinDistanceFromPlayer * _enemySpawnComponent.MinDistanceFromPlayer;
                float2 randomOffset = _random.NextFloat2Direction() *
                    _random.NextFloat(_enemySpawnComponent.MinDistanceFromPlayer, _enemySpawnComponent.EnemySpawnRadius);
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
