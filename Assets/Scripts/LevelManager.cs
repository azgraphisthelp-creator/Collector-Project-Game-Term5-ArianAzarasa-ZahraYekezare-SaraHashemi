using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

[Header("Progress Bar")]
[SerializeField] private Transform progressStart;
private float totalProgressDistance;

private Gate progressGate;
    [SerializeField] private float gateSpawnDelay = 2f;
[Header("Progress")]
[SerializeField] private float progressDistance = 130f;

private float progressStartZ;
private List<Gate> passedGates = new List<Gate>();
    private bool waitingForFinish;
    public static LevelManager Instance;
    private bool finishActive = false;

private float gateStartZ;
private float gateEndZ;

[Header("Finish Position")]
[SerializeField] private float finishPosX = 0f;
[SerializeField] private float finishPosY = 0.5f;
[Header("Road Settings")]
[SerializeField] private LayerMask roadLayer;
[SerializeField] private float rayHeight = 20f;
    [Header("Camera")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private GameObject gameplayCamera;
    [SerializeField] private GameObject endCamera;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Stack")]
    public Transform stackPoint;

    [Header("Endless Gate Settings")]
    [SerializeField] private Gate gatePrefab;

    [SerializeField] private float gateDistance = 80f;
[SerializeField] private int startRequiredBalls = 12;
[SerializeField] private int maxRequiredBalls = 40;

[SerializeField] private int startSpawnCount = 18;
[SerializeField] private int maxSpawnCount = 50;
private bool finishButtonUsed = false;
private bool finishMode = false;
    private int gateIndex;
    private Vector3 nextGatePosition;
[Header("Player")]
[SerializeField] private Transform playerTransform;

    [Header("Finish")]
    [SerializeField] private GameObject finishZonePrefab;
    [SerializeField] private float finishForwardOffset = 20f;
    [Header("Ball")]
    public Ball ballPrefab;
    [Header("Timing")]
    [SerializeField] private float destroyDelay = 10f;
    [SerializeField] private float nextGateEarlySpawnTime = 2f;
    private Queue<Gate> activeGates = new Queue<Gate>();
private List<Gate> spawnedGates = new List<Gate>();
    private Gate currentGate;
    private int nextGateIndex;

    public List<Ball> collectedBalls = new List<Ball>();
    private List<Ball> ballsToDeposit = new List<Ball>();

    private Transform depositPoint;

    private bool gateRunning;
    private bool nextGateSpawned;

    private int requiredBalls;
    private int depositedBalls;

    private Coroutine gateRoutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SwitchToGameplayCamera();
        progressStartZ = playerTransform.position.z;

    }

    void SwitchToGameplayCamera()
    {
        gameplayCamera.SetActive(true);
        endCamera.SetActive(false);

        cameraFollow.StartFollow();
    }

    void SwitchToEndCamera()
    {
        gameplayCamera.SetActive(false);
        endCamera.SetActive(true);

        cameraFollow.StopFollow();
    }

    public void AddBall(Ball ball)
    {
        if (ball == null) return;
        if (collectedBalls.Contains(ball)) return;

        collectedBalls.Add(ball);
        ball.EnterStackMode();

        RefreshStack();

        UIManager.Instance.SetBallText(collectedBalls.Count);
    }

    public int GetBallCount()
    {
        return collectedBalls.Count;
    }

    public Vector3 GetBallPosition(int index)
    {
        float spacingX = 0.7f;
        float spacingZ = 0.7f;

        int rowSize = 3;

        int row = index / rowSize;
        int col = index % rowSize;

        float x = (col - 1) * spacingX;
        float z = -row * spacingZ;

        return stackPoint.position + new Vector3(x, 0, z);
    }

    void RefreshStack()
    {
        for (int i = 0; i < collectedBalls.Count; i++)
        {
            if (collectedBalls[i] != null)
                collectedBalls[i].SetStackIndex(i);
        }
    }
    public void RefreshStackPublic()
    {
        RefreshStack();
    }


    public void SpawnNextGate()
    {
    Debug.Log("SpawnNextGate");
    if(finishMode)
        return;


    if(nextGateSpawned)
        return;


    nextGateSpawned = true;






    Vector3 spawnPos = nextGatePosition;
    Debug.Log("GATE DISTANCE = " + gateDistance);
    Debug.Log("SPAWN POS = " + spawnPos);

    Gate gate = Instantiate(
        gatePrefab,
        spawnPos,
        Quaternion.identity
    );

    gateEndZ = gate.transform.position.z;
    gate.spawner = this;



    int required =
    Mathf.Clamp(
        startRequiredBalls + gateIndex * 3,
        startRequiredBalls,
        maxRequiredBalls
    );



gate.Setup(
    required,
    gate.depositPoint
);
        if (progressGate == null)
            progressGate = gate;
            ResetProgressBar();


    currentGate = gate;
    spawnedGates.Add(gate);

    SpawnBallsForGate(gate);



   gateIndex++;




nextGatePosition = gate.transform.position + Vector3.forward * gateDistance;
    Debug.Log("GATE SPAWNED : " + gateIndex);
}

    public void GatePassed(Gate gate)
    {
        if (progressGate == gate)
        {
            StartCoroutine(SetNextProgressGate());
        }

        StartCoroutine(DestroyGateRoutine(gate));


        if (!nextGateSpawned)
        {
            StartCoroutine(EarlySpawnNextGate());
        }
    }
