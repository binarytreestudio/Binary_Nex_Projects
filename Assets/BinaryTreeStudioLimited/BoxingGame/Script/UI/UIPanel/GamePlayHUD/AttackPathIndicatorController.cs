using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Jazz;

public class AttackPathIndicatorController : MonoBehaviour
{
    [Serializable]
    private struct PathAngleMapping
    {
        public EnemyController.PlayerAttackPath path;
        public float angle;
    }
    [SerializeField] private List<PathAngleMapping> pathAngleMappings = new();
    [SerializeField] private Image arrowImage;
    [SerializeField] private GameObject crossFinisherAdditionalArrow;

    private float timer;

    public void Show(EnemyController.PlayerAttackPath path, float duration)
    {
        switch (path)
        {
            case EnemyController.PlayerAttackPath.LeftHook:
                arrowImage.transform.localEulerAngles = new Vector3(0f, 0f, GetAngleForPath(path));
                break;
            case EnemyController.PlayerAttackPath.RightHook:
                arrowImage.transform.localEulerAngles = new Vector3(0f, 0f, GetAngleForPath(path));
                break;
            case EnemyController.PlayerAttackPath.Uppercut:
                arrowImage.transform.localEulerAngles = new Vector3(0f, 0f, GetAngleForPath(path));
                break;
            case EnemyController.PlayerAttackPath.CrossFinisher:
                arrowImage.transform.localEulerAngles = new Vector3(0f, 0f, GetAngleForPath(path));
                crossFinisherAdditionalArrow.SetActive(true);
                break;
            default:
                break;
        }

        if (duration <= 0f) return;

        timer = duration;

        DOTween.To(() => timer, x => timer = x, 0f, duration).SetEase(Ease.Linear)
        .OnUpdate(() =>
        {
            arrowImage.fillAmount = timer / duration;
        })
        .OnComplete(() =>
        {
            Hide();
        });

        // Local helper function
        float GetAngleForPath(EnemyController.PlayerAttackPath path)
        {
            return pathAngleMappings.Find(mapping => mapping.path == path).angle;
        }
    }

    public void Hide()
    {
        Destroy(gameObject);
    }
}
