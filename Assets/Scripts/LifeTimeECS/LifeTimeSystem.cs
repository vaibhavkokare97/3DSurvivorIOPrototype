using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics;

partial struct LifeTimeSystem : ISystem
{
    private EntityManager _entityManager;

    private Entity _playerEntity;
    private LocalTransform _playerTransform;

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
        _playerTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);
        NativeArray<Entity> allLifeTimeEntities = _entityManager.GetAllEntities();

        foreach (Entity entity in allLifeTimeEntities)
        {
            if (_entityManager.HasComponent<LifeTimeComponent>(entity))
            {
                LocalTransform entityTransform = _entityManager.GetComponentData<LocalTransform>(entity);
                LifeTimeComponent lifeTimeComponent = _entityManager.GetComponentData<LifeTimeComponent>(entity);
                lifeTimeComponent.RemainingLife -= SystemAPI.Time.DeltaTime;
                float3 distanceInVector = entityTransform.Position - _playerTransform.Position;
                float dist = math.sqrt(math.square(distanceInVector.x) + math.square(distanceInVector.z));
                if (lifeTimeComponent.RemainingLife <= 0f && dist > 30f)
                {
                    _entityManager.DestroyEntity(entity);
                    continue;
                }
                _entityManager.SetComponentData(entity, lifeTimeComponent);
            }
        }
    }
}
