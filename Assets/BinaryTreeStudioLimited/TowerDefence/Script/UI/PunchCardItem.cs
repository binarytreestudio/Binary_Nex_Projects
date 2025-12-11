using UnityEngine;
using UnityEngine.UI;
public class PunchCardItem : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image image;
    public Image Image { get => image; }


    [SerializeField] private bool _isAnimating = false;
    public bool IsAnimating { get => _isAnimating; private set => _isAnimating = value; }

    public Vector2 deck_desired_position;
    public float deck_desired_angle;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (IsAnimating)
            return;

        rectTransform.position = Vector2.Lerp(rectTransform.position, deck_desired_position, Time.deltaTime * 10f);
        rectTransform.rotation = Quaternion.Lerp(rectTransform.rotation, Quaternion.Euler(0, 0, deck_desired_angle), Time.deltaTime * 10f);
    }

    public void SetSprite(Sprite sprite)
    {
        Image.sprite = sprite;
    }
    public void PunchAnimation()
    {
        _isAnimating = true;
    }
    public void EndPunchAnimation()
    {
        _isAnimating = false;
    }
}
