using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    bool completedTutorial = false;
    public bool CompletedTutorial => completedTutorial;
}
