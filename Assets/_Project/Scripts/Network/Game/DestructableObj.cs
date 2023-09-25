using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableObj : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 6)
        {
            List<GameObject> obj = new List<GameObject>();

            foreach (Transform child in gameObject.transform)
            {
                obj.Add(child.gameObject);
            }

            GenerateFragments(obj);
        }
    }

    private void GenerateFragments(List<GameObject> fragments)
    {
        foreach (GameObject fragment in fragments)
        {
            fragment.transform.gameObject.SetActive(true);
            fragment.transform.SetParent(null);
            Destroy(fragment, 4);
        }

        Destroy(gameObject);
    }
}
