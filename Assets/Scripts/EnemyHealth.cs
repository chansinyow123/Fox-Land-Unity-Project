using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int health = 3;
    [SerializeField] GameObject enemyDeathAnimation = default;

    public void DealDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        GameObject cloneDeath = Instantiate(enemyDeathAnimation, transform.position, Quaternion.identity) as GameObject;
        Destroy(cloneDeath, cloneDeath.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }

    public int GetHealth()
    {
        return health;
    }
}
