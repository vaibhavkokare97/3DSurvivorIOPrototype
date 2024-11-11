using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics;

partial struct EnemySystem : ISystem
{
    private EntityManager _entityManager;

    private Entity _playerEntity;
    private LocalTransform _playerTransform;

    private Entity _enemySpawnEntity;
    private EnemySpawnComponent _enemySpawnComponent;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _entityManager = state.EntityManager;
        _playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        _playerTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);
        _enemySpawnEntity = SystemAPI.GetSingletonEntity<EnemySpawnComponent>();
        _enemySpawnComponent = _entityManager.GetComponentData<EnemySpawnComponent>(_enemySpawnEntity);

        UpdateEnemies(ref state);
    }

    private void UpdateEnemies(ref SystemState state)
    {
        NativeArray<Entity> allEnemies = _entityManager.GetAllEntities();
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (Entity enemy in allEnemies)
        {
            if (_entityManager.HasComponent<EnemyComponent>(enemy))
            {
                //move
                LocalTransform enemyTransform = _entityManager.GetComponentData<LocalTransform>(enemy);
                EnemyComponent enemyComponent = _entityManager.GetComponentData<EnemyComponent>(enemy);

                float3 moveDirection = math.normalize(_playerTransform.Position - enemyTransform.Position);

                enemyTransform.Position += enemyComponent.speed * moveDirection * SystemAPI.Time.DeltaTime;

                enemyComponent.incrementalCheckForPlayerInterval += SystemAPI.Time.DeltaTime;
                if (enemyComponent.isSpecial && enemyComponent.incrementalCheckForPlayerInterval > 4f)
                {
                    NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);

                    physicsWorld.SphereCastAll(new float3(enemyTransform.Position), 11f, float3.zero, 4f, ref hits, new CollisionFilter
                    {
                        BelongsTo = (uint)1 << 7,
                        CollidesWith = (uint)1 << 6
                    });

                    if (hits.Length > 0)
                    {
                        for (int i = 0; i < hits.Length; i++)
                        {
                            Entity hitEntity = hits[i].Entity;
                            if (_entityManager.HasComponent<PlayerComponent>(hitEntity))
                            {
                                EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);
                                Entity bulletEntity = _entityManager.Instantiate(_enemySpawnComponent.enemyBulletPrefab);

                                LocalTransform hitEntityTransform = _entityManager.GetComponentData<LocalTransform>(hitEntity);
                                float3 direction = math.normalize(hitEntityTransform.Position - enemyTransform.Position);
                                ECB.AddComponent(bulletEntity, new BulletComponent
                                {
                                    speed = 10f,
                                    damage = 2f,
                                    directionX = direction.x,
                                    directionZ = direction.z
                                });

                                ECB.AddComponent(bulletEntity, new LifeTimeComponent
                                {
                                    RemainingLife = 3f
                                });

                                LocalTransform bulletTransform = _entityManager.GetComponentData<LocalTransform>(bulletEntity);
                                bulletTransform.Position = enemyTransform.Position + direction * 1f;


                                ECB.SetComponent(bulletEntity, bulletTransform);
                                ECB.Playback(_entityManager);
                                ECB.Dispose();
                                break;
                            }
                        }
                        enemyComponent.incrementalCheckForPlayerInterval = 0f;
                    }
                    hits.Dispose();
                }


                _entityManager.SetComponentData(enemy, enemyTransform);
                _entityManager.SetComponentData(enemy, enemyComponent);
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