private IEnumerator SetNextProgressGate()
{
    yield return new WaitForSeconds(0.1f);

    Gate next = null;

    float closestDistance = Mathf.Infinity;

    foreach (Gate g in spawnedGates)
    {
        if (g == null || g == progressGate)
            continue;
        Debug.Log(
        "Gate: " + g.name +
        " Z = " + g.transform.position.z
        );


        float distance =
            g.transform.position.z - playerTransform.position.z;


        // انتخاب نزدیک ترین گیت جلوتر
        if (distance > 0 && distance < gateDistance + 30f)
        {
            if (distance < closestDistance)
            {
                closestDistance = distance;
                next = g;
            }
        }
    }


    if (next != null)
    {
        progressGate = next;

        ResetProgressBar();

        Debug.Log(
            "NEW PROGRESS GATE = " + next.name +
            " | Distance = " + closestDistance
        );
    }
}
IEnumerator DestroyGateRoutine(Gate gate)
{
    yield return new WaitForSeconds(destroyDelay);

    if (gate != null)
    {
        if (currentGate == gate)
            currentGate = null;


        if (progressGate == gate)
        {
            progressGate = null;
        }


        Destroy(gate.gameObject);
    }
}

    public void StartGate(int required, Transform target)
    {
        if (gateRunning)
            return;

        gateRunning = true;

        requiredBalls = required;
        depositedBalls = 0;

        depositPoint = target;

        ballsToDeposit.Clear();

        int count = Mathf.Min(requiredBalls, collectedBalls.Count);

        for (int i = 0; i < count; i++)
        {
            ballsToDeposit.Add(collectedBalls[i]);
        }

        playerMovement.StopMovement();

        SwitchToEndCamera();

        if (gateRoutine != null)
            StopCoroutine(gateRoutine);

        gateRoutine = StartCoroutine(SendBallsRoutine());
    }

    IEnumerator SendBallsRoutine()
    {
        List<Ball> snapshot = new List<Ball>(ballsToDeposit);

        foreach (Ball ball in snapshot)
        {
            if (ball == null)
                continue;

            ball.GoToDeposit(depositPoint.position);

            yield return new WaitForSeconds(0.08f);
        }
    }


    public void BallDeposited(Ball ball)
    {
        if (!gateRunning)
            return;

        if (ball == null)
            return;


        if (ballsToDeposit.Contains(ball))
            ballsToDeposit.Remove(ball);


        if (collectedBalls.Contains(ball))
            collectedBalls.Remove(ball);


        depositedBalls++;


        RefreshStack();


        UIManager.Instance.SetBallText(collectedBalls.Count);


        if (currentGate != null)
        {
            Debug.Log(
                "ADDING PROGRESS TO GATE : "
                + currentGate.name
            );

            currentGate.AddProgress();
        }
        else
        {
            Debug.Log("CURRENT GATE NULL");
        }


        ball.gameObject.SetActive(false);


        Debug.Log(
            "DEPOSITED = "
            + depositedBalls
            + " / "
            + requiredBalls
        );


        if (depositedBalls >= requiredBalls)
        {
            StartCoroutine(FinishGateRoutine());
        }
    }
    IEnumerator FinishGateRoutine()
    {
        yield return new WaitForSeconds(0.3f);


        gateRunning = false;


        playerMovement.ResumeMovement();


        bool win = depositedBalls >= requiredBalls;


        if (!win)
        {
            LoseFromGate();
            yield break;
        }


        ScoreManager.Instance.AddScore(requiredBalls);


        if (currentGate != null)
        {
            Gate oldGate = currentGate;

            currentGate = null;
            gateStartZ = oldGate.transform.position.z;
            GatePassed(oldGate);
        }


        SwitchToGameplayCamera();


        StartCoroutine(EarlySpawnNextGate());
    }



    public void LoseFromGate()
    {
        gateRunning = false;

        if (gateRoutine != null)
            StopCoroutine(gateRoutine);

        ballsToDeposit.Clear();

        playerMovement.StopMovement();

        SwitchToEndCamera();

        UIManager.Instance.ShowLosePanel();

        Time.timeScale = 0f;
    }


    public void Retry()
    {
        StartCoroutine(RetryRoutine());
    }

   private IEnumerator RetryRoutine()
{
    yield return new WaitForSecondsRealtime(0.15f);


    if(ScoreManager.Instance != null)
        ScoreManager.Instance.ResetScore();


    Time.timeScale = 1f;

    SceneManager.LoadScene(
        SceneManager.GetActiveScene().buildIndex
    );
}

    public void NextLevel()
{
    if(ScoreManager.Instance != null)
        ScoreManager.Instance.ResetScore();


    Time.timeScale = 1f;

    SceneManager.LoadScene(
        SceneManager.GetActiveScene().buildIndex + 1
    );
}
    public void SpawnGateFromRoad(Transform spawn)
    {
        Debug.Log("SpawnGateFromRoad");
        Gate gate = Instantiate(
            gatePrefab,
            spawn.position,
            spawn.rotation
        );
        gateEndZ = gate.transform.position.z;

        if (progressStart != null)
        {
            gateStartZ = progressStart.position.z;
        }

        gate.spawner = this;


        int required =
            Mathf.Clamp(
                startRequiredBalls + Mathf.FloorToInt(gateIndex * 2.5f),
                startRequiredBalls,
                maxRequiredBalls
            );




  gate.Setup(
    required,
    gate.depositPoint
);

        if (progressGate == null)
        {
            progressGate = gate;
        }
        currentGate = gate;


        SpawnBallsForGate(gate);


        gateIndex++;

        nextGatePosition = spawn.position + Vector3.forward * gateDistance;


    }

