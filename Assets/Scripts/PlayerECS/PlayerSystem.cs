using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public partial struct PlayerSystem : ISystem
{
    private EntityManager _entityManager;

    private Entity _playerEntity;
    private PlayerComponent _playerComponent;

    private GameMangerRef _gameManagerComponent;

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

        Move(ref state);
    }

    private void Move(ref SystemState state)
    {
        foreach(var gameManagerRef in SystemAPI.Query<GameMangerRef>())
        {
            LocalTransform playerTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);
            playerTransform.Position += new float3(gameManagerRef.Value.Value.joystick.Horizontal * SystemAPI.Time.DeltaTime,
                0,
                gameManagerRef.Value.Value.joystick.Vertical * SystemAPI.Time.DeltaTime) * _playerComponent.moveSpeed;

            gameManagerRef.Value.Value.playerPosition = playerTransform.Position;

            _entityManager.SetComponentData(_playerEntity, playerTransform);
        }
        
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
