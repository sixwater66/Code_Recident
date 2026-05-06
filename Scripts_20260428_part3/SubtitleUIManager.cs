using System.Collections;
using UnityEngine;
using TMPro;

public class SubtitleUIManager : MonoBehaviour
{
    public GameObject subtitlePanel;
    public TextMeshProUGUI subtitleText;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);
    }

    public void ShowSubtitle(string text, float duration)
    {
        if (subtitlePanel == null || subtitleText == null) return;

        subtitleText.text = text;
        subtitlePanel.SetActive(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay(duration));
    }

    public void HideSubtitle()
    {
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideSubtitle();
    }
}