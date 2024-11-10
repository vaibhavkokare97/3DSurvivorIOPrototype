using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Joystick joystick;

    public GameObject debugPlayer;
    public float3 playerPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        debugPlayer.transform.position = playerPosition;
    }

    private void OnDisable()
    {
        Instance = null;
    }
}

public struct GameMangerRef : IComponentData
{
    public UnityObjectRef<GameManager> Value;
}

public partial struct UpdateGameManagerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (GameManager.Instance == null) return;
        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(entity, new GameMangerRef
        {
            Value = GameManager.Instance
        });
        state.Enabled = false;
    }
}



