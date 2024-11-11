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
            if (entityManager.HasComponent<BulletComponent>(entity))
            {
                LocalTransform bulletTransform = entityManager.GetComponentData<LocalTransform>(entity);
                BulletComponent bulletComponent = entityManager.GetComponentData<BulletComponent>(entity);

                bulletTransform.Position += new float3(bulletComponent.DirectionX, 0f, bulletComponent.DirectionZ) *
                    bulletComponent.Speed * SystemAPI.Time.DeltaTime;
                entityManager.SetComponentData(entity, bulletTransform);

                if (entityManager.HasComponent<IsEnemyItem>(entity))
                {
                    NativeList<ColliderCastHit> hitsPlayer = new NativeList<ColliderCastHit>(Allocator.Temp);
                    physicsWorld.SphereCastAll(new float3(bulletTransform.Position), bulletTransform.Scale, float3.zero, 1f, ref hitsPlayer, new CollisionFilter
                    {
                        BelongsTo = (uint)1 << 9,
                        CollidesWith = (uint)1 << 6
                    });
                    hitsPlayer.Dispose();
                }
                else
                {
                    NativeList<ColliderCastHit> hitsEnemy = new NativeList<ColliderCastHit>(Allocator.Temp);

                    physicsWorld.SphereCastAll(new float3(bulletTransform.Position), bulletTransform.Scale, float3.zero, 1f, ref hitsEnemy, new CollisionFilter
                    {
                        BelongsTo = (uint)1 << 8,
                        CollidesWith = (uint)1 << 7
                    });

                    if (hitsEnemy.Length > 0)
                    {
                        for (int i = 0; i < hitsEnemy.Length; i++)
                        {
                            Entity hitEntity = hitsEnemy[i].Entity;

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

                                        ECB.AddComponent(coinEntity, new LifeTimeComponent
                                        {
                                            RemainingLife = 30f
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
                                        ECB.AddComponent(healthEntity, new LifeTimeComponent
                                        {
                                            RemainingLife = 30f
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
                    hitsEnemy.Dispose();
                }
                

                


                

                
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
