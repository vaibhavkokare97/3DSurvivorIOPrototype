using Unity.Entities;

public struct EnemySpawnComponent : IComponentData
{
    public Entity enemyPrefab;
    public Entity specialEnemyPrefab;
    public Entity enemyBulletPrefab;
    public Entity coinPrefab;
    public Entity healthPrefab;

    public int enemiesSpawnCountPerSecond;
    public int enemiesSpawnIncrementAmount;
    public int maxEnemiesSpawnPerSecond;
    public float enemySpawnRadius;
    public float minDistanceFromPlayer;

    public float timeBeforeNextSpawn;
    public float currentTimeBeforeSpawn;
}
