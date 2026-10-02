using UnityEngine;

/// <summary>공격을 받을 수 있는 모든 대상(적, 샌드백, 부서지는 오브젝트 등)이 구현하는 인터페이스.</summary>
public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint);
}
