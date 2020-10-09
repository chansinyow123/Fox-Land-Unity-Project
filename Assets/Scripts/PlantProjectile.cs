using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantProjectile : MonoBehaviour
{
    [SerializeField] float speed = 3f;
    [SerializeField] GameObject impactSprite = default;
    [SerializeField] int damageDeal = 1;

    bool done = false;

    Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = new Vector2(speed * -transform.localScale.x, 0f);
    }

    private IEnumerator OnTriggerEnter2D(Collider2D collision)
    {
        yield return StartCoroutine(DealDamageToPlayer(collision));

        if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy") &&
            collision.gameObject.layer != LayerMask.NameToLayer("Default"))
        {
            Debug.Log("okCool");
            GameObject cloneImpact = Instantiate(impactSprite, transform.position, Quaternion.identity) as GameObject;
            Destroy(cloneImpact, cloneImpact.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
            Destroy(gameObject);
        }
    }

    private IEnumerator DealDamageToPlayer(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            yield return null;
        }

        Debug.Log("Yeah");
        collision.GetComponent<Player>().TakeDamage(damageDeal);
    }
}
