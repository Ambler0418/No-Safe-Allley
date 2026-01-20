using UnityEngine;
using TMPro;
using System.Collections;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _lifeTime = 1f;
    [SerializeField] private float _moveSpeed = 0.3f;
    [SerializeField] private Vector3 _moveDirection = new Vector3(0, 1, 0);

    private float _timer;

    private void Awake()
    {
        _timer = _lifeTime;
    }

    public void SetText(int damageAmount)
    {
        _text.text = "-" + damageAmount.ToString();
    }

    private void Update()
    {
        // 위로 이동
        transform.position += _moveDirection * _moveSpeed * Time.deltaTime;

        // 시간에 따라 알파 값 감소 (페이드 아웃)
        _timer -= Time.deltaTime;
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, _timer / _lifeTime);

        // 생명 시간이 다 되면 파괴
        if (_timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}
