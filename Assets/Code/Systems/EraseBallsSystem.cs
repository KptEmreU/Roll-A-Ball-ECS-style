using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.UI;

public class EraseBallsSystem : ComponentSystem
{

    public void SetUI()
    {
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
    }

    public Text scoreText;
    private int score = 0;

    public struct PlayerGroup
    {
        public ComponentDataArray<Position> playerPos;
        public ComponentDataArray<Cube> cube;
        public int Length;
        
    }

    public struct BallsGroup
    {
        public ComponentDataArray<Ball> ball;
        public ComponentDataArray<Position> ballPos;
        public EntityArray entityArray;
        public SubtractiveComponent<Die> die;
        public int Length;
    }

    [Inject] PlayerGroup playerGroup;
    [Inject] BallsGroup ballsGroup;

    //[Inject] EndFrameBarrier endFrameBarrier;
    protected override void OnUpdate()
    {
        //EntityCommandBuffer commandBuffer = endFrameBarrier.CreateCommandBuffer();
        

        for (int i = 0; i < ballsGroup.Length; i++)
        {
            float dist = math.distance(playerGroup.playerPos[0].Value, ballsGroup.ballPos[i].Value);
            
            //!= 0 is required as this system actually checks distances even before I set them apart in the first frame 
            // in another system.
            if ( dist < 1 & dist != 0)
            {
                //this is to make things float up and down
                //var newPos = ballsGroup.ballPos[i];
                //newPos.Value = new float3(newPos.Value.x, 3 - dist, newPos.Value.z);
                //ballsGroup.ballPos[i] = newPos;


                // this baby is working perfect. But I understand I can also destroy them no problem so why send another component
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

    

    
}
