using TMPro;
using UnityEngine;

/// <summary>
/// 떠오르며 사라지는 글자 하나. 생성되면 위로 떠오르며 서서히 투명해지다가 스스로 사라진다.
/// 숫자(데미지)와 문장(패리 알림 등) 둘 다 표시할 수 있다.
/// 월드 공간 텍스트(TextMeshPro)가 붙은 프리팹에 이 스크립트를 붙여서 사용한다.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class DamagePopup : MonoBehaviour
{
    [Tooltip("떠 있는 시간(초)")]
    [SerializeField, Min(0.1f)] private float lifetime = 0.8f;
    [Tooltip("처음 떠오르는 속도 (점점 느려진다)")]
    [SerializeField] private float riseSpeed = 1.5f;
    [Tooltip("그리는 순서. 캐릭터·몬스터보다 크게 해야 글자가 가려지지 않는다.")]
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

    /// <summary>숫자를 표시한다 (데미지용). 소수는 반올림해서 보여준다.</summary>
    public void Setup(float amount)
    {
        text.text = Mathf.RoundToInt(amount).ToString();
    }

    /// <summary>[추가] 원하는 글자와 색으로 표시한다 (패리 알림 등).</summary>
    public void Setup(string message, Color color)
    {
        text.text = message;
        startColor = color;   // [추가] 사라지는 연출이 이 색을 기준으로 투명해지도록
        text.color = color;
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