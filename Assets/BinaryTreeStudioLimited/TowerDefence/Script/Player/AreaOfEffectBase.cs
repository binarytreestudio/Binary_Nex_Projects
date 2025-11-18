using Unity.VisualScripting;
using UnityEngine;

public class AreaOfEffectBase : MonoBehaviour
{
    public virtual void OnEnterAOE(EnemyController enemy)
    {

    }
    public virtual void OnExitAOE(EnemyController enemy)
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy") == false)
            return;

        EnemyController controller = other.GetComponent<EnemyController>();
        if (controller != null)
        {
            OnEnterAOE(controller);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy") == false)
            return;

        EnemyController controller = other.GetComponent<EnemyController>();
        if (controller != null)
        {
            OnExitAOE(controller);
        }
    }


}
