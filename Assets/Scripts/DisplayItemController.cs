using TMPro;
using UnityEngine;

public class DisplayItemController : MonoBehaviour
{
    [SerializeField] private string name;
    [SerializeField] private float price;
    [SerializeField] private TextMeshProUGUI tmp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void getDisplayItem()
    {
        if (tmp.text != "")
        {
            tmp.text += "\nName : " + name + " Price : " + price + "€";
        }
        else
        {
            tmp.text = "Name : " + name + " Price : " + price + "€";
        }

    }
}
