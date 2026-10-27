using UnityEngine;

/// <summary>
/// 패리에 성공하면 플레이어 머리 위에 "패리" / "퍼펙트 패리!" 글씨를 띄운다.
/// </summary>
[RequireComponent(typeof(PlayerParry))]   // [추가]
public class ParryPopupSpawner : MonoBehaviour
{
    // [삭제] parry 칸 → Awake에서 자동으로 찾음
    [Tooltip("DamagePopup이 붙은 프리팹 (한글 폰트가 적용된 것)")]
    [SerializeField] private DamagePopup popupPrefab;
    [Tooltip("플레이어 위치에서 얼마나 위에 띄울지 (월드 단위)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, 0f);

    [Header("일반 패리")]
    [SerializeField] private string normalText = "패리";
    [SerializeField] private Color normalColor = Color.white;

    [Header("퍼펙트 패리")]
    [SerializeField] private string perfectText = "퍼펙트 패리!";
    [SerializeField] private Color perfectColor = new Color(1f, 0.6f, 0.1f);
    [Tooltip("퍼펙트 글씨를 일반보다 몇 배 크게 띄울지")]
    [SerializeField, Min(1f)] private float perfectScale = 1.3f;

    private PlayerParry parry;   // [수정] 자동 연결

    private void Awake()
    {
        parry = GetComponent<PlayerParry>();   // [추가]
    }

    private void OnEnable()
    {
        if (parry != null) parry.ParrySucceeded += SpawnPopup;
    }

    private void OnDisable()
    {
        if (parry != null) parry.ParrySucceeded -= SpawnPopup;
    }

    private void SpawnPopup(ParryResult result, EnemyAttackData blockedAttack)
    {
        if (popupPrefab == null) return;

        bool isPerfect = result == ParryResult.Perfect;
        Vector3 spawnPos = transform.position + offset;   // [수정] parry.transform → transform (같은 오브젝트)

        DamagePopup popup = Instantiate(popupPrefab, spawnPos, Quaternion.identity);
        popup.Setup(isPerfect ? perfectText : normalText,
            isPerfect ? perfectColor : normalColor);

        if (isPerfect) popup.transform.localScale *= perfectScale;
    }
}