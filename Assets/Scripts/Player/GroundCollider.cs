using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCollider : MonoBehaviour
{
    Player player;

    void Start()
    {
        player = gameObject.GetComponentInParent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Ground") &&
            collision.gameObject.layer != LayerMask.NameToLayer("Spike"))
        {
            return;
        }

        player.setTouchGroundToTrue();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Ground") &&
            collision.gameObject.layer != LayerMask.NameToLayer("Spike"))
        {
            return;
        }

        player.setTouchGroundToFalse();
    }
}
