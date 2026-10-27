using UnityEngine;

/// <summary>
/// 적의 그림 색을 한 곳에서 관리한다. 색은 이 스크립트만 바꾼다.
/// - 상태 색(공격 예고, 그로기, 사망 등): 다른 스크립트가 요청, 바꿀 때까지 유지
/// - 피격 번쩍임: 상태 색 위에 잠깐 덮어썼다가, 끝나면 그 순간의 상태 색으로 돌아감
/// </summary>
[RequireComponent(typeof(Enemy))]
public class EnemyVisual : MonoBehaviour
{
    [Tooltip("색을 바꿀 그림 (비워두면 자식까지 찾아서 자동 연결)")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("피격 번쩍임")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField, Min(0.01f)] private float hitFlashTime = 0.1f;

    [Header("사망")]                                                              // [추가]
    [SerializeField] private Color deadColor = new Color(0.4f, 0.4f, 0.4f);      // [추가] 회색

    private Enemy enemy;
    private Color baseColor;   // 그림의 원래 색
    private Color stateColor;  // 지금 상태가 원하는 색
    private float flashTimer;  // 남은 번쩍임 시간

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"{nameof(EnemyVisual)}: SpriteRenderer를 찾지 못했습니다.", this);
            enabled = false;
            return;
        }

        baseColor = spriteRenderer.color;
        stateColor = baseColor;
    }

    private void OnEnable()
    {
        if (enemy == null) return;
        enemy.Damaged += OnDamaged;
        enemy.Died += OnDied;        // [추가]
    }

    private void OnDisable()
    {
        if (enemy == null) return;
        enemy.Damaged -= OnDamaged;
        enemy.Died -= OnDied;        // [추가]
    }

    /// <summary>상태 색을 바꾼다 (공격 예고, 그로기 등). 다시 바꿀 때까지 유지된다.</summary>
    public void SetStateColor(Color color)
    {
        stateColor = color;
        Apply();
    }

    /// <summary>상태 색을 원래 색으로 되돌린다.</summary>
    public void ClearStateColor()
    {
        stateColor = baseColor;
        Apply();
    }

    private void OnDamaged(float amount, Vector3 hitPoint)
    {
        flashTimer = hitFlashTime;
        Apply();
    }

    // [추가] 쓰러지면 상태 색을 회색으로. 막타의 빨간 번쩍임이 끝난 뒤 회색이 된다.
    private void OnDied()
    {
        stateColor = deadColor;
        Apply();
    }

    private void Update()
    {
        if (flashTimer <= 0f) return;

        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f) Apply(); // 번쩍임 끝 → 그 순간의 상태 색으로
    }

    // 실제로 그림 색을 바꾸는 곳은 여기 한 군데뿐
    private void Apply()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = flashTimer > 0f ? hitColor : stateColor;
    }
}