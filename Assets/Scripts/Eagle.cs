using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eagle : MonoBehaviour
{
    [SerializeField] float flySpeed = 2f;
    [SerializeField] float eagleFollowDistance = 3f;
    [SerializeField] float eagleStopDistance = 9f;

    GameObject playerGameObject;
    Rigidbody2D eagleRB;
    float oldPosX;
    bool isChasing = false;

    // Start is called before the first frame update
    void Start()
    {
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        eagleRB = GetComponent<Rigidbody2D>();
        oldPosX = transform.position.x;
    }

    private void FixedUpdate()
    {
        FollowPlayer();
        FlipSprite();
    }

    private void FollowPlayer()
    {
        float playerDistance = Vector2.Distance(eagleRB.position, playerGameObject.transform.position);

        //Debug.Log(playerDistance);

        if (playerDistance < eagleFollowDistance && eagleRB.velocity.magnitude == 0)
        {
            isChasing = true;
        }

        if (isChasing)
        {
            eagleRB.position = Vector2.MoveTowards(transform.position, playerGameObject.transform.position, flySpeed * Time.deltaTime);

            if (playerDistance > eagleStopDistance)
            {
                isChasing = false;
            }
        }
    }

    private void FlipSprite()
    {
        if (eagleRB.velocity.magnitude == 0) {
            if (oldPosX < transform.position.x)
            {
                transform.localScale = new Vector2(-1, 1);
            }
            if (oldPosX > transform.position.x)
            {
                transform.localScale = new Vector2(1, 1);
            }

            oldPosX = transform.position.x;
        }
    }
}
