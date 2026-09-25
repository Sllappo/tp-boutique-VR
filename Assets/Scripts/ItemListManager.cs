using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemListManager : MonoBehaviour
{
    public static ItemListManager Instance { get; private set; } // Allows all scripts to access the instance of this class

    [SerializeField] private Transform content;    
    [SerializeField] private TextMeshProUGUI itemRow;
    [SerializeField] private TextMeshProUGUI subtotalText;

    private float subtotal = 0f;
    private List<GameObject> ItemRows = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddItem(string itemName, float price)
    {
        TryAddItem(itemName, price);
    }

    public bool TryAddItem(string itemName, float price)
    {
        if (content == null || itemRow == null || subtotalText == null)
        {
            Debug.LogError("ItemListManager : renseignez Content, Item Row et Subtotal Text dans l'Inspector.", this);
            return false;
        }

        TextMeshProUGUI row = Instantiate(itemRow, content);
        row.text = itemName + " - " + price.ToString("0.00") + "€";
        ItemRows.Add(row.gameObject);

        subtotal += price;
        UpdateSubtotal();
        return true;
    }

    public void ResetList()
    {
        foreach (var row in ItemRows)
            Destroy(row);
        ItemRows.Clear();

        subtotal = 0f;
        UpdateSubtotal();
    }

    private void UpdateSubtotal()
    {
        subtotalText.text = "Sous-total : " + subtotal.ToString("0.00") + "€";
    }
}
