using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    [SerializeField] private bool completedTutorial = false;
    public bool CompletedTutorial => completedTutorial;
}
