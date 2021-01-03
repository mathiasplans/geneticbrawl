using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ability : MonoBehaviour {
    [SerializeField] GameObject telegraphObject;
    [SerializeField] float duration;
    [SerializeField] float damage;

    private Telegraph telegraph;
    private float range = 2.5f;
    private float cooldown = 2.5f;
    private bool update;

    bool updateScore = false;
    Action<float> newScore;

    // Start is called before the first frame update
    void Start() {
        GameObject newTelegraph = Instantiate(telegraphObject, transform);
        this.telegraphObject = newTelegraph;

        this.telegraph = this.telegraphObject.gameObject.GetComponent<Telegraph>();
        this.telegraphObject.tag = gameObject.tag;
        this.telegraphObject.SetActive(true);
    }

    void Update() {
        if (this.updateScore) {
            this.telegraph.SetScore(newScore);
            this.updateScore = false;
        }

        if (this.update) {
            this.telegraph.SetOnHit((Damagable target) => {target.Damage(this.damage); return this.damage;});
            this.update = false;
        }
    }

    public void UpdateDamage(float damage) {
        this.damage = damage;
        this.update = true;
    }

    public void Use(Movable mv, Vector2 direction) {
        this.telegraph.Cast(this.duration, mv, direction);
    }

    public void Cancle() {
        this.telegraph.Cancle();
    }

    public bool IsInProgress() {
        return this.telegraph.IsInProgress();
    }

    public float GetRange() {
        return this.range;
    }

    public float GetCooldown() {
        return this.cooldown;
    }

    public void SetScore(Action<float> newScore) {
        this.updateScore = true;
        this.newScore = newScore;
    }
}
