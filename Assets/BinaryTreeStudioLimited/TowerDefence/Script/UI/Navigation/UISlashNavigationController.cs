using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UISlashNavigationController : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;
    [SerializeField] private Nex.Essentials.SlashDetector defaultLeftSlashDetector;
    [SerializeField] private Nex.Essentials.SlashDetector defaultRightSlashDetector;

    private bool gameStarted = false;

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstSelected);

        if (BattleManager.Instance.GameStarted)
        {
            PlayerManager.Instance.OnPlayerSlashDetected += OnPlayerSlashDetected;
            gameStarted = true;
        }
        else
        {
            defaultLeftSlashDetector.OnSlashDetected += LeftSlashDetected;
            defaultRightSlashDetector.OnSlashDetected += RightSlashDetected;
        }
    }

    private void OnDisable()
    {
        if (gameStarted)
        {
            PlayerManager.Instance.OnPlayerSlashDetected -= OnPlayerSlashDetected;
        }
        else
        {
            defaultLeftSlashDetector.OnSlashDetected -= LeftSlashDetected;
            defaultRightSlashDetector.OnSlashDetected -= RightSlashDetected;
        }
        gameStarted = false;
    }

    private void OnPlayerSlashDetected(int playerIndex, Jazz.Handedness handedness, Vector2 direction)
    {
        Debug.Log("UI navigation Slash detected: " + handedness + " Direction: " + direction);
        Navigation navigation = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().navigation;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (handedness == Jazz.Handedness.Left && direction.x > 0)
            {
                EventSystem.current.SetSelectedGameObject(navigation.selectOnRight.gameObject);
            }
            else if (handedness == Jazz.Handedness.Right && direction.x < 0)
            {
                EventSystem.current.SetSelectedGameObject(navigation.selectOnLeft.gameObject);
            }
        }
        else if (direction.y > 0)
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected == null) { Debug.LogWarning("No current selected GameObject"); return; }
            var selectable = currentSelected.GetComponent<Selectable>();
            if (selectable == null) { Debug.LogWarning("Current selected GameObject is not selectable"); return; }
            var pointer = new PointerEventData(EventSystem.current);
            // (position is irrelevant for ExecuteEvents, but some components read it)
            pointer.position = Input.mousePosition;
            ExecuteEvents.Execute(currentSelected, pointer, ExecuteEvents.submitHandler);
            ExecuteEvents.Execute(currentSelected, pointer, ExecuteEvents.pointerClickHandler);
        }
    }

    private void LeftSlashDetected(Vector2 direction)
    {
        OnPlayerSlashDetected(0, Jazz.Handedness.Left, direction);
    }

    private void RightSlashDetected(Vector2 direction)
    {
        OnPlayerSlashDetected(0, Jazz.Handedness.Right, direction);
    }
}

