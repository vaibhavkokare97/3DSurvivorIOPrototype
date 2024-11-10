using Unity.Entities;

public struct PlayerComponent : IComponentData
{
    public float currentHealth;
    public float moveSpeed;

    public Entity bulletPrefab;
}
