using TMPro;
using UnityEngine;

/// <summary>
/// 스태미나가 부족한 순간, 그때의 플레이어 위치(머리 위)에 경고 문구를 잠깐 띄운다.
/// 문구 내용은 연결된 텍스트 오브젝트에 직접 적어둔 글자를 그대로 사용한다.
/// 나중에 DOTween 연출을 넣을 때는 Show() / Hide() 안쪽만 바꾸면 된다.
/// </summary>
public class StaminaWarningUI : MonoBehaviour
{
    [SerializeField] private PlayerHealthStamina player;
    [Tooltip("경고 문구를 표시할 TextMeshPro 텍스트 (문구는 이 오브젝트에 직접 입력)")]
    [SerializeField] private TMP_Text warningText;
    [Tooltip("문구가 화면에 떠 있는 시간(초)")]
    [SerializeField, Min(0.1f)] private float showDuration = 1f;
    [Tooltip("플레이어 위치에서 얼마나 위에 띄울지 (월드 단위)")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);

    private Camera cam;
    private Vector3 anchorWorldPos; // 경고가 뜬 순간의 월드 위치 (이후 플레이어가 움직여도 고정)
    private float hideTimer;

    private void Awake()
    {
        cam = Camera.main;
        if (cam == null)
            Debug.LogWarning($"{nameof(StaminaWarningUI)}: 'MainCamera' 태그가 붙은 카메라를 찾지 못했습니다.", this);

        if (warningText != null)
            warningText.gameObject.SetActive(false); // 처음엔 숨겨둔다
    }

    private void OnEnable()
    {
        if (player != null) player.OnStaminaExhausted += Show;
    }

    private void OnDisable()
    {
        if (player != null) player.OnStaminaExhausted -= Show;
    }

    // 카메라 추적 스크립트는 보통 LateUpdate에서 카메라를 움직이므로,
    // 그와 같은 타이밍에 위치를 계산해야 글자가 덜덜 떨리지 않는다.
    private void LateUpdate()
    {
        if (hideTimer <= 0f) return;

        UpdateScreenPosition();

        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f) Hide();
    }

    private void Show()
    {
        if (warningText == null || player == null) return;

        // [추가] 이미 경고가 떠 있는 동안에는 새로 띄우지 않는다 (위치·시간 모두 그대로 유지)
        if (hideTimer > 0f) return;

        anchorWorldPos = player.transform.position + worldOffset;

        warningText.gameObject.SetActive(true);
        UpdateScreenPosition();
        hideTimer = showDuration;
    }

    private void Hide()
    {
        if (warningText == null) return;

        warningText.gameObject.SetActive(false);
    }

    // 기억해둔 월드 위치가 지금 화면의 어디에 해당하는지 계산해서 글자를 옮긴다
    private void UpdateScreenPosition()
    {
        if (cam == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(anchorWorldPos);
        screenPos.z = 0f; // z는 카메라와의 거리값이라 UI에서는 필요 없음
        warningText.rectTransform.position = screenPos;
    }
}