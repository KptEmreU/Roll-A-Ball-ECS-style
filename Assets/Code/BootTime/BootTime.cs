using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;
using Unity.Jobs;

public class BootTime
{

    static MeshInstanceRenderer playerLook;
    static MeshInstanceRenderer cubeLook;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Init()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();

        var firstEntityArchetype = entityManager.CreateArchetype(typeof(Cube), typeof(TransformMatrix), typeof(Position), typeof(Rotation), typeof(FirstSteps), typeof (Player), typeof(PlayerInput));

        var turningCubesEntityArchetype = entityManager.CreateArchetype(typeof(Ball), typeof(TransformMatrix), typeof(Position), typeof(Rotation),typeof(MustRotate));

        cubeLook = GetLookFromPrototype("cubeLook");
        playerLook = GetLookFromPrototype("playerLook");

        var player = entityManager.CreateEntity(firstEntityArchetype);

        entityManager.AddSharedComponentData(player, playerLook);

        for (int i = 0; i < 10; i++)
        {
            var ball = entityManager.CreateEntity(turningCubesEntityArchetype);

            entityManager.AddSharedComponentData(ball, cubeLook); 
        }

        World.Active.GetOrCreateManager<EraseBallsSystem>().SetUI();
        World.Active.GetOrCreateManager<FollowCameraSystem>().FindCameraTransform();
    }

    private static MeshInstanceRenderer GetLookFromPrototype(string protoName)
    {
        var proto = GameObject.Find(protoName);
        Debug.Log(proto.name + " this is the name found");
        var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
        Object.Destroy(proto);
        return result;
    }

}
