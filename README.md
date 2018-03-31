# Roll-A-Ball-ECS-style
Recreation of Unity Roll-A-Ball tutorial 

Completed by Unity 2018.1.0b12 build from welcome thread. https://beta.unity3d.com/download/ed1bf90b40e6/public_download.html

![Alt Text](https://thumbs.gfycat.com/FlashyContentFoxhound-size_restricted.gif)

This is a pure-ECS example as much as it could be. I have used Canvas and Camera Transform and I have created table from cubes so they are actually game objects. There is only 1 job component system and 4 regular component systems. Honestly Eraseballsystem can be turned into job too as it might make lots of stuff when you spawn 4000 cubes. And yes you can :) Though even at 36000 cubes it is 80 frames in my computer. So your call.

Other than that balls and cubes are all entities, they don't use any physics or colliders. It means actually you can fly away from the table but you can easily limit it with a new system stoping the inputs when a particular X or Z value is happened on the player ball.

Let's start

First you need tons of new Libraries as using statements;
```
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
```


Copy paste them above your scripts until you understand which ones are actually needed :) Visual Studio will handle the redundant ones anyway.

Every game has a start and in our example it is "BootTime" script

Again as we are writing our code in ECS style we won't use any Monobehaviour, there will be no script attaching. So our class actually don't need to be a Monobehaviour. Here is the first difference
```
public class BootTime
{....}
```