public void SetCurrentGate(Gate gate)
{
    currentGate = gate;

    Debug.Log("CURRENT GATE = " + gate.name);
}
    private void SpawnFinishZone()
{
    Vector3 spawnPos;


    spawnPos = playerTransform.position 
             + Vector3.forward * finishForwardOffset;
    spawnPos.x = finishPosX;
    spawnPos.y = finishPosY;


    Ray ray = new Ray(
        new Vector3(spawnPos.x, rayHeight, spawnPos.z),
        Vector3.down
    );


    if(Physics.Raycast(ray, out RaycastHit hit, 100f, roadLayer))
    {
        spawnPos.y = hit.point.y + 0.05f;
    }



        Instantiate(
            finishZonePrefab,
            spawnPos,
            Quaternion.identity
        );
    totalProgressDistance =
    spawnPos.z - progressStart.position.z;


    Debug.Log(
        "FINISH SPAWN POSITION : "
        + spawnPos
    );
}public void ContinueAfterFinish()
{

    Debug.Log("CONTINUE AFTER FINISH");


    finishMode = false;




    StartCoroutine(SpawnGateAfterFinish());

}

IEnumerator SpawnGateAfterFinish()
{
    yield return new WaitForSeconds(1f);


    SpawnNextGate();
}
  private void SpawnBallsForGate(Gate gate)
{
    if (gate.spawnArea == null)
    {
        Debug.LogWarning("Spawn Area Missing");
        return;
    }

int spawnCount =
    Mathf.Clamp(
        startSpawnCount + gateIndex * 2,
        startSpawnCount,
        maxSpawnCount
    );


    Debug.Log(
        "GATE " + gateIndex +
        " BALLS = " + spawnCount
    );


    for(int i = 0; i < spawnCount; i++)
    {
        Instantiate(
            ballPrefab,
            gate.GetRandomSpawnPoint(),
            Quaternion.identity
        );
    }
}
    public void CreateFinishFromButton()
    {
        if (finishMode)
            return;


        finishMode = true;


        foreach (Gate gate in spawnedGates)
{
    if(gate != null)
        StartCoroutine(DestroyGateRoutine(gate));
}


        spawnedGates.Clear();



        SpawnFinishZone();


        Debug.Log("FINAL FINISH STARTED");
    }
    public void FinishButtonPressed()
    {
        if (finishButtonUsed)
            return;


        finishButtonUsed = true;


        finishMode = true;


        foreach (Gate gate in spawnedGates)
        {
            if (gate != null)
                Destroy(gate.gameObject);
        }


        spawnedGates.Clear();


        SpawnFinishZone();


        Debug.Log("FINAL FINISH CREATED");
    }
    public void RemoveBallFromStack(Ball ball)
    {
        if (ball == null)
            return;


        for (int i = collectedBalls.Count - 1; i >= 0; i--)
        {
            if (collectedBalls[i] == ball)
            {
                collectedBalls.RemoveAt(i);
                break;
            }
        }


        RefreshStack();


        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetBallText(
                collectedBalls.Count
            );
        }


        Debug.Log(
            "CURRENT BALL COUNTER = "
            + collectedBalls.Count
        );
    }
