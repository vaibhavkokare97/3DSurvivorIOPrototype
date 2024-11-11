using Unity.Entities;

public struct EnemyComponent : IComponentData
{
    public bool IsSpecial;
    public float incrementalCheckForPlayerInterval;

    public float CurrentHealth;
    public float Damage;
    public float Speed;
}
