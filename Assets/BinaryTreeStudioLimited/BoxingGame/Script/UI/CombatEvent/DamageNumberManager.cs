using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Transform damageNumberParent;

    void Start()
    {
        BattleManager.Instance.OnAttackSuccess += ShowDamageNumber;
    }

    private void ShowDamageNumber(BattleManager.HitType hitType, float damage)
    {
        GameObject damageNumberObj = Instantiate(damageNumberPrefab, damageNumberParent != null ? damageNumberParent : transform);
        damageNumberObj.GetComponent<DamageNumberController>().Init(hitType, damage);
    }

    void OnDestroy()
    {
        BattleManager.Instance.OnAttackSuccess -= ShowDamageNumber;
    }
}
