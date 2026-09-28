using UnityEngine;

/// <summary>
/// 공격/패링 테스트용 샌드백.
/// 맞으면 빨갛게, 패링 판정에 닿으면 초록색으로 번쩍이고 콘솔에 로그를 남긴다. Collider가 필요하다.
/// </summary>
public class TrainingDummy : MonoBehaviour, IDamageable, IParryable
{
    [SerializeField] private float maxHealth = 100f;

    [Header("Flash")]
    [SerializeField] private float flashTime = 0.1f;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private Color parryColor = new Color(0.2f, 1f, 0.3f, 1f);
    [Tooltip("패링 성공 시 초록색이 유지되는 시간 (피격보다 길게 두면 눈에 잘 띈다)")]
    [SerializeField] private float parryFlashTime = 0.3f;

    private float health;
    private Renderer rend;
    private Color baseColor;
    private float flashTimer;

    private void Awake()
    {
        health = maxHealth;
        rend = GetComponentInChildren<Renderer>();
        if (rend != null) baseColor = rend.material.color;
    }

    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        health -= amount;
        Debug.Log($"[{name}] 피격! 데미지 {amount}, 남은 체력 {health}");

        Flash(hitColor, flashTime);

        if (health <= 0f) health = maxHealth; // 무한 샌드백
    }

    public void OnParried(Vector3 hitPoint)
    {
        Debug.Log($"[{name}] 패링 판정!");

        Flash(parryColor, parryFlashTime);
    }

    // 색을 바꾸고 duration 뒤에 원래 색으로 되돌린다. (나중에 호출된 색이 우선)
    private void Flash(Color color, float duration)
    {
        if (rend == null) return;

        rend.material.color = color;
        flashTimer = duration;
    }

    private void Update()
    {
        if (flashTimer <= 0f) return;

        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f && rend != null) rend.material.color = baseColor;
    }
}