using UnityEngine;

public class DisplayItemController : MonoBehaviour
{
    [SerializeField] private string itemName;
    [SerializeField] private float price;

    public string ItemName => itemName;
    public float Price => price;

    public void getDisplayItem()
    {
        TryAddToList();
    }

    public bool TryAddToList()
    {
        ItemListManager manager = ItemListManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("Impossible d'ajouter l'article : aucun ItemListManager n'est disponible.", this);
            return false;
        }

        return manager.TryAddItem(itemName, price);
    }
}
