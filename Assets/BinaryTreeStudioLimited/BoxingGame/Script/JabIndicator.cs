using System.Collections;
using UnityEngine;
using Nex.Essentials;
using Microsoft.Unity.VisualStudio.Editor;

public class JabIndicator : MonoBehaviour
{
    [SerializeField] private JabDetector jabDetector = null!;
    public RectTransform RectTransform { get; set; } = null!;
    [SerializeField] GameObject jabImage;

    private Coroutine resetCoroutine;

    protected void Awake()
    {
        RectTransform = (RectTransform)transform;
    }
    protected void Start()
    {
        jabDetector.OnJabDetected += HandleJabDetected;
        jabImage.SetActive(false);
    }

    void HandleJabDetected(Vector2 position)
    {
        RectTransform.position = position;
        jabImage.SetActive(true);

        // Restart the reset color coroutine
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }
        resetCoroutine = StartCoroutine(ResetColorAfterDelay(jabDetector.jabCooldown));

    }

    private IEnumerator ResetColorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        resetCoroutine = null;
        jabImage.SetActive(false);
    }

    private void OnDestroy()
    {
        jabDetector.OnJabDetected -= HandleJabDetected;
    }
}
