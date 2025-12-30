using UnityEngine;
using UnityEngine.EventSystems;

public class HandInteractionZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스가 영역에 들어오면 HandManager에 알림
        HandManager.Instance.ExpandHand();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 영역을 벗어나면 HandManager에 알림
        HandManager.Instance.RetractHand();
    }
}
