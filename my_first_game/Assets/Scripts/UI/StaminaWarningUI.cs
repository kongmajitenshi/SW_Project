using TMPro;
using UnityEngine;

/// <summary>
/// 스태미나가 부족한 순간, 그때의 플레이어 위치(머리 위)에 경고 문구를 잠깐 띄운다.
/// 문구 내용은 연결된 텍스트 오브젝트에 직접 적어둔 글자를 그대로 사용한다.
/// </summary>
public class StaminaWarningUI : MonoBehaviour
{
    [Tooltip("플레이어의 PlayerStamina (Player 오브젝트를 드래그)")]
    [SerializeField] private PlayerStamina player;   // [수정] PlayerHealthStamina → PlayerStamina
    [Tooltip("경고 문구를 표시할 TextMeshPro 텍스트 (문구는 이 오브젝트에 직접 입력)")]
    [SerializeField] private TMP_Text warningText;
    [Tooltip("문구가 화면에 떠 있는 시간(초)")]
    [SerializeField, Min(0.1f)] private float showDuration = 1f;
    [Tooltip("플레이어 위치에서 얼마나 위에 띄울지 (월드 단위)")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);

    private Camera cam;
    private Vector3 anchorWorldPos;
    private float hideTimer;

    private void Awake()
    {
        cam = Camera.main;
        if (cam == null)
            Debug.LogWarning($"{nameof(StaminaWarningUI)}: 'MainCamera' 태그가 붙은 카메라를 찾지 못했습니다.", this);

        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (player != null) player.OnStaminaExhausted += Show;
    }

    private void OnDisable()
    {
        if (player != null) player.OnStaminaExhausted -= Show;
    }

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

    private void UpdateScreenPosition()
    {
        if (cam == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(anchorWorldPos);
        screenPos.z = 0f;
        warningText.rectTransform.position = screenPos;
    }
}