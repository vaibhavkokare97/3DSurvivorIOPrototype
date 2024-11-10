using Unity.Entities;

public struct PlayerComponent : IComponentData
{
    public float currentHealth;
    public float maxHealth;
    public int coinCount;
    public float moveSpeed;

    public float incrementalCheckForEnemyInterval;

    public Entity bulletPrefab;
}
