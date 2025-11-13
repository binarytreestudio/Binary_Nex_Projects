using Unity.Barracuda;
using UnityEngine;

public class ObjectTimeOut : MonoBehaviour
{
    [SerializeField] private float timeOutDuration = 5f;
    void Start()
    {
        Destroy(gameObject, timeOutDuration);
    }
}
