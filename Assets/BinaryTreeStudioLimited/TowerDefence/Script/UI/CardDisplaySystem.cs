using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

public class CardDisplaySystem : MonoBehaviour
{
    [SerializeField] private RectTransform cardContainer; // Container for card UI elements
    public List<PunchCardItem> cardImages; // List of card sprites
    public GameObject cardPrefab; // Reference to the UI Image component to display the card

    public int currentCardIndex = 0;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;

    [Header("Card Position Setting")]
    // Card positioning variables
    public float card_spacing = 50f;
    public float cardLeftPadding = 20f;
    public float card_offset_y = 10f;
    public float card_angle = 10f;

    public void Initialize(Sprite normalPunch)
    {
        // Instantiate card UI elements based on the cardPrefab
        for (int i = 0; i < 6; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardContainer);
            newCard.transform.SetParent(cardContainer, false); // Set parent to the cardContainer
            PunchCardItem item = newCard.GetComponent<PunchCardItem>();
            cardImages.Add(item);
            cardImages[0].SetSprite(normalPunch);
        }
    }

    public void SetPowerUpIcons(List<AppliedPowerUp> powerUps)
    {
        int reverseIndex = -1;
        for (int i = 0; i < powerUps.Count; i++)
        {
            if (powerUps[i] == null)
                continue;
            if (powerUps[i].powerUpType == PowerUpDatabase.PowerUpType.RecoverHP)
                continue;

            reverseIndex = cardImages.Count - 1 - i; // reverse the list index to match the display order
            cardImages[reverseIndex].SetSprite(DatabaseManager.Instance.powerUpDatabase.GetPowerUpData(powerUps[i].powerUpType).icon);
        }
    }

    void Update()
    {
        RecalculateCardPosition();
    }

    public void MoveCardToBottom(int index)
    {
        StartCoroutine(MoveCardToBottomAnimation(index));
    }

    private IEnumerator MoveCardToBottomAnimation(int index)
    {
        //if (cardImages.Count == 0) yield break;

        //PunchCardItem item = cardImages.Pop(cardImages.Count - 1);
        //cardImages.Insert(0, item);
        cardImages.MoveItemInList(cardImages.Count - 1, 0);
        PunchCardItem item = cardImages[0];
        item.PunchAnimation();
        Vector3 direction = Vector3.zero;
        switch (index)
        {
            case -1:
                direction = Vector3.left + Vector3.up;
                //item.transform.rotation = Quaternion.Euler(-2f, 0, 0);
                item.transform.DOLocalRotate(new Vector3(90f, 0, 0), animationDuration);
                break;
            case 0:
                direction = Vector3.up;
                //item.transform.rotation = Quaternion.Euler(0, 0, 0);
                item.transform.DOLocalRotate(new Vector3(90, 0, 0), animationDuration);
                break;
            case 1:
                direction = Vector3.right + Vector3.up;
                //item.transform.rotation = Quaternion.Euler(2f, 0, 0);
                item.transform.DOLocalRotate(new Vector3(90f, 0, 0), animationDuration);
                break;
        }


        // 向上移動
        item.transform.DOMove(item.transform.position + direction * .9f, animationDuration);

        // scale 變大
        item.transform.DOScale(Vector3.one * 1.4f, animationDuration);

        // fade out
        item.Image.DOFade(0f, animationDuration - 0.2f).SetDelay(0.2f);



        yield return new WaitForSeconds(animationDuration);

        item.transform.SetAsFirstSibling();

        item.EndPunchAnimation();
        RecalculateCardPosition();

        item.transform.position = item.deck_desired_position + Vector2.up * 0.5f;
        item.transform.localScale = Vector3.one;

        item.Image.DOFade(1f, 0.2f);

    }
    public void RecalculateCardPosition()
    {
        int index = 0;
        float count_half = cardImages.Count / 2f;
        for (int i = 0; i < cardImages.Count; i++)
        {
            cardImages[i].deck_desired_position = new Vector2((index - count_half) * card_spacing, (index - count_half) * (index - count_half) * -card_offset_y);
            cardImages[i].deck_desired_position += new Vector2(cardContainer.position.x, cardContainer.position.y); // Use container's position

            cardImages[i].deck_desired_angle = (index - count_half) * -card_angle;

            // Apply parent's rotation on top of the desired angle
            cardImages[i].transform.rotation = Quaternion.Euler(cardContainer.rotation.eulerAngles.x, 0f, cardImages[i].deck_desired_angle);

            index++;
        }
    }
}