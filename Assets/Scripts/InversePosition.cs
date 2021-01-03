using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InversePosition : MonoBehaviour {
    [SerializeField] Transform reference;

    // Update is called once per frame
    void Update() {
        gameObject.transform.position = -reference.position;
    }
}
