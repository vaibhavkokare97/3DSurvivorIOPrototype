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
        _enemySpawnComponent = _entityManager.GetComponentData<EnemySpawnComponent>(_playerEntity);

        MoveAndAttack(ref state);
    }

    private void MoveAndAttack(ref SystemState state)
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

                enemyTransform.Position += enemyComponent.Speed * moveDirection * SystemAPI.Time.DeltaTime;

                if (enemyComponent.IsSpecial)
                {
                    NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);

                    physicsWorld.SphereCastAll(new float3(_playerTransform.Position), 10f, float3.zero, 4f, ref hits, new CollisionFilter
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
                                float3 direction = math.normalize(enemyTransform.Position - hitEntityTransform.Position);
                                ECB.AddComponent(bulletEntity, new BulletComponent
                                {
                                    Speed = 25f,
                                    Damage = 10f,
                                    DirectionX = direction.x,
                                    DirectionZ = direction.z
                                });

                                ECB.AddComponent(bulletEntity, new BulletLifeTimeComponent
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
                    }
                        
                }

                _entityManager.SetComponentData(enemy, enemyTransform);
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
