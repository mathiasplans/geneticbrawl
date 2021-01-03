using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Telegraph : MonoBehaviour {
    [SerializeField] public Transform inversePosition;

    private SpriteRenderer srenderer;
    private bool cancle = false;
    private bool inProgress = false;

    public virtual void SetOnHit(Func<Damagable, float> newOnHit) {

    }

    public virtual void SetScore(Action<float> score) {

    }

    public virtual void Hit(Movable mv, Vector2 direction) {

    }

    public void Start() {
        this.srenderer = gameObject.GetComponent<SpriteRenderer>();

        // Disable both renderer and collider
        this.srenderer.enabled = false;
    }

    private IEnumerator TelegraphCast(float duration, Movable mv, Vector2 direction) {
        // Reset the material to default
        this.srenderer.material.SetFloat("_Progress", 0f);

        // Enable the renderer
        this.srenderer.enabled = true;
        float ellapsed = 0f;

        while (ellapsed < duration) {
            if (this.cancle)
                break;
            this.srenderer.material.SetFloat("_Progress", Mathf.Min(ellapsed / duration, 1.0f));
            yield return new WaitForEndOfFrame();
            ellapsed += Time.deltaTime;
        }

        if (!this.cancle) {
            // Hit the targets
            Vector3 ip = inversePosition.position;
            this.Hit(mv, direction);

            // Disable the renderer
            this.srenderer.enabled = false;
        }

        else {
            this.cancle = false;
        }

        this.inProgress = false;
    }

    public void Cast(float duration, Movable mv, Vector2 direction) {
        // Only one cast at once is possible
        if (!this.inProgress) {
            this.inProgress = true;

            // Do not cancle
            this.cancle = false;

            // Start a coroutine
            StartCoroutine(TelegraphCast(duration, mv, direction));
        }
    }

    public void Cancle() {
        // Stop the coroutine
        this.cancle = true;
    }

    public bool IsInProgress() {
        return this.inProgress;
    }
}
