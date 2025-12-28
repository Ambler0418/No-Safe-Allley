using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Image healthBarFill;
    public void updateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthBarFill != null)
     {
         healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
        // --- 아래 else문 추가 ---
        else
        {
            Debug.LogError("HealthBarFill 이미지가 할당되지 않았습니다! " + this.gameObject.name + "의 Inspector 창에서 연결해주세요.");
        }
    }
}
