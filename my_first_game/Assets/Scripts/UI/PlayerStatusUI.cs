using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerHealthStamina player;

    [Header("UI Fill Images")]
    [Tooltip("체력바의 Fill Image를 넣어주세요")]
    [SerializeField] private Image healthFillImage;

    [Tooltip("스태미너바의 Fill Image를 넣어주세요")]
    [SerializeField] private Image staminaFillImage;

    private void Start()
    {
        // 씬 시작 시 플레이어 연결 확인
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.GetComponent<PlayerHealthStamina>();
        }

        // 초기 UI 수치 반영
        if (player != null)
        {
            // 이벤트 등록
            player.OnHealthChanged += UpdateHealthUI;
            player.OnStaminaChanged += UpdateStaminaUI;

            // 시작 직후 즉시 100% 갱신
            UpdateHealthUI(player.CurrentHealth, player.CurrentHealth);
            UpdateStaminaUI(player.CurrentStamina, player.CurrentStamina);
        }
        else
        {
            Debug.LogWarning("[PlayerStatusUI] PlayerHealthStamina를 찾을 수 없습니다.");
        }
    }

    private void OnDestroy()
    {
        // 씬 전환/오브젝트 파괴 시 이벤트 구독 해제 (메모리 누수 방지)
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealthUI;
            player.OnStaminaChanged -= UpdateStaminaUI;
        }
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthFillImage != null && maxHealth > 0f)
        {
            healthFillImage.fillAmount = currentHealth / maxHealth;
        }
    }

    private void UpdateStaminaUI(float currentStamina, float maxStamina)
    {
        if (staminaFillImage != null && maxStamina > 0f)
        {
            staminaFillImage.fillAmount = currentStamina / maxStamina;
        }
    }
}