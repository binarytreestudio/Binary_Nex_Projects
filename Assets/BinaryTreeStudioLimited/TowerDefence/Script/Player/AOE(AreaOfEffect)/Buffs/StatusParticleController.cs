using UnityEngine;

public class StatusParticleController : MonoBehaviour
{
    public GameObject iceBuff;
    public GameObject posionBuff;

    public void ShowBuff(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Slow:
                iceBuff.SetActive(true);
                break;

            case StatusEffectType.Poison:
                posionBuff.SetActive(true);
                break;
        }
    }

    public void HideBuff(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Slow:
                iceBuff.SetActive(false);
                break;
            case StatusEffectType.Poison:
                posionBuff.SetActive(false);
                break;
        }
    }
}
