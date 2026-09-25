using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[Serializable]
public sealed class ScanFeedback : IDisposable
{
    [SerializeField, Min(0f)] private float beamDuration = 0.12f;
    [SerializeField, Min(0f)] private float popupDuration = 1.1f;
    [SerializeField, Min(0.001f)] private float beamWidth = 0.006f;
    [SerializeField, Range(0f, 1f)] private float volume = 0.35f;
    [SerializeField] private AudioClip shotClip;

    private GameObject root;
    private Transform source;
    private LineRenderer beam;
    private Canvas popupCanvas;
    private TextMeshProUGUI popupText;
    private AudioSource audioSource;
    private AudioClip defaultShotClip;
    private Camera viewerCamera;
    private float beamHideTime;
    private float popupHideTime;

    public void Show(Transform source, Vector3 origin, Vector3 end, string message, bool success)
    {
        EnsureCreated();
        this.source = source;
        root.transform.position = source != null ? source.position : origin;
        root.SetActive(true);

        Color color = success ? new Color(0.25f, 1f, 0.5f) : new Color(1f, 0.8f, 0.2f);
        beam.startColor = color;
        beam.endColor = color;
        beam.widthMultiplier = Mathf.Max(0.001f, beamWidth);
        beam.SetPosition(0, origin);
        beam.SetPosition(1, end);
        beam.enabled = beamDuration > 0f;

        popupText.text = message;
        popupText.color = color;
        popupCanvas.gameObject.SetActive(popupDuration > 0f);

        float now = Time.unscaledTime;
        beamHideTime = now + Mathf.Max(0f, beamDuration);
        popupHideTime = now + Mathf.Max(0f, popupDuration);

        AudioClip clip = shotClip != null ? shotClip : defaultShotClip;
        if (clip != null)
            audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));

        Tick();
    }

    public void Tick()
    {
        if (root == null || !root.activeSelf)
            return;

        if (source != null)
            root.transform.position = source.position;

        float now = Time.unscaledTime;
        if (now >= beamHideTime)
            beam.enabled = false;

        if (now >= popupHideTime)
            popupCanvas.gameObject.SetActive(false);

        if (popupCanvas.gameObject.activeSelf)
        {
            if (viewerCamera == null || !viewerCamera.isActiveAndEnabled)
                viewerCamera = Camera.main;

            if (viewerCamera != null)
            {
                popupCanvas.worldCamera = viewerCamera;
                Transform popup = popupCanvas.transform;
                Vector3 direction = popup.position - viewerCamera.transform.position;
                if (direction.sqrMagnitude > 0.0001f)
                    popup.rotation = Quaternion.LookRotation(direction, viewerCamera.transform.up);
            }
        }

        if (!beam.enabled && !popupCanvas.gameObject.activeSelf && !audioSource.isPlaying)
            root.SetActive(false);
    }

    public void Hide()
    {
        if (root == null)
            return;

        if (audioSource != null)
            audioSource.Stop();
        if (beam != null)
            beam.enabled = false;
        if (popupCanvas != null)
            popupCanvas.gameObject.SetActive(false);
        root.SetActive(false);
    }

    public void Dispose()
    {
        if (root == null)
            return;

        Hide();
        if (Application.isPlaying)
            UnityEngine.Object.Destroy(root);
        else
            UnityEngine.Object.DestroyImmediate(root);

        root = null;
        source = null;
        beam = null;
        popupCanvas = null;
        popupText = null;
        audioSource = null;
        viewerCamera = null;
        defaultShotClip = null;
    }

    private void EnsureCreated()
    {
        if (root != null)
            return;

        // Racine independante : la taille du retour ne depend pas de l'echelle du pistolet.
        root = new GameObject("Scan Feedback (Runtime)");
        root.SetActive(false);

        GameObject beamObject = new GameObject("Scan Beam", typeof(LineRenderer));
        beamObject.transform.SetParent(root.transform, false);
        beam = beamObject.GetComponent<LineRenderer>();
        beam.useWorldSpace = true;
        beam.positionCount = 2;
        beam.alignment = LineAlignment.View;
        beam.numCapVertices = 4;
        beam.shadowCastingMode = ShadowCastingMode.Off;
        beam.receiveShadows = false;
        beam.sharedMaterial = Resources.Load<Material>("ScanFeedback/Beam");
        beam.enabled = false;

        GameObject popupObject = new GameObject("Scan Popup", typeof(RectTransform), typeof(Canvas), typeof(Image));
        popupObject.transform.SetParent(root.transform, false);
        popupCanvas = popupObject.GetComponent<Canvas>();
        popupCanvas.renderMode = RenderMode.WorldSpace;
        popupCanvas.sortingOrder = 100;
        RectTransform popupRect = popupObject.GetComponent<RectTransform>();
        popupRect.sizeDelta = new Vector2(300f, 84f);
        popupRect.localScale = Vector3.one * 0.001f;
        popupRect.localPosition = Vector3.up * 0.25f;
        Image background = popupObject.GetComponent<Image>();
        background.color = new Color(0.025f, 0.035f, 0.055f, 0.94f);
        background.raycastTarget = false;

        GameObject textObject = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(popupRect, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 8f);
        textRect.offsetMax = new Vector2(-12f, -8f);
        popupText = textObject.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.instance != null)
            popupText.font = TMP_Settings.defaultFontAsset;
        popupText.fontSize = 24f;
        popupText.enableAutoSizing = true;
        popupText.fontSizeMin = 16f;
        popupText.fontSizeMax = 24f;
        popupText.alignment = TextAlignmentOptions.Center;
        popupText.textWrappingMode = TextWrappingModes.Normal;
        popupText.overflowMode = TextOverflowModes.Ellipsis;
        popupText.richText = false;
        popupText.raycastTarget = false;
        popupObject.SetActive(false);

        audioSource = root.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 0f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 0.5f;
        audioSource.maxDistance = 8f;
        defaultShotClip = Resources.Load<AudioClip>("ScanFeedback/Shot");
    }
}
