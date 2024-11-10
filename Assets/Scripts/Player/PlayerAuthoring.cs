using Unity.Entities;
using UnityEngine;

internal class PlayerAuthoring : MonoBehaviour
{
    public float moveSpeed = 0.1f;
}

class PlayerAuthoringBaker : Baker<PlayerAuthoring>
{
    public override void Bake(PlayerAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new PlayerComponent
        {
            moveSpeed = authoring.moveSpeed
        });
    }
}
