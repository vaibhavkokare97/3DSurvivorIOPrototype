using Unity.Entities;

public struct BulletComponent : IComponentData
{
    public float speed;
    public float directionX, directionZ;
    public float damage;
}
