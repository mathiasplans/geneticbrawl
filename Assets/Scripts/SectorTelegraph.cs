using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SectorTelegraph : Telegraph {
    private float size = 0;
    public Func<Damagable, float> onHit = DefaultOnHit;
    public Action<float> score;

    static void DefaultScore(float score) {

    }

    static float DefaultOnHit(Damagable target) {
        target.Damage(100f);
        return 100f;
    }

    public override void SetOnHit(Func<Damagable, float> newOnHit) {
        this.onHit = newOnHit;
    }

    public override void SetScore(Action<float> newScore) {
        this.score = newScore;
    }

    new void Start() {
        base.Start();
        this.size = gameObject.GetComponent<SpriteRenderer>().material.GetFloat("_Size");
    }

    public override void Hit(Movable mv, Vector2 direction) {
        // Get the list of colliders in the circle
        Vector3 mvpos = mv.GetWorldPosition();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(mvpos, 2.5f);

        // Iterate through each collider
        Damagable d = null;
        foreach (Collider2D collider in colliders) {
            // Don't damage allies and thyself
            if (collider.gameObject.tag == gameObject.tag)
                continue;

            try {
                d = collider.gameObject.GetComponent<Damagable>();
            }

            catch (Exception) {
                continue;
            }

            // Get the position of the offender
            Vector2 pos = collider.gameObject.transform.position;

            // Normalize
            Vector2 dirToTarget = pos - new Vector2(mvpos.x, mvpos.y);
            dirToTarget.Normalize();

            // Get the dot product with the direction
            float angle = Mathf.Acos(Vector2.Dot(dirToTarget, direction.normalized));

            // If the other is in the telegraph
            if (angle < size) {
                this.score(this.onHit(d));
            }
        }
    }
}
