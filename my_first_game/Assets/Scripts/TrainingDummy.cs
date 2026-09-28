using UnityEngine;

/// <summary>공격 테스트용 샌드백. 맞으면 빨갛게 번쩍이고 콘솔에 로그를 남긴다. Collider가 필요하다.</summary>
public class TrainingDummy : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float flashTime = 0.1f;

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

        if (rend != null)
        {
            rend.material.color = Color.red;
            flashTimer = flashTime;
        }

        if (health <= 0f) health = maxHealth; // 무한 샌드백
    }

    private void Update()
    {
        if (flashTimer <= 0f) return;

        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f && rend != null) rend.material.color = baseColor;
    }
}
