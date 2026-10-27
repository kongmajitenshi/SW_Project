using UnityEngine;

/// <summary>
/// 스태미나가 부족한 상태에서 행동을 시도하면 플레이어 그림을 좌우로 떨고 효과음을 낸다.
/// 실제 위치(Rigidbody2D)가 아니라 그림이 붙은 자식 오브젝트만 흔들어서 물리/이동에 영향을 주지 않는다.
/// </summary>
[RequireComponent(typeof(PlayerStamina))]   // [추가]
public class StaminaFailFeedback : MonoBehaviour
{
    [Header("References")]
    // [삭제] player 칸 → Awake에서 자동으로 찾음
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

    private PlayerStamina stamina;   // [수정] PlayerHealthStamina player → PlayerStamina, 자동 연결
    private Vector3 baseLocalPos;
    private float shakeTimer;
    private float lastPlayTime = -999f;

    private void Awake()
    {
        stamina = GetComponent<PlayerStamina>();   // [추가]

        if (stamina == null || shakeTarget == null)
        {
            Debug.LogError($"{nameof(StaminaFailFeedback)}: PlayerStamina가 없거나 shakeTarget이 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        if (shakeTarget == transform || shakeTarget.GetComponent<Rigidbody2D>() != null)
        {
            Debug.LogError($"{nameof(StaminaFailFeedback)}: shakeTarget에 Player 본체가 들어가 있습니다. " +
                           "그림(SpriteRenderer)이 붙은 '자식' 오브젝트를 넣어주세요.", this);
            enabled = false;
            return;
        }

        baseLocalPos = shakeTarget.localPosition;
    }

    private void OnEnable()
    {
        if (stamina != null) stamina.OnStaminaExhausted += Play;   // [수정]
    }

    private void OnDisable()
    {
        if (stamina != null) stamina.OnStaminaExhausted -= Play;   // [수정]

        if (shakeTarget != null) shakeTarget.localPosition = baseLocalPos;
        shakeTimer = 0f;
    }

    private void Play()
    {
        if (Time.time < lastPlayTime + minInterval) return;
        lastPlayTime = Time.time;

        shakeTimer = duration;

        if (audioSource != null && failSound != null)
            audioSource.PlayOneShot(failSound);
    }

    private void LateUpdate()
    {
        if (shakeTimer <= 0f) return;

        shakeTimer -= Time.deltaTime;

        if (shakeTimer <= 0f)
        {
            shakeTarget.localPosition = baseLocalPos;
            return;
        }

        float elapsed = duration - shakeTimer;
        float fade = shakeTimer / duration;
        float offsetX = Mathf.Sin(elapsed * frequency * Mathf.PI * 2f) * strength * fade;

        shakeTarget.localPosition = baseLocalPos + new Vector3(offsetX, 0f, 0f);
    }
}