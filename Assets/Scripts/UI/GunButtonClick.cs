using UnityEngine;
using UnityEngine.EventSystems;

public class GunButtonClick : MonoBehaviour, IPointerClickHandler
{
    ShopManager shopManager;

    public Gun Gun;
    private void Start()
    {
        shopManager = FindObjectOfType<ShopManager>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                shopManager.BuyGun(Gun);
                break;
            case PointerEventData.InputButton.Right:
                shopManager.SellGun(Gun);
                break;
        }
    }
}
