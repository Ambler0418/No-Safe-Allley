using UnityEngine;
using System.Collections.Generic;

namespace Map
{
    [System.Serializable]
    public class DialogueLine
    {
        [Tooltip("화자 이름 (비어있으면 내레이션)")]
        public string speakerName;
        
        [TextArea(3, 5)]
        public string text; // 대사 내용
        
        [Tooltip("표시할 캐릭터 스프라이트 (없으면 기존 유지/숨김)")]
        public Sprite characterSprite;

        [Tooltip("캐릭터가 화면 왼쪽에 위치하는지 여부 (false면 오른쪽)")]
        public bool isLeft = true;
        
        [Tooltip("배경 이미지 변경이 필요할 때 설정")]
        public Sprite backgroundImage;
    }

    [CreateAssetMenu(fileName = "NewDialogueEvent", menuName = "Map/Dialogue Event Data")]
    public class DialogueEventData : ScriptableObject
    {
        public string eventTitle; // 이벤트 식별용 이름
        
        [Header("Dialogue Sequence")]
        public List<DialogueLine> lines = new List<DialogueLine>();

        [Header("Rewards (Optional)")]
        public int goldReward;
        // 추후 아이템이나 카드 보상 추가 가능
    }
}
