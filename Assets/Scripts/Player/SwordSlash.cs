using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class SwordSlash : MonoBehaviour
{
    [SerializeField] float swordForce = 10f;
    [SerializeField] float enemyStopSecond = 0.2f;
    [SerializeField] int swordDamage = 1;
    [SerializeField] Vector2 swordCollisionArea = new Vector2(2.8f, 1.9f);
    [SerializeField] GameObject hitParticle = default;
    [SerializeField] GameObject flashParticle = default;
    GameObject cloneFlashParticle;

    GameObject playerGameObject;
    Player player;

    Collider2D[] allEnemyCollision;
    Collider2D enemyCollision;
    Collider2D spikeCollision;

    private void Awake()
    {
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        player = playerGameObject.GetComponent<Player>();
        Destroy(GetComponent<SpriteRenderer>(), 0.03f);
        Destroy(gameObject, enemyStopSecond + 0.5f);      //hacky way to destroy gameObject

        RotateCollision();

        allEnemyCollision = Physics2D.OverlapBoxAll(transform.position, swordCollisionArea, 0f, LayerMask.GetMask("Enemy"));
        enemyCollision = Physics2D.OverlapBox(transform.position, swordCollisionArea, 0f, LayerMask.GetMask("Enemy"));
        spikeCollision = Physics2D.OverlapBox(transform.position, swordCollisionArea, 0f, LayerMask.GetMask("Spike"));

        DealEnemyDamage();
        IncreasePower();
        PlayerBounce();
        InstHitGameObject();
        InstSpikeFlash();

        EnemyKnockBack();

    }

    private void RotateCollision()
    {
        if (gameObject.transform.localEulerAngles.z == 90 ||
            gameObject.transform.localEulerAngles.z == 270)
        {
            swordCollisionArea = new Vector2(swordCollisionArea.y, swordCollisionArea.x);
        }
    }

    private void DealEnemyDamage()
    {
        foreach (Collider2D collision in allEnemyCollision)
        {
            collision.GetComponent<EnemyHealth>().DealDamage(swordDamage);
        }
    }

    private void PlayerBounce()
    {
        if ((enemyCollision != null ||
            spikeCollision != null) &&
            gameObject.transform.localEulerAngles.z == 270)
        {
            player.PlayerBounce();
        }
    }

    private void InstSpikeFlash()
    {
        if (spikeCollision != null) { 
            Vector2 spikeContactPos = spikeCollision.ClosestPoint(transform.position);
            cloneFlashParticle = Instantiate(flashParticle, spikeContactPos, Quaternion.identity) as GameObject;
            Destroy(cloneFlashParticle, cloneFlashParticle.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        }
    }

    private void EnemyKnockBack()
    {
        Rigidbody2D enemyRB;

        foreach (Collider2D collision in allEnemyCollision)
        {
            enemyRB = collision.gameObject.GetComponent<Rigidbody2D>();

            if (collision.gameObject.GetComponent<EnemyHealth>().GetHealth() <= 0 || collision.gameObject.tag != "Eagle")
            {
                continue;
            }

            if (gameObject.transform.localEulerAngles.z == 90)
            {
                enemyRB.AddForce(new Vector2(0f, swordForce), ForceMode2D.Impulse);
            }
            else if (gameObject.transform.localEulerAngles.z == 270)
            {
                enemyRB.AddForce(new Vector2(0, -swordForce), ForceMode2D.Impulse);
            }
            else
            {
                enemyRB.AddForce(new Vector2(swordForce, 0f) * playerGameObject.transform.localScale, ForceMode2D.Impulse);
            }

            //Stop Enemy Force
            StartCoroutine(StopEnemyForce(enemyRB));
        }

    }

    private IEnumerator StopEnemyForce(Rigidbody2D enemyRB)
    {
        yield return new WaitForSeconds(enemyStopSecond);
        enemyRB.velocity = new Vector2(0f, 0f);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, swordCollisionArea);
    }

    private void IncreasePower()
    {
        foreach (Collider2D collision in allEnemyCollision)
        {
            player.IncreasePowerUI();
        }
    }

    private void InstHitGameObject()
    {
        if (enemyCollision != null)
        {
            Instantiate(hitParticle, transform.position, Quaternion.identity);
        }
    }
}
