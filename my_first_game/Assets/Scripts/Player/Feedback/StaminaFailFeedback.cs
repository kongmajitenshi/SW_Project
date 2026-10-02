using UnityEngine;

/// <summary>
/// 스태미나가 부족한 상태에서 행동을 시도하면 플레이어 그림을 좌우로 떨고 효과음을 낸다.
/// 실제 위치(Rigidbody2D)가 아니라 그림이 붙은 자식 오브젝트만 흔들어서 물리/이동에 영향을 주지 않는다.
/// </summary>
public class StaminaFailFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealthStamina player;
    [Tooltip("흔들 대상. 플레이어 그림(SpriteRenderer)이 붙은 '자식' 오브젝트를 넣는다. Rigidbody2D가 붙은 Player 자체는 넣지 말 것.")]
    [SerializeField] private Transform shakeTarget;
    [Tooltip("효과음을 재생할 AudioSource (없으면 소리 없이 떨림만)")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("스태미나 부족 효과음 (없으면 소리 없이 떨림만)")]
    [SerializeField] private AudioClip failSound;

    [Header("Shake")]
    [Tooltip("떨리는 시간(초)")]
    [SerializeField, Min(0.01f)] private float duration = 0.2f;
    [Tooltip("최대 흔들림 폭 (월드 단위). 너무 크면 순간이동처럼 보인다.")]
    [SerializeField, Min(0f)] private float strength = 0.06f;
    [Tooltip("1초에 좌우로 몇 번 왕복하는지")]
    [SerializeField, Min(1f)] private float frequency = 20f;
    [Tooltip("연타 시 떨림/소리가 다시 시작되기까지의 최소 간격(초)")]
    [SerializeField, Min(0f)] private float minInterval = 0.1f;

    private Vector3 baseLocalPos;   // 흔들기 전 원래 자리 (항상 여기를 기준으로 흔든다)
    private float shakeTimer;       // 남은 떨림 시간
    private float lastPlayTime = -999f;

    private void Awake()
    {
        if (player == null || shakeTarget == null)
        {
            Debug.LogError($"{nameof(StaminaFailFeedback)}: player 또는 shakeTarget이 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        baseLocalPos = shakeTarget.localPosition;
    }

    private void OnEnable()
    {
        if (player != null) player.OnStaminaExhausted += Play;
    }

    private void OnDisable()
    {
        if (player != null) player.OnStaminaExhausted -= Play;

        // 떨리는 도중에 꺼지면 그림이 어긋난 자리에 멈추지 않도록 원위치
        if (shakeTarget != null) shakeTarget.localPosition = baseLocalPos;
        shakeTimer = 0f;
    }

    private void Play()
    {
        // 너무 짧은 간격의 연타는 무시 (소리가 겹쳐서 시끄러워지는 것 방지)
        if (Time.time < lastPlayTime + minInterval) return;
        lastPlayTime = Time.time;

        shakeTimer = duration; // 떨리는 중이었다면 처음부터 다시

        if (audioSource != null && failSound != null)
            audioSource.PlayOneShot(failSound);
    }

    // Animator는 Update와 LateUpdate 사이에 위치를 덮어쓸 수 있으므로,
    // 그 뒤인 LateUpdate에서 흔들어야 떨림이 사라지지 않는다.
    private void LateUpdate()
    {
        if (shakeTimer <= 0f) return;

        shakeTimer -= Time.deltaTime;

        if (shakeTimer <= 0f)
        {
            shakeTarget.localPosition = baseLocalPos; // 끝나면 정확히 원래 자리로
            return;
        }

        float elapsed = duration - shakeTimer;       // 떨림 시작 후 지난 시간
        float fade = shakeTimer / duration;          // 1 → 0 으로 줄어듦 (점점 약해지게)
        float offsetX = Mathf.Sin(elapsed * frequency * Mathf.PI * 2f) * strength * fade;

        shakeTarget.localPosition = baseLocalPos + new Vector3(offsetX, 0f, 0f);
    }
}