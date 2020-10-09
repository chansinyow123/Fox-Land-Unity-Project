using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantShootDetection : MonoBehaviour
{
    [SerializeField] Plant plant = default;
    bool done = false;                          // Only Run Once

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !done)
        {
            plant.Shooting();
            done = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && done)
        {
            plant.StopShooting();
            done = false;
        }
    }
}
