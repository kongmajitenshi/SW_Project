using UnityEngine;

/// <summary>
/// 무기 한 종류의 수치 데이터. 무기마다 에셋을 하나씩 만든다.
/// </summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Combat/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "새 무기";

    [Tooltip("이 무기를 장착했을 때 플레이어 기본 공격력에 더해지는 값")]
    public float attackPower = 5f;
}