using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


namespace TowerDefence
{
    public class BreakThroughEnemyController : MonoBehaviour
    {
        [Header("Break Through Settings")]
        [SerializeField] private float breakThroughDuration;

        [Header("Skin Settings")]
        [SerializeField] private Transform skinListTransform;


        public void Init(EnemyManager.EnemyType skinIndex, Transform breakThroughY, Transform breakThroughDestination)
        {
            GameObject skin = skinListTransform.GetChild((int)skinIndex).gameObject;
            skin.SetActive(true);

            transform.DOMoveY(breakThroughY.position.y, breakThroughDuration / 2).OnComplete(() =>
            {
                transform.DOMove(breakThroughDestination.position, breakThroughDuration).OnComplete(() =>
                {
                    Destroy(gameObject);
                });
            });
        }
    }
}