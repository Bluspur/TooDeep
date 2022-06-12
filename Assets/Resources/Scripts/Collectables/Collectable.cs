using System;
using UnityEngine;
using Bluspur.Movement;

namespace Bluspur.Collectables
{
    public class Collectable : MonoBehaviour
    {
        [SerializeField] private int value; 

        public static event Action<int> OnCoinCollected;

        private int timesFired = 0;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                timesFired++;
                if(timesFired == 1)
                {
                    OnCoinCollected?.Invoke(value);
                    Destroy(gameObject);
                }
            }
        }

        public static void AddCoins(int value)
        {
            OnCoinCollected?.Invoke(value);
        }
    }
}

