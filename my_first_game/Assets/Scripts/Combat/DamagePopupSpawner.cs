using UnityEngine;

/// <summary>
/// 플레이어의 공격이 맞을 때마다 맞은 위치에 데미지 숫자를 생성한다.
/// 몬스터 쪽 코드는 수정할 필요가 없다 (IDamageable만 구현하면 자동으로 숫자가 뜬다).
/// </summary>
public class DamagePopupSpawner : MonoBehaviour
{
    [SerializeField] private PlayerAttack attack;
    [Tooltip("DamagePopup이 붙은 프리팹")]
    [SerializeField] private DamagePopup popupPrefab;
    [Tooltip("맞은 위치에서 얼마나 위에 띄울지 (월드 단위)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.5f, 0f);
    [Tooltip("숫자가 겹치지 않도록 좌우로 무작위로 흩어지는 범위")]
    [SerializeField, Min(0f)] private float randomSpreadX = 0.3f;

    private void OnEnable()
    {
        if (attack != null) attack.HitLanded += SpawnPopup;
    }

    private void OnDisable()
    {
        if (attack != null) attack.HitLanded -= SpawnPopup;
    }

    private void SpawnPopup(IDamageable target, Vector3 hitPoint, float damage)
    {
        if (popupPrefab == null) return;

        Vector3 randomOffset = new Vector3(Random.Range(-randomSpreadX, randomSpreadX), 0f, 0f);
        Vector3 spawnPos = hitPoint + offset + randomOffset;

        DamagePopup popup = Instantiate(popupPrefab, spawnPos, Quaternion.identity);
        popup.Setup(damage);
    }
}