using UnityEngine;
using UnityEngine.EventSystems;

public class ShieldButtonClick : MonoBehaviour, IPointerClickHandler
{
    ShopManager shopManager;

    private void Start()
    {
        shopManager = FindObjectOfType<ShopManager>();
    }

    [SerializeField] string shield;

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                shopManager.BuyShield(shield);
                break;
            case PointerEventData.InputButton.Right:
                shopManager.SellShield(shield);
                break;
        }
    }
}
