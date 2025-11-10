using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartScreenController : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;


    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstSelected);

        PlayerManager.Instance.OnPlayerSlashDetected += OnPlayerSlashDetected;
    }

    private void OnDisable()
    {
        PlayerManager.Instance.OnPlayerSlashDetected -= OnPlayerSlashDetected;
    }

    private void OnPlayerSlashDetected(int playerIndex, Jazz.Handedness handedness, Vector2 direction, int combo)
    {
        Navigation navigation = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().navigation;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                EventSystem.current.SetSelectedGameObject(navigation.selectOnRight.gameObject);
            }
            else if (direction.x < 0)
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
}
