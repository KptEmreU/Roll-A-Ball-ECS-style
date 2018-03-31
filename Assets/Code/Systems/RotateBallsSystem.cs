using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

class RotateBallsSystem : JobComponentSystem
{

    [ComputeJobOptimization]
    public struct RotationJob : IJobProcessComponentData<Rotation, MustRotate>
    {
        public float time;
        public Vector3 vector3;

        public void Execute(ref Rotation rot, [ReadOnly] ref MustRotate mR)
        {
            

            rot.Value = Quaternion.AngleAxis(math.sin(time) * 100, vector3);

            //rot = damnRot;
        }
    }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new RotationJob() { time = Time.timeSinceLevelLoad , vector3 = Vector3.up };
            var handle = job.Schedule(this,1, inputDeps);
            return handle;
        }
    
}
