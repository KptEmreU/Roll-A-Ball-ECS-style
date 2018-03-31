using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class FollowCameraSystem : ComponentSystem {

    public Transform cameraTransform;
    public float3 offset = new float3(0, 6, -8);

    public void FindCameraTransform()
    {
        cameraTransform = Camera.main.transform;
    }

    public struct PlayerGroup
    {
        public ComponentDataArray<Player> Player;
        public ComponentDataArray<Position> Position;
    }

    [Inject] PlayerGroup player;

    protected override void OnUpdate()
    {
        if (cameraTransform != null)
        {
            cameraTransform.position = player.Position[0].Value + offset; 
        }
    }

}