public void RemoveBallFromFinish(Ball ball)
{
    if(ball == null)
        return;


    if(collectedBalls.Contains(ball))
    {
        collectedBalls.Remove(ball);
    }


    RefreshStack();


    UpdateBallUI();


    Debug.Log(
        "FINISH REMOVE | COUNT = "
        + collectedBalls.Count
    );
}


    private void UpdateBallUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetBallText(
                collectedBalls.Count
            );
        }
    }
    IEnumerator SpawnGateDelay()
    {
        yield return new WaitForSeconds(gateSpawnDelay);

        nextGateSpawned = false;

        SpawnNextGate();
    }
    IEnumerator EarlySpawnNextGate()
    {
        nextGateSpawned = true;

        yield return new WaitForSeconds(nextGateEarlySpawnTime);

        SpawnNextGate();
    }
private void UpdateProgressBar()
{
    if(playerTransform == null)
        return;


    float distance =
        playerTransform.position.z - progressStartZ;


    float progress =
        distance / progressDistance;


    progress = Mathf.Clamp01(progress);


    if(UIManager.Instance != null)
    {
        UIManager.Instance.UpdateProgress(progress);
    }


    // وقتی به 130 رسید دوباره صفر شود
    if(distance >= progressDistance)
    {
        progressStartZ = playerTransform.position.z;

        UIManager.Instance.UpdateProgress(0f);
    }
}
    private void Update()
    {
        UpdateProgressBar();
    }
private void ResetProgressBar()
{
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateProgress(0f);
    }
}
}