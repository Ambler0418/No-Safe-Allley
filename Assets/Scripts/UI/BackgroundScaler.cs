using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogError("BackgroundScaler: SpriteRenderer 또는 Sprite가 없습니다.");
            return;
        }

        // 카메라와 화면 크기 가져오기
        Camera camera = Camera.main;
        float screenHeight = camera.orthographicSize * 2;
        float screenWidth = screenHeight * camera.aspect;

        // 스프라이트 원본 크기 가져오기
        float spriteHeight = sr.sprite.bounds.size.y;
        float spriteWidth = sr.sprite.bounds.size.x;

        // 화면 너비와 높이에 맞춰야 할 스케일 비율을 각각 계산
        float widthScale = screenWidth / spriteWidth;
        float heightScale = screenHeight / spriteHeight;

        // 두 비율 중 더 '큰' 비율을 선택하여 화면을 빈틈없이 덮도록 함
        float scale = Mathf.Max(widthScale, heightScale);

        // 계산된 스케일을 X와 Y에 동일하게 적용
        transform.localScale = new Vector3(scale, scale, 1f);
        
        // Z 포지션을 카메라보다 멀리 둠
        transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, 10);
    }
}
