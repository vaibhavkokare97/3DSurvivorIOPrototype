using Unity.Entities;
using UnityEngine;

class EnemySpawnAuthoring : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject specialEnemyPrefab;
    public GameObject enemyBulletPrefab;
    public GameObject coinPrefab;
    public GameObject healthPrefab;

    public int enemiesSpawnCountPerSecond = 10;
    public int enemiesSpawnIncrementAmount = 2;
    public int maxEnemiesSpawnPerSecond = 50;
    public float enemySpawnRadius = 40f;
    public float minDistanceFromPlayer = 5f;

    public float timeBeforeNextSpawn = 2f;
}

class EnemySpawnAuthoringBaker : Baker<EnemySpawnAuthoring>
{
    public override void Bake(EnemySpawnAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new EnemySpawnComponent
        {
            enemyPrefab = GetEntity(authoring.enemyPrefab, TransformUsageFlags.None),
            specialEnemyPrefab = GetEntity(authoring.specialEnemyPrefab, TransformUsageFlags.None),
            enemyBulletPrefab = GetEntity(authoring.enemyBulletPrefab, TransformUsageFlags.None),
            coinPrefab = GetEntity(authoring.coinPrefab, TransformUsageFlags.None),
            healthPrefab = GetEntity(authoring.healthPrefab, TransformUsageFlags.None),
            enemiesSpawnCountPerSecond = authoring.enemiesSpawnCountPerSecond,
            enemiesSpawnIncrementAmount = authoring.enemiesSpawnIncrementAmount,
            maxEnemiesSpawnPerSecond = authoring.maxEnemiesSpawnPerSecond,
            enemySpawnRadius = authoring.enemySpawnRadius,
            minDistanceFromPlayer = authoring.minDistanceFromPlayer,
            timeBeforeNextSpawn = authoring.timeBeforeNextSpawn,
        });
    }
}
