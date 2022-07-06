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
        if (eventData.button == PointerEventData.InputButton.Left) shopManager.BuyShield(shield);
        if (eventData.button == PointerEventData.InputButton.Right) shopManager.SellShield(shield);
    }
}
