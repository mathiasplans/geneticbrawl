using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour {
    [SerializeField] Transform reference;
    private Vector3 startPos;
    bool started = false;
    [SerializeField] bool movable = true;

    void Start() {
        if (!started)
            this.startPos = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update() {
        if (!this.movable)
            return;

        Vector3 pos = gameObject.transform.position;
        pos.x = this.reference.position.x + this.startPos.x;
        pos.y = this.reference.position.y + this.startPos.y;
        gameObject.transform.position = pos;
    }

    public void SetPosition(Vector3 newPos) {
        this.startPos = newPos;
        this.started = true;
    }

    public Vector3 GetPosition() {
        return this.startPos;
    }

    public Vector3 GetWorldPosition() {
        if (this.movable)
            return this.reference.position + this.startPos;

        return this.startPos;
    }
}
