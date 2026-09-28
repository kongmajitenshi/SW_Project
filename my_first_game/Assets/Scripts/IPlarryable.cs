using UnityEngine;

/// <summary>
/// 패링 판정에 걸릴 수 있는 대상이 구현한다. (IDamageable의 패링 버전)
/// PlayerParry가 판정 범위 안에서 이 인터페이스를 가진 오브젝트를 찾아 OnParried를 호출한다.
/// </summary>
public interface IParryable
{
    /// <param name="hitPoint">패링 판정이 닿은 위치</param>
    void OnParried(Vector3 hitPoint);
}