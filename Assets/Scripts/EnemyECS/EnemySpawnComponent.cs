using Unity.Entities;

public struct EnemySpawnComponent : IComponentData
{
    public Entity enemyPrefab;
    public Entity specialEnemyPrefab;
    public Entity enemyBulletPrefab;
    public Entity coinPrefab;
    public Entity healthPrefab;

    public int EnemiesSpawnCountPerSecond;
    public int EnemiesSpawnIncrementAmount;
    public int MaxEnemiesSpawnPerSecond;
    public float EnemySpawnRadius;
    public float MinDistanceFromPlayer;

    public float TimeBeforeNextSpawn;
    public float CurrentTimeBeforeSpawn;
}
