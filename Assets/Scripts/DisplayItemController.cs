using UnityEngine;

public class DisplayItemController : MonoBehaviour
{
    [SerializeField] private string itemName;
    [SerializeField] private float price;

    public void getDisplayItem()
    {
        ItemListManager.Instance.AddItem(itemName, price);
    }
}