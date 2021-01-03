using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damagable : MonoBehaviour {
    public float maxHp = 1;
    public float hp = 0;
    private bool staggered = false;

    private SpriteRenderer srender;

    public void Start() {
        this.hp = maxHp;
        this.srender = gameObject.GetComponent<SpriteRenderer>();
        this.srender.material.SetFloat("_Health", 1);
    }

    public void Update() {
        this.srender.material.SetFloat("_Health", this.hp / this.maxHp);
    }

    public virtual void OnStagger() {

    }

    public virtual void OnDestagger() {

    }

    private void Stagger() {
        this.staggered = true;
        this.OnStagger();
    }

    private void DeStagger() {
        this.staggered = false;
        this.OnDestagger();
    }

    private IEnumerator Recover(float duration) {
        yield return new WaitForSeconds(duration);
        this.DeStagger();
    }

    public void Damage(float dmg) {
        this.hp -= dmg;
        this.hp = Mathf.Max(0f, this.hp);

        if (this.hp == 0f) {
            this.Die();
        }

        else if (!this.staggered) {
            this.Stagger();
            StartCoroutine(Recover(1));
        }
    }

    public virtual void Die() {
        // Die
    }

    public void Heal(float amount) {
        this.hp += amount;
        this.hp = Mathf.Min(this.maxHp, this.hp);
    }
}
