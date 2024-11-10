using Unity.Entities;
using UnityEngine;

class EnemySpawnAuthoring : MonoBehaviour
{
    public GameObject enemyPrefab;

    public int EnemiesSpawnCountPerSecond = 10;
    public int EnemiesSpawnIncrementAmount = 2;
    public int MaxEnemiesSpawnPerSecond = 50;
    public float EnemySpawnRadius = 40f;
    public float MinDistanceFromPlayer = 5f;

    public float TimeBeforeNextSpawn = 2f;
}

class EnemySpawnAuthoringBaker : Baker<EnemySpawnAuthoring>
{
    public override void Bake(EnemySpawnAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new EnemySpawnComponent
        {
            enemyPrefab = GetEntity(authoring.enemyPrefab, TransformUsageFlags.None),
            EnemiesSpawnCountPerSecond = authoring.EnemiesSpawnCountPerSecond,
            EnemiesSpawnIncrementAmount = authoring.EnemiesSpawnIncrementAmount,
            MaxEnemiesSpawnPerSecond = authoring.MaxEnemiesSpawnPerSecond,
            EnemySpawnRadius = authoring.EnemySpawnRadius,
            MinDistanceFromPlayer = authoring.MinDistanceFromPlayer,
            TimeBeforeNextSpawn = authoring.TimeBeforeNextSpawn,
        });
    }
}
