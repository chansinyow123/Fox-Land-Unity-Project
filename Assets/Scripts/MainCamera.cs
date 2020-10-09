using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField] Player py = default;

    void LateUpdate()
    {
        transform.position = new Vector3(py.transform.position.x, py.transform.position.y + 3, transform.position.z);
    }
}
