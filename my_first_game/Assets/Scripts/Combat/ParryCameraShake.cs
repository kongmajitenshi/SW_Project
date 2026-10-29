using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 패리에 성공하면 카메라를 흔든다. 퍼펙트 패리는 더 세게 흔든다.
/// 실제로 흔드는 일은 Cinemachine Impulse가 하고, 이 스크립트는 "언제, 얼마나 세게"만 정한다.
/// </summary>
[RequireComponent(typeof(PlayerParry), typeof(CinemachineImpulseSource))]
public class ParryCameraShake : MonoBehaviour
{
    [Tooltip("일반 패리 때 흔들림 세기 (Impulse Source의 Default Velocity에 곱해짐)")]
    [SerializeField, Min(0f)] private float normalForce = 0.3f;
    [Tooltip("퍼펙트 패리 때 흔들림 세기")]
    [SerializeField, Min(0f)] private float perfectForce = 0.8f;

    private PlayerParry parry;
    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        parry = GetComponent<PlayerParry>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        if (parry == null || impulseSource == null)
        {
            Debug.LogError($"{nameof(ParryCameraShake)}: 같은 오브젝트에 필요한 컴포넌트가 없습니다. " +
                           "(PlayerParry, CinemachineImpulseSource)", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (parry != null) parry.ParrySucceeded += Shake;
    }

    private void OnDisable()
    {
        if (parry != null) parry.ParrySucceeded -= Shake;
    }

    private void Shake(ParryResult result, EnemyAttackData blockedAttack)
    {
        float force = result == ParryResult.Perfect ? perfectForce : normalForce;
        impulseSource.GenerateImpulseWithForce(force);
    }
}
