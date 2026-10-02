using TMPro;
using UnityEngine;

/// <summary>
/// 데미지 숫자 하나. 생성되면 위로 떠오르며 서서히 투명해지다가 스스로 사라진다.
/// 월드 공간 텍스트(TextMeshPro)가 붙은 프리팹에 이 스크립트를 붙여서 사용한다.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class DamagePopup : MonoBehaviour
{
    [Tooltip("떠 있는 시간(초)")]
    [SerializeField, Min(0.1f)] private float lifetime = 0.8f;
    [Tooltip("처음 떠오르는 속도 (점점 느려진다)")]
    [SerializeField] private float riseSpeed = 1.5f;
    [Tooltip("그리는 순서. 캐릭터·몬스터보다 크게 해야 숫자가 가려지지 않는다.")]
    [SerializeField] private int sortingOrder = 100;

    private TextMeshPro text;
    private Color startColor;
    private float timer;

    private void Awake()
    {
        text = GetComponent<TextMeshPro>();
        startColor = text.color;
        text.sortingOrder = sortingOrder;
    }

    /// <summary>생성 직후 호출해서 표시할 숫자를 정한다.</summary>
    public void Setup(float amount)
    {
        text.text = Mathf.RoundToInt(amount).ToString();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / lifetime);   // 0 → 1 로 진행

        // 위로 떠오르기 (처음엔 빠르고 점점 느려짐)
        transform.position += Vector3.up * (riseSpeed * (1f - t) * Time.deltaTime);

        // 서서히 투명해지기
        Color c = startColor;
        c.a = startColor.a * (1f - t);
        text.color = c;

        if (timer >= lifetime) Destroy(gameObject);
    }
}