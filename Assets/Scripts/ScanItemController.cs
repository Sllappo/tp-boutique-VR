using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScanItemController : MonoBehaviour
{
    LayerMask layerMask;

    void Awake()
    {
        layerMask = LayerMask.GetMask("ItemScan");
    }


    public void Scan()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))

        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            Debug.Log(hit.transform.name);
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            Debug.Log("Did not Hit");
        }
    }
}
