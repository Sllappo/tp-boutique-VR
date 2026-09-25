using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ScanItemController : MonoBehaviour
{
    [SerializeField, Min(0f)]
    [Tooltip("Distance maximale du scan. 0 conserve une portee illimitee.")]
    private float maxScanDistance = 0f;

    [SerializeField] private bool showDebugRay;
    [SerializeField] private ScanFeedback feedback = new ScanFeedback();

    private XRGrabInteractable grabInteractable;

    private void OnEnable()
    {
        grabInteractable = GetComponentInParent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogWarning("Le scanner doit etre place sur un objet avec XR Grab Interactable, ou sur un de ses enfants.", this);
            return;
        }

        // Certaines scenes relient deja Activated a Scan dans l'Inspector.
        var activated = grabInteractable.activated;
        for (int i = 0; i < activated.GetPersistentEventCount(); i++)
        {
            if (activated.GetPersistentTarget(i) == this
                && activated.GetPersistentMethodName(i) == nameof(Scan)
                && activated.GetPersistentListenerState(i) != UnityEventCallState.Off)
                return;
        }

        activated.AddListener(OnActivated);
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.activated.RemoveListener(OnActivated);

        feedback.Hide();
    }

    private void LateUpdate()
    {
        feedback.Tick();
    }

    private void OnDestroy()
    {
        feedback.Dispose();
    }

    private void OnActivated(ActivateEventArgs args)
    {
        if (grabInteractable.isSelected)
            Scan();
    }

    public void Scan()
    {
        if (!isActiveAndEnabled)
            return;

        Transform scanTransform = transform;
        Ray ray = new Ray(scanTransform.position, scanTransform.forward);
        float distance = maxScanDistance > 0f ? maxScanDistance : Mathf.Infinity;

        // Un article est identifie par son composant, meme si son collider est sur Default.
        // RaycastAll n'est appele qu'au tir ; le decor et le pistolet ne masquent pas les articles.
        RaycastHit[] hits = Physics.RaycastAll(ray, distance);
        DisplayItemController closestItem = null;
        float closestDistance = distance;

        foreach (RaycastHit hit in hits)
        {
            if (hit.distance > closestDistance)
                continue;

            // Le collider peut etre sur un enfant de l'objet qui porte les donnees de l'article.
            DisplayItemController item = hit.collider.GetComponentInParent<DisplayItemController>();
            if (item == null || !item.isActiveAndEnabled)
                continue;

            closestItem = item;
            closestDistance = hit.distance;
        }

        if (showDebugRay)
        {
            bool hasItem = closestItem != null;
            float debugDistance = hasItem ? closestDistance : (maxScanDistance > 0f ? maxScanDistance : 1000f);
            Debug.DrawRay(ray.origin, ray.direction * debugDistance, hasItem ? Color.yellow : Color.white, 0.5f);
        }

        bool added = closestItem != null && closestItem.TryAddToList();
        string message = closestItem == null
            ? "Aucun article"
            : added
                ? $"{closestItem.ItemName}\n+ {closestItem.Price:0.00} €"
                : "Ajout impossible";
        float beamDistance = closestItem != null
            ? closestDistance
            : (maxScanDistance > 0f ? maxScanDistance : 10f);
        feedback.Show(scanTransform, ray.origin, ray.GetPoint(beamDistance), message, added);
    }
}
