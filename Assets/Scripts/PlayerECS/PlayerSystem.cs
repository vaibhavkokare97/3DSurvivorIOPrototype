using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics;

public partial struct PlayerSystem : ISystem
{
    private EntityManager _entityManager;

    private Entity _playerEntity;
    private PlayerComponent _playerComponent;
    private LocalTransform _playerTransform;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _entityManager = state.EntityManager;
        _playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        _playerComponent = _entityManager.GetComponentData<PlayerComponent>(_playerEntity);
        _playerTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);

        Move(ref state);
        Attack(ref state);
        OnInteract(ref state);
    }

    private void Attack(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        _playerComponent.incrementalCheckForEnemyInterval += SystemAPI.Time.DeltaTime;

        if (_playerComponent.incrementalCheckForEnemyInterval > 1f)
        {
            NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);

            physicsWorld.SphereCastAll(new float3(_playerTransform.Position), 10f, float3.zero, 4f, ref hits, new CollisionFilter
            {
                BelongsTo = (uint)1 << 6,
                CollidesWith = (uint)1 << 7 | (uint)1 << 9
            });


            if (hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    Entity hitEntity = hits[i].Entity;
                    if (_entityManager.HasComponent<EnemyComponent>(hitEntity))
                    {
                        EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);
                        Entity bulletEntity = _entityManager.Instantiate(_playerComponent.bulletPrefab);

                        LocalTransform hitEntityTransform = _entityManager.GetComponentData<LocalTransform>(hitEntity);
                        float3 direction = math.normalize(hitEntityTransform.Position - _playerTransform.Position);
                        ECB.AddComponent(bulletEntity, new BulletComponent
                        {
                            speed = 25f,
                            damage = 110f,
                            directionX = direction.x,
                            directionZ = direction.z
                        });

                        ECB.AddComponent(bulletEntity, new LifeTimeComponent
                        {
                            RemainingLife = 3f
                        });

                        LocalTransform bulletTransform = _entityManager.GetComponentData<LocalTransform>(bulletEntity);
                        bulletTransform.Position = _playerTransform.Position + direction * 1f;


                        ECB.SetComponent(bulletEntity, bulletTransform);
                        ECB.Playback(_entityManager);
                        ECB.Dispose();
                        break;
                    }
                }

                _playerComponent.incrementalCheckForEnemyInterval = 0f;
            }

            hits.Dispose();
        }
        _entityManager.SetComponentData(_playerEntity, _playerComponent);
    }

    private void Move(ref SystemState state)
    {
        foreach (var gameManagerRef in SystemAPI.Query<GameMangerRef>())
        {
            /*if (gameManagerRef.Value != null)
            {
                break;
            }*/
            _playerTransform.Position += new float3(gameManagerRef.Value.Value.joystick.Horizontal * SystemAPI.Time.DeltaTime,
                0,
                gameManagerRef.Value.Value.joystick.Vertical * SystemAPI.Time.DeltaTime) * _playerComponent.moveSpeed;

            gameManagerRef.Value.Value.playerPosition = _playerTransform.Position;
            gameManagerRef.Value.Value.healthCount = _playerComponent.currentHealth;
            gameManagerRef.Value.Value.coinCount = _playerComponent.coinCount;

            _entityManager.SetComponentData(_playerEntity, _playerTransform);
        }
    }

    public void OnInteract(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);
        float3 p1 = new float3(_playerTransform.Position - _playerTransform.Up() * 0.3f);
        float3 p2 = new float3(_playerTransform.Position + _playerTransform.Up() * 0.3f);

        physicsWorld.CapsuleCastAll(p1, p2, 1f, float3.zero, 0.2f, ref hits, new CollisionFilter
        {
            BelongsTo = (uint)1 << 6,
            CollidesWith = (uint)1 << 7 | (uint)1 << 9 | (uint)1 << 10 | (uint)1 << 11
        });


        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                Entity hitEntity = hits[i].Entity;

                if (_entityManager.HasComponent<EnemyComponent>(hitEntity))
                {
                    EnemyComponent enemyComponent = _entityManager.GetComponentData<EnemyComponent>(hitEntity);
                    _playerComponent.currentHealth -= enemyComponent.damage;
                    _entityManager.SetComponentData(_playerEntity, _playerComponent);

                    if (_playerComponent.currentHealth <= 0f)
                    {
                        //_entityManager.DestroyEntity(_playerEntity);
                        // GameOver
                    }
                    _entityManager.DestroyEntity(hitEntity);
                }
                else if (_entityManager.HasComponent<CoinComponent>(hitEntity))
                {
                    CoinComponent coinComponent = _entityManager.GetComponentData<CoinComponent>(hitEntity);
                    _playerComponent.coinCount += coinComponent.coinIncrementValue;
                    _entityManager.SetComponentData(_playerEntity, _playerComponent);
                    _entityManager.DestroyEntity(hitEntity);
                }
                else if (_entityManager.HasComponent<HealthPackComponent>(hitEntity))
                {
                    HealthPackComponent healthComponent = _entityManager.GetComponentData<HealthPackComponent>(hitEntity);
                    _playerComponent.currentHealth += healthComponent.healthIncrementValue;
                    _playerComponent.currentHealth = math.min(_playerComponent.currentHealth, _playerComponent.maxHealth);
                    _entityManager.SetComponentData(_playerEntity, _playerComponent);
                    _entityManager.DestroyEntity(hitEntity);
                }
                else if (_entityManager.HasComponent<BulletComponent>(hitEntity))
                {
                    BulletComponent bulletComponent = _entityManager.GetComponentData<BulletComponent>(hitEntity);
                    _playerComponent.currentHealth -= bulletComponent.damage;
                    _entityManager.SetComponentData(_playerEntity, _playerComponent);

                    if (_playerComponent.currentHealth <= 0f)
                    {
                        //_entityManager.DestroyEntity(_playerEntity);
                        // GameOver
                    }
                    _entityManager.DestroyEntity(hitEntity);
                }
            }

        }

        hits.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
