using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Transform damageNumberParent;

    void Start()
    {
        BattleManager.Instance.OnPlayerAttackSuccess += ShowDamageNumber;
    }

    void OnDestroy()
    {
        BattleManager.Instance.OnPlayerAttackSuccess -= ShowDamageNumber;
    }

    private void ShowDamageNumber(int playerIndex, BattleManager.HitType hitType, float damage, EnemyController.PlayerAttackPath path)
    {
        GameObject damageNumberObj = Instantiate(damageNumberPrefab, damageNumberParent != null ? damageNumberParent : transform);
        damageNumberObj.GetComponent<DamageNumberController>().Init(hitType, damage);
    }


}
