using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class PlayerMoveSystem : ComponentSystem
{
    public struct PlayerGroup
    {
        public int Length;
        public ComponentDataArray<Position> positionOfPlayer;
        public ComponentDataArray<PlayerInput> playerInput;
    }

    [Inject] PlayerGroup player;

    private float speed = 6;

    protected override void OnUpdate()
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < player.Length; i++)
        {
            var pos = player.positionOfPlayer[i];
            var input = player.playerInput[i];

            input.Move = new float2(Input.GetAxisRaw("Horizontal") * speed * dt, Input.GetAxisRaw("Vertical") * speed * dt);
            pos.Value += new float3(input.Move.x, 0, input.Move.y);

            player.positionOfPlayer[i] = pos;
        }

    }
}


