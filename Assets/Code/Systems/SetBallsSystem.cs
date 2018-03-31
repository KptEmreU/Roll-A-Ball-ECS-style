using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;


public class SetBallsSystem : ComponentSystem {

    public struct BallsGroup
    {
        public int Length ;
        public ComponentDataArray<Ball> balls;
        public ComponentDataArray<Position> positions;
        

    }


    [Inject] BallsGroup ballsGroup;

    private bool isFirstTime = true;
    private float offset = 10f;

    protected override void OnUpdate()
    {
        

        if (isFirstTime)
        {
            for (int i = 0; i < ballsGroup.Length; i++)
            {
                var coeff = (360 / (ballsGroup.Length % 359 + 0.0001f)) * i / 57.3f;

                var positions = ballsGroup.positions[i];

                positions.Value = new float3(math.cos(coeff) * offset, 0, math.sin(coeff) * offset);

                ballsGroup.positions[i] = positions;

              
            }
            
        }
        

        isFirstTime = false;
    }

    
}
