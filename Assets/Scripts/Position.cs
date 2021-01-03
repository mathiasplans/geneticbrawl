using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position : MonoBehaviour {
    [SerializeField] float movementSpeed;
    [SerializeField] float dashDistance;
    [SerializeField] float dashSpeed;
    [SerializeField] float dashCooldown;
    private Vector2 dirEnable = new Vector2();
    private Vector2 lastDir = new Vector2();
    private bool canUseDash = true;

    IEnumerator Dash() {
        float travelledDistance = 0;
        Vector3 tp;
        Vector3 dist;
        float speed;
        while (travelledDistance < dashDistance) {
            tp = gameObject.transform.position;
            speed = Time.deltaTime * this.dashSpeed;
            dist = this.lastDir * speed;
            tp.x += dist.x;
            tp.y += dist.y;
            gameObject.transform.position = tp;
            travelledDistance += dist.magnitude;
            yield return 0;
        }

        // Cooldown
        yield return new WaitForSeconds(dashCooldown);
        this.canUseDash = true;
    }

    // Update is called once per frame
    void Update() {
        // TODO: use subroutines to smoothly introduce the direction
        this.dirEnable.x = 0;
        this.dirEnable.y = 0;
        if (Input.GetKey("w")) {
            this.dirEnable.y += 1;
            this.lastDir.y = 1;
        }

        if (Input.GetKey("a")) {
            this.dirEnable.x -= 1;
            this.lastDir.x = -1;
        }

        if (Input.GetKey("s")) {
            this.dirEnable.y -= 1;
            this.lastDir.y = -1;
        }

        if (Input.GetKey("d")) {
            this.dirEnable.x += 1;
            this.lastDir.x = 1;
        }

        if (this.canUseDash && Input.GetKey("space")) {
            this.canUseDash = false;
            StartCoroutine(Dash());
        }

        // Normalize the dirEnable
        if (this.dirEnable.x != 0 || this.dirEnable.y != 0)
            this.dirEnable.Normalize();

        // Normalize last direction
        if (this.lastDir.x != 0 || this.lastDir.y != 0)
            this.lastDir.Normalize();

        float movement = Time.deltaTime * this.movementSpeed;
        Vector3 tp = gameObject.transform.position;
        tp.x += this.dirEnable.x * movement;
        tp.y += this.dirEnable.y * movement;
        gameObject.transform.position = tp;
    }
}
