public class DontDestroyOnLoad : Singleton<DontDestroyOnLoad, RemoveLateComer>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);
    }
}