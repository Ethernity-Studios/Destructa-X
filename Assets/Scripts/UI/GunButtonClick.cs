using UnityEngine;
using UnityEngine.EventSystems;

public class GunButtonClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]   ShopManager shopManager;

    public Gun Gun;

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
