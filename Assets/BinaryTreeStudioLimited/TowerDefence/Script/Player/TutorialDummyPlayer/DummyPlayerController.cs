using UnityEngine;

public class DummyPlayerController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void SetTrigger(string name)
    {
        animator.SetTrigger(name);
    }

    public void SetBool(string name, bool value)
    {
        animator.SetBool(name, value);
    }
}
