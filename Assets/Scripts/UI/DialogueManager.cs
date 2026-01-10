using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Map; // DialogueEventData 참조

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject dialoguePanel;     // 전체 대화 UI 패널
    public Image backgroundImage;        // 배경 이미지
    public Image leftCharacterImage;     // 왼쪽 캐릭터 일러스트
    public Image rightCharacterImage;    // 오른쪽 캐릭터 일러스트
    public TextMeshProUGUI nameText;     // 화자 이름 텍스트
    public TextMeshProUGUI bodyText;     // 대사 내용 텍스트
    public GameObject nextIndicator;     // 다음 대사가 있음을 알리는 아이콘 (화살표 등)

    [Header("Settings")]
    public float typingSpeed = 0.05f;    // 글자 나오는 속도

    private Queue<DialogueLine> _linesQueue = new Queue<DialogueLine>();
    private bool _isTyping = false;
    private string _currentFullText = "";
    private System.Action _onDialogueFinished; // 대화 종료 후 실행할 콜백

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작 시 패널 숨김
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        // 대화 중이고 패널이 켜져있을 때 클릭/스페이스바로 진행
        if (dialoguePanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                OnSubmit();
            }
        }
    }

    /// <summary>
    /// 대화 이벤트를 시작합니다.
    /// </summary>
    /// <param name="data">대화 데이터</param>
    /// <param name="onFinished">종료 시 실행할 콜백 (예: 맵으로 복귀)</param>
    public void StartDialogue(DialogueEventData data, System.Action onFinished = null)
    {
        if (data == null) return;

        _onDialogueFinished = onFinished;
        _linesQueue.Clear();

        foreach (var line in data.lines)
        {
            _linesQueue.Enqueue(line);
        }

        dialoguePanel.SetActive(true);
        DisplayNextLine();
    }

    public void OnSubmit()
    {
        if (_isTyping)
        {
            // 타이핑 중이라면 즉시 전체 텍스트 표시
            StopAllCoroutines();
            bodyText.text = _currentFullText;
            _isTyping = false;
            if (nextIndicator) nextIndicator.SetActive(true);
        }
        else
        {
            // 타이핑이 끝났다면 다음 줄로
            DisplayNextLine();
        }
    }

    private void DisplayNextLine()
    {
        if (_linesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = _linesQueue.Dequeue();
        _currentFullText = line.text;

        // UI 업데이트
        if (nameText != null) nameText.text = line.speakerName;
        
        // 캐릭터 이미지 처리
        UpdateCharacterImages(line);

        // 배경 이미지 처리 (데이터에 있을 경우만)
        if (line.backgroundImage != null && backgroundImage != null)
        {
            backgroundImage.sprite = line.backgroundImage;
        }

        // 타이핑 코루틴 시작
        StopAllCoroutines();
        StartCoroutine(TypeSentence(line.text));
    }

    private void UpdateCharacterImages(DialogueLine line)
    {
        // 1. 스프라이트가 없는 경우: 기존 이미지를 유지하거나 숨길지 정책 결정
        // 여기서는 스프라이트가 null이면 '유지'하되, 화자에 따라 어둡게 처리하는 등의 연출이 가능함.
        // 일단 간단하게: 스프라이트가 할당된 경우에만 갱신 및 표시
        
        if (line.characterSprite != null)
        {
            if (line.isLeft)
            {
                if (leftCharacterImage != null)
                {
                    leftCharacterImage.sprite = line.characterSprite;
                    leftCharacterImage.gameObject.SetActive(true);
                    leftCharacterImage.color = Color.white; // 밝게
                }
                // 반대편은 조금 어둡게 처리 (선택 사항)
                if (rightCharacterImage != null && rightCharacterImage.gameObject.activeSelf)
                    rightCharacterImage.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                if (rightCharacterImage != null)
                {
                    rightCharacterImage.sprite = line.characterSprite;
                    rightCharacterImage.gameObject.SetActive(true);
                    rightCharacterImage.color = Color.white;
                }
                if (leftCharacterImage != null && leftCharacterImage.gameObject.activeSelf)
                    leftCharacterImage.color = new Color(0.5f, 0.5f, 0.5f);
            }
        }
        else
        {
            // 스프라이트가 없는 대사(내레이션 등)일 때 처리
            // 둘 다 어둡게 하거나 숨기는 로직을 추가할 수 있음
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        _isTyping = true;
        bodyText.text = "";
        if (nextIndicator) nextIndicator.SetActive(false);

        foreach (char letter in sentence.ToCharArray())
        {
            bodyText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;
        if (nextIndicator) nextIndicator.SetActive(true);
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        _onDialogueFinished?.Invoke();
    }
}
