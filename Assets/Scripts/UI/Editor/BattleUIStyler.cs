using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class BattleUIStyler : Editor
{
    [MenuItem("Tools/Style Battle UI (Dark Theme)")]
    public static void ApplyDarkTheme()
    {
        // 1. Canvas 찾기
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("씬에서 Canvas를 찾을 수 없습니다.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Style Battle UI");

        // 2. 모든 Image (패널, 배경 등) 스타일링
        Image[] images = canvas.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            // 버튼의 Target Graphic인 경우 건너뜀 (버튼 로직에서 처리)
            if (img.GetComponent<Button>() != null) continue;
            
            // 카드 UI 등 특정 요소는 제외 (이름으로 필터링하거나 태그 사용 가능)
            // 여기서는 "Panel"이라는 이름이 포함된 경우만 배경으로 간주
            if (img.name.Contains("Panel"))
            {
                img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f); // 짙은 검정 반투명
            }
        }

        // 3. 모든 Button 스타일링
        Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);      // 기본: 짙은 회색
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // 호버: 약간 밝게
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);     // 클릭: 어둡게
            colors.selectedColor = new Color(0.2f, 0.2f, 0.2f, 1f);    // 선택: 기본과 동일
            colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);  // 비활성: 반투명
            btn.colors = colors;

            // 버튼 이미지도 색상 영향 받도록 흰색으로 초기화 (ColorTint 모드일 때)
            if (btn.targetGraphic != null)
            {
                btn.targetGraphic.color = Color.white; 
            }

            // 버튼 자식 텍스트 스타일링
            var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.color = Color.white;
                btnText.fontSize = 24; // 버튼 텍스트 크기 통일 (선택 사항)
                btnText.fontStyle = FontStyles.Bold;
            }
        }

        // 4. 일반 TextMeshPro 텍스트 (버튼 내부 제외)
        TextMeshProUGUI[] texts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in texts)
        {
            // 버튼의 자식이면 이미 처리했으므로 패스
            if (txt.GetComponentInParent<Button>() != null) continue;

            // 기본 텍스트 색상: 흰색 (Rich Text가 적용된 부분은 태그가 우선순위를 가짐)
            txt.color = new Color(0.9f, 0.9f, 0.9f, 1f); 
        }

        Debug.Log("Battle UI 스타일링 완료 (Dark Theme 적용됨)");
    }
}
