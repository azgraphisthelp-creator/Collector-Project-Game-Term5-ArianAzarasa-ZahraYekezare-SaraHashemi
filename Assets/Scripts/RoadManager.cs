using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    public static RoadManager Instance;

private RoadProgress currentRoadProgress;
    [Header("Player")]
    [SerializeField] private Transform player;


    [Header("Road References")]
    [SerializeField] private GameObject roadPrefab;


    [Header("Road Settings")]
    [SerializeField] private float roadLength = 150f;
    [SerializeField] private int roadsOnScreen = 3;


    [Header("Ball Settings")]
    [SerializeField] private int ballsPerRoad = 10;


    private Queue<GameObject> roads = new Queue<GameObject>();

    private float nextSpawnZ;


    private GameObject currentRoad;




    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }



    private void Start()
    {
        nextSpawnZ = 0;


        for(int i = 0; i < roadsOnScreen; i++)
        {
            SpawnRoad();
        }


        SetCurrentRoad();
    }



    private void SpawnRoad()
    {
        GameObject road = Instantiate(
            roadPrefab,
            new Vector3(0,0,nextSpawnZ),
            Quaternion.identity
        );


        roads.Enqueue(road);


        nextSpawnZ += roadLength;



        CreateGateOnRoad(road);

        CreateBallsOnRoad(road);
    }




private void SetCurrentRoad()
{
    if(roads.Count == 0)
        return;


    currentRoad = roads.Peek();

    currentRoadProgress =
    currentRoad.GetComponent<RoadProgress>();


    if(UIManager.Instance != null)
    {
        UIManager.Instance.UpdateProgress(0);
    }
}




    public void PlayerPassedRoad()
    {

        if(roads.Count == 0)
            return;



        GameObject oldRoad = roads.Dequeue();


        Destroy(oldRoad);



        SpawnRoad();



        SetCurrentRoad();
    }





    



    private void CreateGateOnRoad(GameObject road)
    {
        Transform gatePoint =
            road.transform.Find("GateSpawn");


        if(gatePoint == null)
        {
            Debug.LogError(
                "GateSpawn not found in Road prefab"
            );

            return;
        }



        LevelManager.Instance.SpawnGateFromRoad(gatePoint);
    }





    private void CreateBallsOnRoad(GameObject road)
    {
        Transform ballPoint =
            road.transform.Find("BallSpawnCenter");


        if (ballPoint == null)
        {
            Debug.LogError(
                "BallSpawnCenter not found"
            );

            return;
        }



        for (int i = 0; i < ballsPerRoad; i++)
        {

            Vector3 pos =
                ballPoint.position +
                new Vector3(
                    Random.Range(-3f, 3f),
                    0.5f,
                    Random.Range(-5f, 5f)
                );


            Instantiate(
                LevelManager.Instance.ballPrefab,
                pos,
                Quaternion.identity
            );
        }
    }
    public float GetRoadProgress(Vector3 playerPosition)
{
    if(currentRoadProgress == null)
        return 0;


    return currentRoadProgress.GetProgress(playerPosition);
}
}