using Unity.Entities;

public struct EnemyComponent : IComponentData
{
    public bool isSpecial;
    public float incrementalCheckForPlayerInterval;

    public float currentHealth;
    public float damage;
    public float speed;
}
