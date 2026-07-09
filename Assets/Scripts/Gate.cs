using UnityEngine;
using TMPro;
using DG.Tweening;

public class Gate : MonoBehaviour
{
    [Header("Gate Settings")]
    public int requiredBalls = 3;

    [Header("Spawn Settings")]
    public Transform spawnCenter;
    public Vector3 spawnSize = new Vector3(15f, 0f, 30f);
    public int spawnCount = 20;

    [Header("Deposit")]
    public Transform depositPoint;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Transform textTransform;

    [Header("Effects")]
    [SerializeField] private ParticleSystem confettiFX;

    [HideInInspector] public LevelManager spawner;

    private int currentProgress = 0;
    private bool activated = false;

    //================ INIT =================//

    public void Setup(int required, int spawn, Transform center, Transform deposit)
    {
        requiredBalls = required;
        spawnCount = spawn;
        spawnCenter = center;
        depositPoint = deposit;

        currentProgress = 0;
        activated = false;

        UpdateText();
    }

    //================ PROGRESS =================//

    public void AddProgress()
    {
        currentProgress = Mathf.Min(currentProgress + 1, requiredBalls);
        UpdateText();

        if (textTransform != null)
        {
            textTransform.DOKill();
            textTransform.localScale = Vector3.one;
            textTransform.DOPunchScale(Vector3.one * 0.25f, 0.25f, 10, 1f);
        }
    }

    private void UpdateText()
    {
        if (text != null)
            text.text = $"{currentProgress} / {requiredBalls}";
    }

    private void LateUpdate()
    {
        if (text != null && Camera.main != null)
            text.transform.forward = Camera.main.transform.forward;
    }

    //================ SPAWN POINT =================//

    public Vector3 GetRandomSpawnPoint()
    {
        if (spawnCenter == null)
            return transform.position;

        Vector3 c = spawnCenter.position;

        return new Vector3(
            c.x + Random.Range(-spawnSize.x * 0.5f, spawnSize.x * 0.5f),
            c.y,
            c.z + Random.Range(-spawnSize.z * 0.5f, spawnSize.z * 0.5f)
        );
    }

    //================ TRIGGER =================//

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        activated = true;

        if (LevelManager.Instance == null)
            return;

        // ❗ چک تعداد توپ‌ها
        if (LevelManager.Instance.GetBallCount() < requiredBalls)
        {
            LevelManager.Instance.LoseFromGate();
            return;
        }

        // FX
        if (confettiFX != null && depositPoint != null)
        {
            confettiFX.transform.position = depositPoint.position;
            confettiFX.Play();
        }

        // شروع گیت
        LevelManager.Instance.StartGate(requiredBalls, depositPoint);

        // حذف این گیت از لیست اسپاونر
        if (spawner != null)
            spawner.GatePassed(this);
    }
}