using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageDealer : MonoBehaviour
{
    [SerializeField] int damageDeal = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DealDamageToPlayer(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        DealDamageToPlayer(collision);
    }

    private void DealDamageToPlayer(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Yeah");
        collision.GetComponent<Player>().TakeDamage(damageDeal);
    } 
}