But we need something to work right? I mean how we can start this game? Here we have a new attribute where you should write on top of the function you want to work. (Attribute will make this function called after the scene loaded, because we need some gameobjects as templates for our entities. See cubeLook, and playerLook in hierarchy. They will deleted when game starts as those gameobjects are just for samples and will not be in our game.
```
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Init() {....}
```
Ok somehow we make it work while not using a gameObject. But we will need a entity manager so we can spawn some entities right?
```
var entityManager = World.Active.GetOrCreateManager<EntityManager>();
        // This is how we will create our player, ECS works best when you create Archetypes. It is kinda prefab but not ofc 
        var firstEntityArchetype = entityManager.CreateArchetype(typeof(Cube), typeof(TransformMatrix), typeof(Position), typeof(Rotation), typeof(FirstSteps), typeof (Player), typeof(PlayerInput));

        // This is for cubes. As you can see there is no renderer in these components, because we need to add it later.
        var turningCubesEntityArchetype = entityManager.CreateArchetype(typeof(Ball), typeof(TransformMatrix), typeof(Position), typeof(Rotation),typeof(MustRotate));

        // These lines are we get our Mesh Instance Renderers. Check the real script it is just a get function in BootTime
        cubeLook = GetLookFromPrototype("cubeLook");
        playerLook = GetLookFromPrototype("playerLook");

        // This is where we are creating entity
        var player = entityManager.CreateEntity(firstEntityArchetype);

        // This is where we are adding Look (MeshRenders) to Entities. They will become visible now 
        entityManager.AddSharedComponentData(player, playerLook);

        // This is a bad way of creating 10 of cubes. But it works. You can write 1000 too if you want to see :P
        for (int i = 0; i < 10; i++)
        {
            var ball = entityManager.CreateEntity(turningCubesEntityArchetype);

            entityManager.AddSharedComponentData(ball, cubeLook); 
        }
        // These below 2 lines are not ECS way of doing things. First is linking a GameObject Text component to our ECS world
        // Second is finding Camera Transform (another gameObject) so it can shamelessly follow our ball
        World.Active.GetOrCreateManager<EraseBallsSystem>().SetUI();
        World.Active.GetOrCreateManager<FollowCameraSystem>().FindCameraTransform();
```
So we have entities... And they are rendered. But they are all together waiting at (0,0,0) point of our world. We need to spread them with a Component System. Because a component system is how we do things in ECS. They are our hyper Update functions. They do their CPU magic and only use Components that we created to group up to know which entities they will interact. Yet we could never know which component system's update will work first unless we instruct them to wait. So let's assume anything can happen. As ordering them to wait each other is somewhat outside the scope of Roll-A-Ball.

I will not explain each system in detail but we should surgically analyse this one for learning purposes.

```
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

// ECS class will form from ComponentSystem. It will never attach to anything. Not even to entities. 
// It will magically work in your code folder.
public class SetBallsSystem : ComponentSystem { 

// This is a hack I used. If you want to do something only once and at start, like spreading cubes around
// you can do it once and change this bool to false.
// A more ECS way of it might be creating things you want to get into system with a GiveMeAStartFunction component.
// Remember we can create any Component and then create an entity with any component too.
// And then removing that component when first frame is finished will make that entities exempted from that system(!)
// We will come back to add and remove component in realtime
// later. 
private bool isFirstTime = true;

// What is this? This is where we declare, "Hey system, these entities are for you, they will always have
// Component A, Component B and Component C, Yes they may have Component D but it's not your job
// If they have A, B and C do something about it!"
// Also beware these are always arrays. Even if you know there will be 1 entity with that component,
// ECS will think it as an array of 1 guy and never just a single entity.
// These are structs, that's how CPU do its magic btw. But you don't have to care about it
// Guys in Unity can handle those magic or you can read it yourself later.
public struct BallsGroup {....}

// Injection?! Ah we created a struct and it created an array of entites but how we will move them into system,
// Because system is in our folders and not attached to entities it cares. So we say "Hey system, remember the 
// struct I have created for you, use this in your update so you can actually do something meaningfull"
// If you don't inject this data (struct) , you will see that you can not actually play with them in update
[Inject] BallsGroup ballsGroup

// This is our old friend Update, in ECS style. Like x1000 faster (I am just making up numbers, but it is fast!)
protected override void OnUpdate(){ ....}
```
Ok we get the system layout but let's focus into struct right
```
  public struct BallsGroup
    {
        // This is kinda magical, public int Length, don't ask me why but it increments with every new entity enters
        // so actually you can write something like BallsGroup.Length and it gives the length of BallsGroup
        // like regular arrays
        public int Length ;
        
        // This is how get our own componets, balls in this example. We create our own components check components script
        public ComponentDataArray<Ball> balls;
        
        // Hmm but this is not a component we have created. Yet it is very important. This is created for us and if we really
        // want to show an entity on screen we need to have this magical component and some more. Check
        // how we create entities, you will see TransformMatrix, Rotation, Position, MeshRenderer are needed for visible
        // entities. Maybe more. Check your self. Position is one of those.
        public ComponentDataArray<Position> positions;
    }
```

Sooo, we just checked inside of our first system. Next?

Let's make the PlayerMove, so we need a 
```
public class PlayerMoveSystem
```

That's not something very new but just look into OnUpdate function so we learn how to move a ball around
```
protected override void OnUpdate()
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < player.Length; i++)
        {
        // Now this is interesting as Position is a float3 and not a Vector3 in ECS (Don't ask me why)
        // And also you really don't want to mess with their original values directly
        // You take out their copy here with "var pos ="
            var pos = player.positionOfPlayer[i];
        // Remember we have created this PlayerInput component in Components script
            var input = player.playerInput[i];
        
        // Then we do all our math,
        // See that we can actually use our Input class here. As this is not a job and just ECS
            input.Move = new float2(Input.GetAxisRaw("Horizontal") * speed * dt, Input.GetAxisRaw("Vertical") * speed * dt);
            pos.Value += new float3(input.Move.x, 0, input.Move.y);

        // And here we assign the new pos to Position of player
        // Even though there is only one player I use it as an array as I should use it.
        // Though you can write down [0] and it will work, until you have 2 players... Hmm.
            player.positionOfPlayer[i] = pos;
        }

    }
```

Let's rotate cubes but Call this
```
class RotateBallsSystem : JobComponentSystem
```
I mean why not? This is a Job Though and not our usual Component System. It is a JobComponentSystem

```
class RotateBallsSystem : JobComponentSystem
{

// This attribute is for Burst Compiler so it could do its magic. Write it on top of Jobs ;)
    [ComputeJobOptimization]
// See that IJobProcessComponentData is not the real job, but it is the struct which has the data
// but also have the Execution of the data... Hmm yeah it is different. This one does not have update
// because it doesn't even know what is update?! This Job will be executed in somewhere magical in another
// thread. It will forget what is Unity, what is your game state etc. It will only remember whatever you
// spoon feed to it. So this is an important part
    public struct RotationJob : IJobProcessComponentData<Rotation, MustRotate>
    {
    // See how we create empty variables for struct, we need to fill them up. But the job doesn't know what they are yet
    // actually it will never start to execute if do not schedule him. Better if we don't forget to give him these variables
    // when we are scheduling.
        public float time;
        public Vector3 vector3;

    // These are components from ECS. See the "ref" you should write it for now. See [ReadOnly] it is a performance boost
    // If you won't use it CPU will think you can also write into it. So if anyone else is working on it 
    // It will wait. For example Rotation is writable. Happily no one else is using it.
    // Also the Execute is the logic of our Job. It is dumb and empty now. We need to schedule and set it up.
        public void Execute(ref Rotation rot, [ReadOnly] ref MustRotate mR)
        {
            //Math here. This logic uses time and vector3 and static Quaternion turning api
            rot.Value = Quaternion.AngleAxis(math.sin(time) * 100, vector3);

           
        }
    }
      // This is the setup phase. It say OnUpdate Do this job
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
        // But to do this job you will need time and vector3. Remember the code above this one
            var job = new RotationJob() { time = Time.timeSinceLevelLoad , vector3 = Vector3.up };
        // Ok you got your numbers from unity CPU. Now schedule this bad boy. And return to me when finished.
            var handle = job.Schedule(this,1, inputDeps);
            return handle;
        }
    
}
```

And we made our first job too. Let's see how we can destroy entities when our ball approachs them.

```
public class EraseBallsSystem : ComponentSystem
{
    // Remember this is called from our BootTime Script, not very ECS but it works ;) Honestly I don't know anyother way
    public void SetUI()
    {
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
    }

    public Text scoreText;
    private int score = 0;
  
    // Interesting but we need 2 groups here. 1 is player
    public struct PlayerGroup
    {
        public ComponentDataArray<Position> playerPos;
        public ComponentDataArray<Cube> cube;
        public int Length;
        
    }

    // 2.group is Cubes. Don't mind the ballsGroup naming. We all do errors.
    public struct BallsGroup
    {
        public ComponentDataArray<Ball> ball;
        public ComponentDataArray<Position> ballPos;
        
        // But becareful about this one this is something new. It says EntityArray of this group. It will be 
        // direct link to entities. In old way this is the way to get their gameobjects-like.
        public EntityArray entityArray;
        // There are more but this is also an interesting command. IT says even if a entity A and B but also have C (subtractive)
        // dont take those into this group. Remember normally without subtractive groups will take any entity
        // as long as they have A and B regardless if they have C , D, whatever.
        public SubtractiveComponent<Die> die;
        public int Length;
    }

    // See how we can Inject 2 groups because I need to find distance of player to every cube out there.
    // Not the best way but without using physix and colliders this was a quick way
    // And I tried with 36000 entites and it was still 80 frames
    [Inject] PlayerGroup playerGroup;
    [Inject] BallsGroup ballsGroup;

    // All these commented EndFrameBarrier stuff is just another way of doing what I Have done with PostUpdateCOmmands
    // So they are staying here as a tribute to @Spy-Shifty
    //[Inject] EndFrameBarrier endFrameBarrier;
    protected override void OnUpdate()
    {
        //EntityCommandBuffer commandBuffer = endFrameBarrier.CreateCommandBuffer();
        

        for (int i = 0; i < ballsGroup.Length; i++)
        {
            float dist = math.distance(playerGroup.playerPos[0].Value, ballsGroup.ballPos[i].Value);
            
            //!= 0 is required as this system actually checks distances even before I set them apart in the first frame 
            // in another system. So beware if you didn't order your systems to work one after the other anything
            // is possible. use caution for your own race conditions.
            if ( dist < 1 & dist != 0)
            {
                //this is to make things float up and down, if you wanna see this, comment below AddComponent and Destroy stuff
                //var newPos = ballsGroup.ballPos[i];
                //newPos.Value = new float3(newPos.Value.x, 3 - dist, newPos.Value.z);
                //ballsGroup.ballPos[i] = newPos;


                // Now these are interesting because you don't want to Destroy and entity in mid of the process
                // So you have to use PostUpdateCommands API to buffer your commands until the end of the frame. Or
                // at least after all Updates.
                PostUpdateCommands.AddComponent<Die>(ballsGroup.entityArray[i], new Die());
                PostUpdateCommands.DestroyEntity(ballsGroup.entityArray[i]);

                // null check just to be sure at the first frame
                if (scoreText != null)
                {
                    score++;
                    scoreText.text = score.ToString(); 
                }
                //commandBuffer.AddComponent<Die>(ballsGroup.entityArray[i], new Die());
            }

          
        }
        
       
    }
```

If you have come this far. I think you have some knowledge of ECS now.

Remember I am a noob with ECS and I am hardly a good programmer. But I just want to tell my knowledge about ECS. Hope it helps to fellow Unity Devs. 

Sorry for grammar and typos. Too late to proof read :P

Bonus : Roll-A-Ball-20000

![Alt Text](https://thumbs.gfycat.com/RawDecentAllensbigearedbat-size_restricted.gif)
