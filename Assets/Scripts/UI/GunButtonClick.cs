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
        if (eventData.button == PointerEventData.InputButton.Right) shopManager.SellGun(Gun);
        if(eventData.button == PointerEventData.InputButton.Left) shopManager.BuyGun(Gun);
    }
}
