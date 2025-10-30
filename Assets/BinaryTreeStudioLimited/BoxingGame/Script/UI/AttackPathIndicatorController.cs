using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Jazz;

public class AttackPathIndicatorController : MonoBehaviour
{
    [Serializable]
    private struct AttackPathIndicator
    {
        public RectTransform arrow;
        public RectTransform fill;
    }
    [SerializeField] private List<AttackPathIndicator> attackPathIndicators = new();

    private float timer;

    public void Show(float duration)
    {
        gameObject.SetActive(true);
        attackPathIndicators.ForEach(indicator => indicator.arrow.gameObject.SetActive(true));
        timer = duration;

        foreach (var indicator in attackPathIndicators)
        {
            Image image = indicator.arrow.GetComponent<Image>();
            indicator.fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, indicator.arrow.rect.width);
            DOTween.To(() => timer, x => timer = x, 0f, duration).SetEase(Ease.Linear).OnUpdate(() =>
            {
                float fillWidth = (timer / duration) * indicator.arrow.rect.width;
                indicator.fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillWidth);
                image.fillAmount = timer / duration;
            })
            .OnComplete(() =>
            {
                Hide();
            });
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void Hide(Handedness handedness)
    {
        // Currently, only CrossFinisher uses handedness
        switch (handedness)
        {
            case Handedness.Left:
                attackPathIndicators[0].arrow.gameObject.SetActive(false);
                break;
            case Handedness.Right:
                attackPathIndicators[1].arrow.gameObject.SetActive(false);
                break;
        }
    }
}
