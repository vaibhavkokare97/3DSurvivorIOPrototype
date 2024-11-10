using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Physics;

partial struct BulletSystem : ISystem
{
    private Entity _enemySpawnEntity;
    private EnemySpawnComponent _enemySpawnComponent;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        NativeArray<Entity> allEntities = entityManager.GetAllEntities();
        _enemySpawnEntity = SystemAPI.GetSingletonEntity<EnemySpawnComponent>();
        _enemySpawnComponent = entityManager.GetComponentData<EnemySpawnComponent>(_enemySpawnEntity);

        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (Entity entity in allEntities)
        {
            if (entityManager.HasComponent<BulletComponent>(entity) && entityManager.HasComponent<BulletLifeTimeComponent>(entity))
            {
                LocalTransform bulletTransform = entityManager.GetComponentData<LocalTransform>(entity);
                BulletComponent bulletComponent = entityManager.GetComponentData<BulletComponent>(entity);

                bulletTransform.Position += new float3(bulletComponent.DirectionX, 0f, bulletComponent.DirectionZ) *
                    bulletComponent.Speed * SystemAPI.Time.DeltaTime;
                entityManager.SetComponentData(entity, bulletTransform);

                BulletLifeTimeComponent bulletLifeTimeComponent = entityManager.GetComponentData<BulletLifeTimeComponent>(entity);
                bulletLifeTimeComponent.RemainingLife -= SystemAPI.Time.DeltaTime;

                if (bulletLifeTimeComponent.RemainingLife <= 0f)
                {
                    entityManager.DestroyEntity(entity);
                    continue;
                }

                entityManager.SetComponentData(entity, bulletLifeTimeComponent);

                NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);

                physicsWorld.SphereCastAll(new float3(bulletTransform.Position), bulletTransform.Scale, float3.zero, 1f, ref hits, new CollisionFilter
                {
                    BelongsTo = (uint)1 << 8,
                    CollidesWith = (uint)1 << 7
                });


                if (hits.Length > 0)
                {
                    for (int i = 0; i < hits.Length; i++)
                    {
                        Entity hitEntity = hits[i].Entity;

                        if (entityManager.HasComponent<EnemyComponent>(hitEntity))
                        {
                            EnemyComponent enemyComponent = entityManager.GetComponentData<EnemyComponent>(hitEntity);
                            LocalTransform enemyTransform = entityManager.GetComponentData<LocalTransform>(hitEntity);
                            enemyComponent.CurrentHealth -= bulletComponent.Damage;
                            entityManager.SetComponentData(hitEntity, enemyComponent);

                            if (enemyComponent.CurrentHealth <= 0f)
                            {
                                Random _random = Random.CreateFromIndex((uint)_enemySpawnComponent.GetHashCode());
                                int randomNum = _random.NextInt(0, 100);

                                EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);
                                if (randomNum > 30)
                                {

                                    //CoinComponent coinComponent = entityManager.GetComponentData<CoinComponent>();
                                    Entity coinEntity = entityManager.Instantiate(_enemySpawnComponent.coinPrefab);
                                    ECB.AddComponent(coinEntity, new CoinComponent
                                    {
                                        coinIncrementValue = 1
                                    });

                                    LocalTransform coinTransform = entityManager.GetComponentData<LocalTransform>(coinEntity);
                                    coinTransform.Position = enemyTransform.Position;

                                    ECB.SetComponent(coinEntity, coinTransform);

                                }
                                else
                                {
                                    //CoinComponent coinComponent = entityManager.GetComponentData<CoinComponent>();
                                    Entity healthEntity = entityManager.Instantiate(_enemySpawnComponent.healthPrefab);
                                    ECB.AddComponent(healthEntity, new HealthPackComponent
                                    {
                                        healthIncrementValue = 10f
                                    });

                                    LocalTransform healthPackTransform = entityManager.GetComponentData<LocalTransform>(healthEntity);
                                    healthPackTransform.Position = enemyTransform.Position;

                                    ECB.SetComponent(healthEntity, healthPackTransform);
                                }

                                ECB.Playback(entityManager);
                                ECB.Dispose();

                                entityManager.DestroyEntity(hitEntity);

                            }
                        }
                    }
                    entityManager.DestroyEntity(entity);
                }

                hits.Dispose();
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
