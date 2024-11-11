using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Physics;

partial struct BulletSystem : ISystem
{
    private EntityManager _entityManager;
    private Entity _enemySpawnEntity;
    private EnemySpawnComponent _enemySpawnComponent;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    public void OnUpdate(ref SystemState state)
    {
        _entityManager = state.EntityManager;
        NativeArray<Entity> allEntities = _entityManager.GetAllEntities();
        _enemySpawnEntity = SystemAPI.GetSingletonEntity<EnemySpawnComponent>();
        _enemySpawnComponent = _entityManager.GetComponentData<EnemySpawnComponent>(_enemySpawnEntity);

        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (Entity entity in allEntities)
        {
            if (_entityManager.HasComponent<BulletComponent>(entity))
            {
                LocalTransform bulletTransform = _entityManager.GetComponentData<LocalTransform>(entity);
                BulletComponent bulletComponent = _entityManager.GetComponentData<BulletComponent>(entity);

                bulletTransform.Position += new float3(bulletComponent.directionX, 0f, bulletComponent.directionZ) *
                    bulletComponent.speed * SystemAPI.Time.DeltaTime;
                _entityManager.SetComponentData(entity, bulletTransform);

                if (_entityManager.HasComponent<IsEnemyItem>(entity))
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

                            if (_entityManager.HasComponent<EnemyComponent>(hitEntity))
                            {
                                EnemyComponent enemyComponent = _entityManager.GetComponentData<EnemyComponent>(hitEntity);
                                LocalTransform enemyTransform = _entityManager.GetComponentData<LocalTransform>(hitEntity);
                                enemyComponent.currentHealth -= bulletComponent.damage;
                                _entityManager.SetComponentData(hitEntity, enemyComponent);

                                if (enemyComponent.currentHealth <= 0f)
                                {
                                    Random _random = Random.CreateFromIndex((uint)_enemySpawnComponent.GetHashCode());
                                    int randomNum = _random.NextInt(0, 100);

                                    EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);
                                    if (randomNum > 30)
                                    {

                                        //CoinComponent coinComponent = entityManager.GetComponentData<CoinComponent>();
                                        Entity coinEntity = _entityManager.Instantiate(_enemySpawnComponent.coinPrefab);
                                        ECB.AddComponent(coinEntity, new CoinComponent
                                        {
                                            coinIncrementValue = 1
                                        });

                                        ECB.AddComponent(coinEntity, new LifeTimeComponent
                                        {
                                            RemainingLife = 30f
                                        });

                                        LocalTransform coinTransform = _entityManager.GetComponentData<LocalTransform>(coinEntity);
                                        coinTransform.Position = enemyTransform.Position;

                                        ECB.SetComponent(coinEntity, coinTransform);

                                    }
                                    else
                                    {
                                        //CoinComponent coinComponent = entityManager.GetComponentData<CoinComponent>();
                                        Entity healthEntity = _entityManager.Instantiate(_enemySpawnComponent.healthPrefab);
                                        ECB.AddComponent(healthEntity, new HealthPackComponent
                                        {
                                            healthIncrementValue = 10f
                                        });
                                        ECB.AddComponent(healthEntity, new LifeTimeComponent
                                        {
                                            RemainingLife = 30f
                                        });

                                        LocalTransform healthPackTransform = _entityManager.GetComponentData<LocalTransform>(healthEntity);
                                        healthPackTransform.Position = enemyTransform.Position;

                                        ECB.SetComponent(healthEntity, healthPackTransform);
                                    }

                                    ECB.Playback(_entityManager);
                                    ECB.Dispose();

                                    _entityManager.DestroyEntity(hitEntity);

                                }
                            }
                        }
                        _entityManager.DestroyEntity(entity);
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
