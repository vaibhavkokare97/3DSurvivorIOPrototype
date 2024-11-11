using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Joystick joystick;

    private int _coinCount;
    public int coinCount
    {
        get
        {
            return _coinCount;
        }
        set
        {
            _coinCount = value;
            onCoinCountChange?.Invoke(_coinCount);
        }
    }
    public Action<int> onCoinCountChange;

    private float _healthCount;
    public float healthCount
    {
        get
        {
            return _healthCount;
        }
        set
        {
            _healthCount = Mathf.Clamp(value, 0f, 100f);
            OnHealthChange?.Invoke(_healthCount);
        }
    }
    public Action<float> OnHealthChange;


    [SerializeField] private GameObject debugPlayer;
    [HideInInspector] public float3 playerPosition;

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

    private void OnDestroy()
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



