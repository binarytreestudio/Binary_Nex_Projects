using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackPathIndicatorManager : MonoBehaviour
{
    [SerializeField] private GameObject attackPathIndicatorPrefab;

    private struct AttackIndicatorMapping
    {
        public GameObject indicatorObject;
        public EnemyController.PlayerAttackPath path;
        public int position;
    }
    List<AttackIndicatorMapping> attackPathIndicatorObjects = new();

    void Start()
    {
        BattleManager.Instance.OnPlayerAttackSuccess += HideAttackIndicator;

        EnemyController.Instance.OnEnemyCreateBubble += ShowAttackIndicator;
    }

    void OnDestroy()
    {
        BattleManager.Instance.OnPlayerAttackSuccess -= HideAttackIndicator;

        EnemyController.Instance.OnEnemyCreateBubble -= ShowAttackIndicator;
    }

    public void ShowAttackIndicator(EnemyController.PlayerAttackPath path, float duration)
    {
        GameObject indicatorObj = Instantiate(attackPathIndicatorPrefab, transform);
        AttackPathIndicatorController indicatorController = indicatorObj.GetComponent<AttackPathIndicatorController>();

        int position = 4; // center
        if (path != EnemyController.PlayerAttackPath.CrossFinisher)
        {
            position = UnityEngine.Random.Range(0, 9);
            while (attackPathIndicatorObjects.Exists(m => m.position == position) || position == 4)
            {
                position = UnityEngine.Random.Range(0, 9);
            }
        }
        indicatorObj.transform.localPosition = position switch
        {
            0 => new Vector3(-710f, 290f, 0f),
            1 => new Vector3(0f, 290f, 0f),
            2 => new Vector3(710f, 290f, 0f),
            3 => new Vector3(-710f, 0f, 0f),
            4 => new Vector3(0f, 0f, 0f),
            5 => new Vector3(710f, 0f, 0f),
            6 => new Vector3(-710f, -290f, 0f),
            7 => new Vector3(0f, -290f, 0f),
            8 => new Vector3(710f, -290f, 0f),
            _ => new Vector3(0f, 0f, 0f),
        };

        indicatorController.Show(path, duration);

        attackPathIndicatorObjects.Add(new AttackIndicatorMapping { indicatorObject = indicatorObj, path = path, position = position });

        indicatorController.OnIndicatorDestroyed += OnIndicatorDestroyed;
    }

    public void HideAttackIndicator(int playerIndex, BattleManager.HitType hitType, float damage, EnemyController.PlayerAttackPath path)
    {
        AttackIndicatorMapping mapping = attackPathIndicatorObjects.Find(m => m.path == path);
        if (mapping.indicatorObject != null)
        {
            mapping.indicatorObject.GetComponent<AttackPathIndicatorController>().Hide();
        }
    }

    void OnIndicatorDestroyed(GameObject indicatorObject)
    {
        AttackIndicatorMapping mapping = attackPathIndicatorObjects.Find(obj => obj.indicatorObject == indicatorObject);
        mapping.indicatorObject.GetComponent<AttackPathIndicatorController>().OnIndicatorDestroyed -= OnIndicatorDestroyed;
        attackPathIndicatorObjects.Remove(mapping);
    }
}
