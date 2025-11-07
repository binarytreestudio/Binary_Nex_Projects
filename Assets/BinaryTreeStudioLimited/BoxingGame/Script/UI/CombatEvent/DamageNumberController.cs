using UnityEngine;
using DG.Tweening;

public class DamageNumberController : MonoBehaviour
{
    [SerializeField] Color criticalColor = Color.yellow;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color finisherColor = Color.red;
    [SerializeField] TMPro.TextMeshProUGUI damageText;
    [SerializeField] float randomTransformRange = 5f;

    public void Init(BattleManager.HitType hitType, float damage)
    {
        damageText.text = Mathf.RoundToInt(damage).ToString();
        switch (hitType)
        {
            case BattleManager.HitType.Good:
                damageText.color = normalColor;
                break;
            case BattleManager.HitType.Perfect:
                damageText.color = criticalColor;
                break;
            case BattleManager.HitType.Finisher:
                damageText.color = finisherColor;
                break;
        }
        damageText.fontSize = Mathf.Clamp(20 + damage * 0.1f, 20, 100);

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.OutBack);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOLocalJump(new Vector3(Random.Range(-randomTransformRange, randomTransformRange), -randomTransformRange * 2, 0), randomTransformRange * 2, 1, 0.8f).SetEase(Ease.OutQuad));
        seq.Join(damageText.DOFade(0, .8f).SetEase(Ease.Linear));
        seq.OnComplete(() => Destroy(gameObject));
    }
}