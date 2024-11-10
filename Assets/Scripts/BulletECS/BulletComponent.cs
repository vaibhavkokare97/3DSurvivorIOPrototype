using Unity.Entities;

public struct BulletComponent : IComponentData
{
    public float Speed;
    public float DirectionX, DirectionZ;
    public float Damage;
}
