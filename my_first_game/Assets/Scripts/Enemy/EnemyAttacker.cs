using UnityEngine;

/// <summary>
/// 테스트용 적 공격 루프: 대기 → 예고(선딜) → 적중 → 후딜 → 대기 ...
/// 적중하는 순간 PlayerParry.Judge()로 패리 여부를 묻고, 결과에 따라 반응한다.
/// 색 변경은 직접 하지 않고 EnemyVisual에 요청한다.
/// 적이 쓰러지거나 플레이어가 사망하면 공격을 멈춘다.
/// 공격 범위는 아직 없다 (전투 중 적은 항상 화면 안에 있다는 전제의 테스트 버전).
/// </summary>
[RequireComponent(typeof(Enemy), typeof(EnemyVisual))]
public class EnemyAttacker : MonoBehaviour
{
    private enum State { Idle, Windup, Recovery, Staggered }

    [Header("공격 예고 색")]
    [SerializeField] private Color parryableColor = new Color(1f, 0.85f, 0.2f);   // 노랑: 패리 가능
    [SerializeField] private Color unparryableColor = new Color(0.7f, 0.3f, 1f);  // 보라: 패리 불가
    [SerializeField] private Color staggeredColor = new Color(0.4f, 0.7f, 1f);    // 파랑: 그로기

    [Header("패링 당했을 때")]
    [Tooltip("퍼펙트 패리를 당하면 이 시간 동안 아무것도 못 한다 (플레이어의 반격 기회)")]
    [SerializeField, Min(0f)] private float perfectStaggerTime = 1.2f;

    private Enemy enemy;
    private EnemyVisual visual;
    private PlayerParry playerParry;
    private PlayerHealth playerHealth;   // [수정] PlayerHealthStamina → PlayerHealth

    private State state = State.Idle;
    private float timer;
    private int nextAttackIndex;
    private EnemyAttackData currentAttack;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        visual = GetComponent<EnemyVisual>();
    }

    private void OnEnable()
    {
        if (enemy != null) enemy.Died += OnEnemyDied;
    }

    private void OnDisable()
    {
        if (enemy != null) enemy.Died -= OnEnemyDied;
    }

    private void Start()
    {
        if (visual == null)
        {
            Debug.LogError($"{nameof(EnemyAttacker)}: 같은 오브젝트에 EnemyVisual 컴포넌트를 추가해주세요.", this);
            enabled = false;
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerParry = player.GetComponent<PlayerParry>();
            playerHealth = player.GetComponent<PlayerHealth>();   // [수정]
        }

        if (playerHealth == null)
        {
            Debug.LogError($"{nameof(EnemyAttacker)}: 'Player' 태그가 붙은 PlayerHealth를 찾지 못했습니다.", this);   // [수정]
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
        if (playerHealth.IsDead)
        {
            if (state != State.Idle) BackToIdle();
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f) return;

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
        nextAttackIndex = (nextAttackIndex + 1) % attacks.Length;

        state = State.Windup;
        timer = currentAttack.windupTime;
        visual.SetStateColor(currentAttack.parryable ? parryableColor : unparryableColor);

        Debug.Log($"[{enemy.Data.enemyName}] {currentAttack.attackName} 준비 " +
                  $"({(currentAttack.parryable ? "패리 가능" : "패리 불가")})");
    }

    private void Impact()
    {
        ParryResult result = playerParry != null ? playerParry.Judge(currentAttack) : ParryResult.None;

        switch (result)
        {
            case ParryResult.Perfect:
                Debug.Log($"[{enemy.Data.enemyName}] 퍼펙트 패리 당함! {perfectStaggerTime}초 그로기");
                state = State.Staggered;
                timer = perfectStaggerTime;
                visual.SetStateColor(staggeredColor);
                break;

            case ParryResult.Normal:
                Debug.Log($"[{enemy.Data.enemyName}] 공격이 패리됨");
                state = State.Recovery;
                timer = currentAttack.recoveryTime;
                visual.ClearStateColor();
                break;

            default:
                playerHealth.TakeDamage(currentAttack.damage, playerHealth.transform.position);
                state = State.Recovery;
                timer = currentAttack.recoveryTime;
                visual.ClearStateColor();
                break;
        }
    }

    private void BackToIdle()
    {
        state = State.Idle;
        timer = enemy.Data.attackInterval;
        visual.ClearStateColor();
    }

    private void OnEnemyDied()
    {
        enabled = false;
    }
}