using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Damagable {
    [SerializeField] Ability ability;

    private Ability activeAbility = null;
    private Color color = Color.red;
    private Vector3 lastDir;

    new void Start() {
        this.maxHp = 2000;

        base.Start();
        GameObject abilityObject = Instantiate(ability.gameObject, transform);
        abilityObject.tag = gameObject.tag;
        abilityObject.SetActive(true);
        this.ability = abilityObject.GetComponent<Ability>();

        this.ability.SetScore((f) => {});
    }

    public override void OnStagger() {
        this.color = gameObject.GetComponent<SpriteRenderer>().color;
        gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
    }

    public override void OnDestagger() {
        gameObject.GetComponent<SpriteRenderer>().color = this.color;
    }

    // Update is called once per frame
    new void Update() {
        base.Update();

        // Rotate towards cursor
        Vector3 mousePos = Input.mousePosition;
        Vector3 srcPos = Camera.main.WorldToScreenPoint(new Vector3(0, 0, 0));
        mousePos.x -= srcPos.x;
        mousePos.y -= srcPos.y;
        mousePos.Normalize();
        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg - 90f;
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        this.lastDir = mousePos;

        // Use abilities
        if (this.activeAbility == null || !this.activeAbility.IsInProgress()) {
            if (Input.GetKeyDown("1") || Input.GetMouseButtonDown(0)) {
                this.activeAbility = this.ability;
                this.activeAbility.Use(gameObject.GetComponent<Movable>(), mousePos);
            }
        }
    }

    public override void Die() {
        Application.LoadLevel(Application.loadedLevel);
    }
}
