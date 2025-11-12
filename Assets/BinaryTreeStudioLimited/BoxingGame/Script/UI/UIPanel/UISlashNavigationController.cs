using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISlashNavigationController : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;

    [SerializeField] private SlashDetector defaultLeftHandSlashDetector;
    [SerializeField] private SlashDetector defaultRightHandSlashDetector;

    [SerializeField] private GameObject selectorPointerPrefab;
    [SerializeField] private float selectorPointerYOffset = 150;

    private bool gameStarted = false;
    private GameObject selectorPointer = null;

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstSelected);
        selectorPointer = Instantiate(selectorPointerPrefab, transform);
        selectorPointer.transform.position = EventSystem.current.currentSelectedGameObject.transform.position + Vector3.up * selectorPointerYOffset;

        gameStarted = BattleManager.Instance.GameStarted;
        if (gameStarted)
        {
            PlayerManager.Instance.OnPlayerSlashDetected += OnPlayerSlashDetected;
        }
        else
        {
            defaultLeftHandSlashDetector.gameObject.SetActive(true);
            defaultRightHandSlashDetector.gameObject.SetActive(true);

            defaultLeftHandSlashDetector.OnSlashDetected += OnLeftHandSlashDetected;
            defaultRightHandSlashDetector.OnSlashDetected += OnRightHandSlashDetected;
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
            defaultLeftHandSlashDetector.OnSlashDetected -= OnLeftHandSlashDetected;
            defaultRightHandSlashDetector.OnSlashDetected -= OnRightHandSlashDetected;
        }
        defaultLeftHandSlashDetector.gameObject.SetActive(false);
        defaultRightHandSlashDetector.gameObject.SetActive(false);
        Destroy(selectorPointer);
        selectorPointer = null;
    }

    private void OnPlayerSlashDetected(int playerIndex, Jazz.Handedness handedness, Vector2 direction, int combo)
    {
        Navigation navigation = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().navigation;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (handedness == Jazz.Handedness.Left && direction.x > 0)
            {
                EventSystem.current.SetSelectedGameObject(navigation.selectOnRight.gameObject);
                selectorPointer.transform.position = navigation.selectOnRight.gameObject.transform.position + Vector3.up * selectorPointerYOffset;
            }
            else if (handedness == Jazz.Handedness.Right && direction.x < 0)
            {
                EventSystem.current.SetSelectedGameObject(navigation.selectOnLeft.gameObject);
                selectorPointer.transform.position = navigation.selectOnLeft.gameObject.transform.position + Vector3.up * selectorPointerYOffset;
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

    private void OnLeftHandSlashDetected(Vector2 direction)
    {
        OnPlayerSlashDetected(-1, Jazz.Handedness.Left, direction, -1);
    }
    private void OnRightHandSlashDetected(Vector2 direction)
    {
        OnPlayerSlashDetected(-1, Jazz.Handedness.Right, direction, -1);
    }
}
