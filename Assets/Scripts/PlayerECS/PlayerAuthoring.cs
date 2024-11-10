using Unity.Entities;
using UnityEngine;

internal class PlayerAuthoring : MonoBehaviour
{
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    public int coinCount = 0;
    public float moveSpeed = 0.1f;
    public float incrementalCheckForEnemyInterval = 0f;

    public GameObject bulletPrefab;
}

class PlayerAuthoringBaker : Baker<PlayerAuthoring>
{
    public override void Bake(PlayerAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new PlayerComponent
        {
            moveSpeed = authoring.moveSpeed,
            currentHealth = authoring.currentHealth,
            maxHealth = authoring.maxHealth,
            incrementalCheckForEnemyInterval = authoring.incrementalCheckForEnemyInterval,
            bulletPrefab = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
            coinCount = authoring.coinCount
        });
    }
}
