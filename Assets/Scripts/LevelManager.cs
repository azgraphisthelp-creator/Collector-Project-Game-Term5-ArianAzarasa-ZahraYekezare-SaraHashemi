using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Camera")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private GameObject gameplayCamera;
    [SerializeField] private GameObject endCamera;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Stack")]
    public Transform stackPoint;

    [Header("Gate Settings")]
    [SerializeField] private Gate gatePrefab;
    [SerializeField] private Transform[] gateSpawnPoints;

    [System.Serializable]
    public class GateData
    {
        public int requiredBalls;
        public int spawnCount;
        public Transform spawnCenter;
    }

    [SerializeField] private GateData[] gates;

    [Header("Ball")]
    [SerializeField] private Ball ballPrefab;

    [Header("Timing")]
    [SerializeField] private float destroyDelay = 10f;
    [SerializeField] private float nextGateEarlySpawnTime = 2f;

    private Queue<Gate> activeGates = new Queue<Gate>();
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
        SpawnFirstGate();
        SwitchToGameplayCamera();
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

    void SpawnFirstGate()
    {
        if (gates.Length == 0)
            return;

        nextGateIndex = 0;

        GateData data = gates[nextGateIndex];
        Transform spawn = gateSpawnPoints[nextGateIndex];

        Gate gate = Instantiate(
            gatePrefab,
            spawn.position,
            spawn.rotation);

        gate.spawner = this;

        gate.Setup(
            data.requiredBalls,
            data.spawnCount,
            data.spawnCenter,
            gate.depositPoint);

        currentGate = gate;

        activeGates.Enqueue(gate);

        SpawnBallsForGate(gate);

        nextGateIndex++;
    }


    public void SpawnNextGate()
    {
        if (nextGateSpawned)
            return;

        if (nextGateIndex >= gates.Length)
            return;

        nextGateSpawned = true;

        GateData data = gates[nextGateIndex];
        Transform spawn = gateSpawnPoints[nextGateIndex];

        Gate gate = Instantiate(
            gatePrefab,
            spawn.position,
            spawn.rotation);

        gate.spawner = this;

        gate.Setup(
            data.requiredBalls,
            data.spawnCount,
            data.spawnCenter,
            gate.depositPoint);

        currentGate = gate;

        activeGates.Enqueue(gate);

        SpawnBallsForGate(gate);

        nextGateIndex++;
    }


    void SpawnBallsForGate(Gate gate)
    {
        if (gate.spawnCenter == null)
            return;

        for (int i = 0; i < gate.spawnCount; i++)
        {
            Instantiate(
                ballPrefab,
                gate.GetRandomSpawnPoint(),
                Quaternion.identity);
        }
    }


    public void GatePassed(Gate gate)
    {
        StartCoroutine(DestroyGateRoutine(gate));
    }

    IEnumerator DestroyGateRoutine(Gate gate)
    {
        yield return new WaitForSeconds(destroyDelay);

        if (gate != null)
        {
            if (currentGate == gate)
                currentGate = null;

            if (activeGates.Count > 0)
                activeGates.Dequeue();

            Destroy(gate.gameObject);
        }

        nextGateSpawned = false;
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
            currentGate.AddProgress();

        ball.gameObject.SetActive(false);

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
        SwitchToGameplayCamera();

        StartCoroutine(EarlySpawnNextGate());
    }


    IEnumerator EarlySpawnNextGate()
    {
        yield return new WaitForSeconds(nextGateEarlySpawnTime);

        SpawnNextGate();
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

    Time.timeScale = 1f;
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}

    public void NextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}