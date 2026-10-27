using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 체력바와 스태미나바를 표시한다. 체력·스태미나가 바뀌었다는 방송을 듣고 바 길이를 갱신한다.
/// </summary>
public class PlayerStatusUI : MonoBehaviour
{
    [Header("Player Reference (비워두면 'Player' 태그로 자동 검색)")]
    [SerializeField] private PlayerHealth health;     // [수정] PlayerHealthStamina player → 체력·스태미나 따로
    [SerializeField] private PlayerStamina stamina;   // [추가]

    [Header("UI Fill Images")]
    [Tooltip("체력바의 Fill Image (Image Type: Filled)")]
    [SerializeField] private Image healthFillImage;
    [Tooltip("스태미너바의 Fill Image (Image Type: Filled)")]
    [SerializeField] private Image staminaFillImage;

    private void Start()
    {
        // 칸이 비어 있으면 Player 태그로 찾는다
        if (health == null || stamina == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                if (health == null) health = playerObj.GetComponent<PlayerHealth>();       // [수정]
                if (stamina == null) stamina = playerObj.GetComponent<PlayerStamina>();    // [추가]
            }
        }

        if (health != null)
        {
            health.OnHealthChanged += UpdateHealthUI;
            UpdateHealthUI(health.CurrentHealth, health.MaxHealth);
        }
        else
        {
            Debug.LogWarning("[PlayerStatusUI] PlayerHealth를 찾을 수 없습니다.");
        }

        if (stamina != null)   // [추가] 체력과 스태미나를 따로 확인
        {
            stamina.OnStaminaChanged += UpdateStaminaUI;
            UpdateStaminaUI(stamina.CurrentStamina, stamina.MaxStamina);
        }
        else
        {
            Debug.LogWarning("[PlayerStatusUI] PlayerStamina를 찾을 수 없습니다.");
        }
    }

    private void OnDestroy()
    {
        if (health != null) health.OnHealthChanged -= UpdateHealthUI;        // [수정]
        if (stamina != null) stamina.OnStaminaChanged -= UpdateStaminaUI;    // [수정]
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthFillImage != null && maxHealth > 0f)
            healthFillImage.fillAmount = currentHealth / maxHealth;
    }

    private void UpdateStaminaUI(float currentStamina, float maxStamina)
    {
        if (staminaFillImage != null && maxStamina > 0f)
            staminaFillImage.fillAmount = currentStamina / maxStamina;
    }
}