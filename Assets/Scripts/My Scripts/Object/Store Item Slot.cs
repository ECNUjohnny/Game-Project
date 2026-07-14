using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Windows.Input;

public class StoreItemSlot : MonoBehaviour
{
    public Image iconImage;

    public TextMeshProUGUI nameText;

    public TextMeshProUGUI priceText;

    public Button button;

    private ItemData currentItem;

    public void Setup(ItemData item)
    {
        currentItem = item;
        iconImage.sprite = item.Icon;
        nameText.text = item.name;
        priceText.text = $"${item.price}";
    
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnBuyClicked);
    }

    public void OnBuyClicked()
    {
        Debug.Log($"{nameText.text} was bought");
    }
}
