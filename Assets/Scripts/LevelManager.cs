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
    public Transform stackPoint;

    [Header("Gate Settings (PER LEVEL)")]
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

    [Header("Timing")]
    [SerializeField] private float destroyDelay = 10f;
    [SerializeField] private float nextGateEarlySpawnTime = 2f;

    private Queue<Gate> activeGates = new Queue<Gate>();
    private Gate currentGate;
    private int nextGateIndex;

    [Header("Ball")]
    [SerializeField] private Ball ballPrefab;

    public List<Ball> collectedBalls = new List<Ball>();
    private List<Ball> ballsToDeposit = new List<Ball>();

    private Transform depositPoint;
    private bool gateRunning;

    private int requiredBalls;
    private int depositedBalls;

    private bool nextGateSpawned;

    private Coroutine gateRoutine;

    //================ INIT =================//

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

    //================ CAMERA =================//

    private void SwitchToGameplayCamera()
    {
        endCamera.SetActive(false);
        gameplayCamera.SetActive(true);
        cameraFollow.StartFollow();
    }

    private void SwitchToEndCamera()
    {
        gameplayCamera.SetActive(false);
        endCamera.SetActive(true);
        cameraFollow.StopFollow();
    }

    //================ BALL STACK =================//

    public void AddBall(Ball ball)
    {
        if (ball == null) return;
        if (collectedBalls.Contains(ball)) return;

        collectedBalls.Add(ball);
        ball.SetStackIndex(collectedBalls.Count - 1);

        UIManager.Instance.SetBallText(collectedBalls.Count);
    }

    public int GetBallCount() => collectedBalls.Count;

    public Vector3 GetBallPosition(int index)
    {
        float spacingX = .7f;
        float spacingZ = .7f;
        int rowSize = 3;

        int row = index / rowSize;
        int col = index % rowSize;

        float x = (col - 1) * spacingX;
        float z = -row * spacingZ;

        return stackPoint.position + new Vector3(x, 0, z);
    }

    //================ FIRST GATE =================//

    private void SpawnFirstGate()
    {
        if (gates.Length == 0) return;

        nextGateIndex = 0;

        GateData data = gates[0];
        Transform point = gateSpawnPoints[0];

        Gate gate = Instantiate(gatePrefab, point.position, point.rotation);

        gate.spawner = this;
        gate.Setup(
            data.requiredBalls,
            data.spawnCount,
            data.spawnCenter,
            point
        );

        currentGate = gate;
        activeGates.Enqueue(gate);

        SpawnBallsForGate(gate);

        nextGateIndex = 1;
    }

    //================ NEXT GATE =================//

    public void SpawnNextGate()
    {
        if (nextGateSpawned) return;
        if (nextGateIndex >= gates.Length) return;

        nextGateSpawned = true;

        GateData data = gates[nextGateIndex];
        Transform point = gateSpawnPoints[nextGateIndex];

        Gate gate = Instantiate(gatePrefab, point.position, point.rotation);

        gate.spawner = this;
        gate.Setup(
            data.requiredBalls,
            data.spawnCount,
            data.spawnCenter,
            point
        );

        currentGate = gate;
        activeGates.Enqueue(gate);

        SpawnBallsForGate(gate);

        nextGateIndex++;
    }

    private IEnumerator EarlySpawnNextGate()
    {
        yield return new WaitForSeconds(nextGateEarlySpawnTime);
        SpawnNextGate();
    }

    //================ BALL SPAWN =================//

    private void SpawnBallsForGate(Gate gate)
    {
        if (gate.spawnCenter == null)
        {
            Debug.LogWarning("SpawnCenter is NULL on Gate!");
            return;
        }

        for (int i = 0; i < gate.spawnCount; i++)
        {
            Instantiate(
                ballPrefab,
                gate.GetRandomSpawnPoint(),
                Quaternion.identity
            );
        }
    }

    //================ GATE FLOW =================//

    public void GatePassed(Gate gate)
    {
        StartCoroutine(DestroyGateRoutine(gate));
    }

    private IEnumerator DestroyGateRoutine(Gate gate)
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

    //================ START GATE =================//

    public void StartGate(int required, Transform target)
    {
        if (gateRunning) return;

        gateRunning = true;

        requiredBalls = required;
        depositedBalls = 0;

        depositPoint = target;
        ballsToDeposit.Clear();

        int count = Mathf.Min(required, collectedBalls.Count);

        for (int i = 0; i < count; i++)
            ballsToDeposit.Add(collectedBalls[i]);

        playerMovement.StopMovement();
        SwitchToEndCamera();

        if (gateRoutine != null)
            StopCoroutine(gateRoutine);

        gateRoutine = StartCoroutine(SendBallsRoutine());
    }

    private IEnumerator SendBallsRoutine()
    {
        List<Ball> snapshot = new List<Ball>(ballsToDeposit);

        for (int i = 0; i < snapshot.Count; i++)
        {
            if (snapshot[i] != null)
                snapshot[i].GoToDeposit(depositPoint.position);

            yield return new WaitForSeconds(0.08f);
        }
    }

    //================ BALL DEPOSIT =================//

    public void BallDeposited(Ball ball)
    {
        if (!gateRunning) return;

        if (ballsToDeposit.Contains(ball))
            ballsToDeposit.Remove(ball);

        if (collectedBalls.Contains(ball))
            collectedBalls.Remove(ball);

        depositedBalls++;

        ball.gameObject.SetActive(false);

        UIManager.Instance.SetBallText(collectedBalls.Count);

        if (currentGate != null)
            currentGate.AddProgress();

        if (ballsToDeposit.Count <= 0)
            StartCoroutine(FinishGateRoutine());
    }

    //================ FIXED FLOW =================//

    private IEnumerator FinishGateRoutine()
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

        SwitchToGameplayCamera();

        StartCoroutine(EarlySpawnNextGate());
    }

    //================ LOSE FIXED =================//

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

    //================ UI =================//

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}