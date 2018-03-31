using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public struct Cube : IComponentData {}

public struct Ball : IComponentData { }

public struct FirstSteps : IComponentData { }

public struct Player : IComponentData { }

public struct PlayerInput : IComponentData { public float2 Move;  }

public struct MustRotate : IComponentData { }

public struct Die : IComponentData { }
//public class MustRotateComponent : ComponentDataWrapper<MustRotate> { }


