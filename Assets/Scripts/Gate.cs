using UnityEngine;
using TMPro;

public class Gate : MonoBehaviour
{
    private bool progressStarted;
    [Header("Progress")]
public Transform progressStart;
public Transform progressEnd;
    [Header("Gate Settings")]
private int requiredBalls;
    [Header("Spawn Settings")]
    public Collider spawnArea;


    [Header("Deposit")]
    public Transform depositPoint;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI text;

    [Header("Effects")]
    [SerializeField] private ParticleSystem confettiFX;

    [HideInInspector]
    public LevelManager spawner;

    private int currentProgress;
    private bool activated;

  public void Setup(int required, Transform deposit)
{
    requiredBalls = required;
    depositPoint = deposit;

    currentProgress = 0;
    activated = false;

    UpdateText();
}

    public void AddProgress()
    {
        if (currentProgress >= requiredBalls)
            return;

        currentProgress++;

        Debug.Log($"CURRENT GATE TEXT = {currentProgress} / {requiredBalls}");

        UpdateText();
    }

    private void UpdateText()
    {
        if (text == null)
        {
            Debug.LogError($"TEXT IS NULL ON {name}");
            return;
        }

        text.text = $"{currentProgress} / {requiredBalls}";

        Debug.Log("TEXT UPDATED => " + text.text);
    }

    private void LateUpdate()
    {
        if (text != null && Camera.main != null)
            text.transform.forward = Camera.main.transform.forward;
    }

  public Vector3 GetRandomSpawnPoint()
{
    if (spawnArea == null)
        return transform.position;

    Bounds bounds = spawnArea.bounds;

    return new Vector3(
        Random.Range(bounds.min.x, bounds.max.x),
        bounds.center.y,
        Random.Range(bounds.min.z, bounds.max.z)
    );
}
    private void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (LevelManager.Instance == null)
            return;

        if (LevelManager.Instance.GetBallCount() < requiredBalls)
        {
            activated = true;
            LevelManager.Instance.LoseFromGate();
            return;
        }

        activated = true;

        if (confettiFX != null && depositPoint != null)
        {
            confettiFX.transform.position = depositPoint.position;
            confettiFX.Play();
        }

        LevelManager.Instance.SetCurrentGate(this);

        LevelManager.Instance.StartGate(requiredBalls, depositPoint);
    }


}