using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Dummy : Damagable {
    Color color = Color.red;
    AI ai;
    [SerializeField] Ability ability;
    [SerializeField] Transform playerPos;
    public float strength = 5;
    public float vitality = 3;
    public float dexterity = 1;
    public float awareness = 2;
    public float courage = 2;

    Ability[] abilities;

    bool alive = true;
    bool updateScore = false;
    Action<float> newScore;

    IEnumerator AITick() {
        while (this.alive) {
            yield return new WaitForSeconds(0.1f);
            this.Heal(this.ai.Heal());
        }
    }

    private Action onDie;


    public void SetOnDie(Action newOnDie) {
        this.onDie = newOnDie;
    }

    public override void Die() {
        this.alive = false;
        if (this.onDie != null)
            this.onDie();
        Destroy(gameObject);
    }

    new void Start() {
        base.Start();

        GameObject abilityObject = Instantiate(ability.gameObject, transform);
        abilityObject.tag = gameObject.tag;
        abilityObject.SetActive(true);
        this.ability = abilityObject.GetComponent<Ability>();

        this.abilities = new Ability[1];
        this.abilities[0] = this.ability;
        float[] w = new float[1];
        w[0] = 1;

        this.ai = new AI(strength, vitality, dexterity, awareness, courage, 0, this.abilities, w, 10);
        this.ai.SetPosition(gameObject.GetComponent<Movable>().GetPosition());

        this.hp = this.ai.GetHealth();
        this.maxHp = this.ai.GetMaxHP();

        StartCoroutine(AITick());
    }

    public override void OnStagger() {
        this.color = gameObject.GetComponent<SpriteRenderer>().color;
        gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
    }

    public override void OnDestagger() {
        gameObject.GetComponent<SpriteRenderer>().color = this.color;
    }

    bool useAbility = true;

    IEnumerator AbilityCooldown(float duration) {
        yield return new WaitForSeconds(duration);
        this.useAbility = true;
    }

    Vector2 ppos;
    Vector2 pdir;

    new void Update() {
        base.Update();

        // Score callback
        if (this.updateScore) {
            this.ai.SetScore(this.newScore);
            this.updateScore = false;
        }

        // Update AI health
        this.ai.SetHealth(this.hp);

        if (!this.ai.AbilityInProgress()) {
            this.pdir = this.ai.GetDirection();
            float angle = Mathf.Atan2(this.pdir.y, this.pdir.x) * Mathf.Rad2Deg - 90f;
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }

        Vector2 pos = this.ai.GetPosition();
        Vector3 pos3 = new Vector3(pos.x, pos.y, 0);
        gameObject.GetComponent<Movable>().SetPosition(pos3);

        this.ppos = new Vector2(this.playerPos.position.x, this.playerPos.position.y);
        float cooldown = this.ai.Do(this.ppos, this.useAbility, gameObject.GetComponent<Movable>(), this.pdir);
        if (cooldown != 0f) {
            this.useAbility = false;
            StartCoroutine(AbilityCooldown(cooldown));
        }
    }

    public void SetScore(Action<float> newScore) {
        this.updateScore = true;
        this.newScore = newScore;
    }
}
