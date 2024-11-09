using Unity.Entities;
using UnityEngine;

internal class PlayerAuthoring : MonoBehaviour
{
    public Joystick Joystick;
}

class PlayerAuthoringBaker : Baker<PlayerAuthoring>
{
    public override void Bake(PlayerAuthoring authoring)
    {
        
    }
}
