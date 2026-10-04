using UnityEngine;

/// <summary>
/// 테스트용 적 공격 루프: 대기 → 예고(선딜) → 적중 → 후딜 → 대기 ...
/// 적중하는 순간 PlayerParry.Judge()로 패링 여부를 묻고, 결과에 따라 반응한다.
/// 공격 범위는 아직 없다 (전투 중 적은 항상 화면 안에 있다는 전제의 테스트 버전).
/// </summary>
[RequireComponent(typeof(Enemy))]
public class EnemyAttacker : MonoBehaviour
{
    private enum State { Idle, Windup, Recovery, Staggered }

    [Header("References")]
    [Tooltip("공격 예고 색을 표시할 그림 (비워두면 자식까지 찾아서 자동 연결)")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("공격 예고 색")]
    [SerializeField] private Color parryableColor = new Color(1f, 0.85f, 0.2f);   // 노랑: 패리 가능
    [SerializeField] private Color unparryableColor = new Color(1f, 0.2f, 0.2f);  // 빨강: 패리 불가
    [SerializeField] private Color staggeredColor = new Color(0.4f, 0.7f, 1f);    // 파랑: 그로기

    [Header("패링 당했을 때")]
    [Tooltip("퍼펙트 패리를 당하면 이 시간 동안 아무것도 못 한다 (플레이어의 반격 기회)")]
    [SerializeField, Min(0f)] private float perfectStaggerTime = 1.2f;

    private Enemy enemy;
    private PlayerParry playerParry;
    private PlayerHealthStamina playerHealth;
    private Color baseColor;

    private State state = State.Idle;
    private float timer;
    private int nextAttackIndex;
    private EnemyAttackData currentAttack;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) baseColor = spriteRenderer.color;
    }

    private void Start()
    {
        // 프리팹은 씬 안의 오브젝트를 미리 연결해 둘 수 없으므로, 시작할 때 플레이어를 찾는다
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerParry = player.GetComponent<PlayerParry>();
            playerHealth = player.GetComponent<PlayerHealthStamina>();
        }

        if (playerHealth == null)
        {
            Debug.LogError($"{nameof(EnemyAttacker)}: 'Player' 태그가 붙은 PlayerHealthStamina를 찾지 못했습니다.", this);
            enabled = false;
            return;
        }

        if (enemy.Data == null || enemy.Data.attacks == null || enemy.Data.attacks.Length == 0)
        {
            Debug.LogError($"{nameof(EnemyAttacker)}: EnemyData에 공격이 하나도 없습니다.", this);
            enabled = false;
            return;
        }

        timer = enemy.Data.attackInterval;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        // 타이머가 끝나면 현재 상태에 따라 다음 단계로
        switch (state)
        {
            case State.Idle:      StartWindup(); break;
            case State.Windup:    Impact();      break;
            case State.Recovery:
            case State.Staggered: BackToIdle();  break;
        }
    }

    private void StartWindup()
    {
        EnemyAttackData[] attacks = enemy.Data.attacks;
        currentAttack = attacks[nextAttackIndex];
        nextAttackIndex = (nextAttackIndex + 1) % attacks.Length; // 마지막 다음엔 다시 0번으로

        state = State.Windup;
        timer = currentAttack.windupTime;
        SetColor(currentAttack.parryable ? parryableColor : unparryableColor);

        Debug.Log($"[{enemy.Data.enemyName}] {currentAttack.attackName} 준비 " +
                  $"({(currentAttack.parryable ? "패리 가능" : "패리 불가")})");
    }

    // 선딜이 끝나는 순간 = 공격이 적중하는 순간
    private void Impact()
    {
        ParryResult result = playerParry != null ? playerParry.Judge(currentAttack) : ParryResult.None;

        switch (result)
        {
            case ParryResult.Perfect:
                Debug.Log($"[{enemy.Data.enemyName}] 퍼펙트 패리 당함! {perfectStaggerTime}초 그로기");
                state = State.Staggered;
                timer = perfectStaggerTime;
                SetColor(staggeredColor);
                break;

            case ParryResult.Normal:
                Debug.Log($"[{enemy.Data.enemyName}] 공격이 패리됨");
                state = State.Recovery;
                timer = currentAttack.recoveryTime;
                SetColor(baseColor);
                break;

            default: // 패리 실패 or 패리 불가 공격 → 플레이어 피격
                playerHealth.TakeDamage(currentAttack.damage, playerHealth.transform.position);
                state = State.Recovery;
                timer = currentAttack.recoveryTime;
                SetColor(baseColor);
                break;
        }
    }

    private void BackToIdle()
    {
        state = State.Idle;
        timer = enemy.Data.attackInterval;
        SetColor(baseColor);
    }

    private void SetColor(Color color)
    {
        if (spriteRenderer != null) spriteRenderer.color = color;
    }
}