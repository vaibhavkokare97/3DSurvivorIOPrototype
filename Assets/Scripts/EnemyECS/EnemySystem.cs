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

        Move(ref state);
    }

    private void Move(ref SystemState state)
    {
        NativeArray<Entity> allEnemies = _entityManager.GetAllEntities();


        foreach (Entity enemy in allEnemies)
        {
            if (_entityManager.HasComponent<EnemyComponent>(enemy))
            {
                //move

                LocalTransform enemyTransform = _entityManager.GetComponentData<LocalTransform>(enemy);
                EnemyComponent enemyComponent = _entityManager.GetComponentData<EnemyComponent>(enemy);

                float3 moveDirection = math.normalize(_playerTransform.Position - enemyTransform.Position);

                enemyTransform.Position += enemyComponent.Speed * moveDirection * SystemAPI.Time.DeltaTime;
                _entityManager.SetComponentData(enemy, enemyTransform);
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
