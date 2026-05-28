using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    // this allows you to hit a box and it gets destroyed after get a gem after.
    [SerializeField] private GameObject effectPrefab;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            Instantiate(effectPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
