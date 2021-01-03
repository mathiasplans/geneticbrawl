using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AI {
    // Possible activities that AI can do
    enum Activity {
        PATROL,          // Just walk around
        WARCRY,          // Let other AIs know where the target is
        STAND_STILL,     // Don't move
        APPROACH,        // Approach the target
        RETREAT,         // Retreat from the target
        ATTACK           // Attack
    }

    // Stats
    // Makes attack stronger, but while attacking, the movement is slower
    float strength;

    // The AI has more health, but it also moves and attacks slower
    float vitality;

    // The AI attacks faster, but has reduced damage
    float dexterity;

    // The AI sees further, but has reduced warcry radius
    float awareness;

    // The AI approaches the target faster and flees less likely
    float courage;

    // The AI does more damage with melee, but less damage with ranged
    float meleeAffinity;

    // Modifiers
    public float attackDamage;
    float attackSpeed;
    float movementSpeed;
    float attackSlowdown;
    float healthMax;
    float healthRegen;
    float meleeRange;
    float sightRange;
    float warcryRange;
    float approachSpeed;
    float fleeSpeed;
    float fleeProb;
    float meleeDamage;
    float rangedDamage;

    // Spatial attributes
    Vector2 position;
    Vector2 targetLocation;
    Vector2 lastDir;
    float maxDist;

    // Abilities!
    Ability[] abilities;
    float[] abilityWeights;

    // Internal state
    bool targetLocationKnown;
    float health;
    System.Random rnd;

    public AI(float strength, float vitality, float dexterity, float awareness, float courage, float meleeAffinity, Ability[] abilities, float[] abilityWeights, float maxDist) {
        this.strength = strength;
        this.vitality = vitality;
        this.dexterity = dexterity;
        this.awareness = awareness;
        this.courage = courage;
        this.meleeAffinity = meleeAffinity;

        this.abilities = abilities;
        this.abilityWeights = abilityWeights;

        // Calculate modifiers
        this.attackDamage = this.strength * 30;
        this.attackSpeed = 2f - this.vitality / 10 + this.dexterity / 3;
        this.movementSpeed = 10f - this.vitality / 10;
        this.attackSlowdown = this.strength / 15 + this.vitality / 5;
        this.healthMax = 100 * this.vitality;
        this.healthRegen = this.vitality / 5;
        this.meleeRange = this.vitality / 10;
        this.sightRange = 7 + this.awareness;
        this.warcryRange = 3 - this.awareness / 3;
        this.approachSpeed = 10 + this.courage + this.movementSpeed;
        this.fleeSpeed = 10 - this.courage + this.movementSpeed;
        this.fleeProb = -this.courage;
        this.meleeDamage = 25f + this.meleeDamage + this.attackDamage;
        this.rangedDamage = 25f - this.meleeDamage + this.attackDamage;

        this.health = this.healthMax;

        // Spatial
        this.maxDist = maxDist;
        rnd = new System.Random();
        this.lastDir = new Vector2();
        this.lastDir.x = rnd.Next(-10, 10);
        this.lastDir.y = rnd.Next(-10, 10);
        this.lastDir.Normalize();

        // Movement reduction
        float r = 7;
        this.movementSpeed /= r;
        this.approachSpeed /= r;
        this.fleeSpeed /= r;

        // Update abilities
        foreach (Ability ability in this.abilities) {
            ability.UpdateDamage(this.attackDamage);
        }
    }

    public void SetScore(Action<float> newScore) {
        foreach (Ability ability in this.abilities) {
            ability.SetScore(newScore);
        }
    }

    // Set a new position
    public void SetPosition(Vector2 newPos) {
        this.position = newPos;
    }

    private Vector2 TargetDir() {
        return this.targetLocation - this.position;
    }

    private float DistanceFromTarget() {
        return this.TargetDir().magnitude;
    }

    private float[] AbilityFits() {
        float[] w = new float[this.abilities.Length];
        for (uint i = 0; i < this.abilities.Length; ++i) {
            w[i] = this.abilities[i].GetRange() - this.DistanceFromTarget();
        }

        return w;
    }

    private float[] AbilityWeightsStand(float[] fits) {
        float[] weights = new float[fits.Length];

        for (uint i = 0; i < 0; ++i) {
            if (fits[i] < 0)
                weights[i] = -1;

            else
                weights[i] = (100f - fits[i]) * this.abilityWeights[i];
        }

        return weights;
    }

    private float[] AbilityWeightsMovement(float[] fits) {
        float[] weights = new float[fits.Length];

        for (uint i = 0; i < 0; ++i) {
            weights[i] = (100f - Mathf.Abs(fits[i])) * this.abilityWeights[i];

            if (fits[i] < 0)
                weights[i] += this.fleeProb;
        }

        return weights;
    }

    float lost = 0;

    private float PatrolWeigth() {
        if (this.lost > 10) {
            this.targetLocationKnown = false;
            this.lost = 0;
        }

        if (this.targetLocationKnown)
            return lost;

        return 10000f;
    }

    private float Max(float[] o) {
        float mx = float.MinValue;
        foreach (float f in o) {
            if (f > mx) {
                mx = f;
            }
        }

        return mx;
    }

    bool movedLast = false;

    private float StandStillWeigth(float[] fits) {
        if (!this.targetLocationKnown)
            return -1;

        // Get the weights for abilities
        float[] w = this.AbilityWeightsStand(fits);

        float moveBias = 0;
        if (this.movedLast)
            moveBias = -1;

        // Get the maximum weight
        return (Max(w) + this.health / (this.healthMax * this.courage + 30)) / 2 + moveBias;
    }

    bool approachLast = false;

    private float ApproachWeight(float abilityMovementMax) {
        if (!this.targetLocationKnown)
            return -1;

        float approachBias = 0;
        if (this.approachLast)
            approachBias = 1;

        return abilityMovementMax + (this.health * this.courage) / (this.healthMax + 20) - 20 / this.DistanceFromTarget() + approachBias;
    }

    bool retreatLast = false;

    private float RetreatWeight(float abilityMovementMax) {
        if (!this.targetLocationKnown)
            return -1;

        if (this.position.magnitude > this.maxDist)
            return -1;

        float retreatBias = 0;
        if (this.retreatLast)
            retreatBias = 2;

        return abilityMovementMax + (this.healthMax + this.fleeProb * 10) / (this.health + 20) / 2 + retreatBias;
    }

    private Ability AttackWeight(float[] fits) {
        int bestAbilityIndex = -1;
        float bestFit = float.MinValue;

        for (int i = 0; i < fits.Length; ++i) {
            if (bestFit < fits[i]) {
                bestAbilityIndex = i;
                bestFit = fits[i];
            }
        }

        if (bestAbilityIndex == -1 || bestFit < 0)
            return null;

        return this.abilities[bestAbilityIndex];
    }

    // Do the AI movement part
    private void Move(float[] fits, float delta) {
        float debugP, debugS, debugA, debugR;

        // Movement operations are:
        // * PATROL -> WARCRY (only spotter)
        // * STAND_STILL
        // * APPROACH
        // * RETREAT
        Activity toTake = Activity.PATROL;
        float bestWeigth = this.PatrolWeigth();
        debugP = bestWeigth;

        // Stand
        float newWeigth = StandStillWeigth(fits);
        debugS = newWeigth;
        if (newWeigth > bestWeigth) {
            bestWeigth = newWeigth;
            toTake = Activity.STAND_STILL;
        }

        // Approach
        float[] w = AbilityWeightsMovement(fits);
        float wm = Max(w);
        newWeigth = ApproachWeight(wm);
        debugA = newWeigth;
        if (newWeigth > bestWeigth) {
            bestWeigth = newWeigth;
            toTake = Activity.APPROACH;
        }

        // Retreat
        newWeigth = RetreatWeight(wm);
        debugR = newWeigth;
        if (newWeigth > bestWeigth) {
            bestWeigth = newWeigth;
            toTake = Activity.RETREAT;
        }

        // Debug.Log("P: " + debugP + " S: " + debugS + " A: " + debugA + " R: " + debugR);

        // Now do movement
        this.approachLast = false;
        this.retreatLast = false;

        Vector2 slowdown = new Vector2(0, 0);
        if (this.AbilityInProgress()) {
            slowdown.x = this.attackSlowdown;
            slowdown.y = this.attackSlowdown;
        }

        switch(toTake) {
            case Activity.PATROL:
                // Scan for targets
                if (this.DistanceFromTarget() < this.sightRange) {
                    // Target is known
                    this.targetLocationKnown = true;
                    break;
                }

                // Else, move somewhere
                this.position += (this.lastDir * this.movementSpeed - slowdown) * delta;
                if (this.position.magnitude > this.maxDist) {
                    this.lastDir = -this.lastDir + new Vector2(0.2f, 0.1f);
                }
                this.movedLast = true;
                break;

            case Activity.STAND_STILL:
                // Don't move
                this.movedLast = false;
                break;

            case Activity.APPROACH:
                // Move towards target
                this.position += (this.TargetDir().normalized * this.approachSpeed - slowdown) * delta;
                this.movedLast = true;
                this.approachLast = true;
                break;

            case Activity.RETREAT:
                // Move away from the target
                this.position += (-this.TargetDir().normalized * this.fleeSpeed - slowdown) * delta;
                this.movedLast = true;
                this.retreatLast = true;
                break;

            default:
                break;
        }
    }

    Ability activeAbility;

    // Do the AI combat part
    private float Combat(float[] fits, bool useAbility, Movable mv, Vector2 dir) {
        // Combat operations are:
        // * ATTACK
        // but also what abilities to use

        // Get the best ability to use
        this.activeAbility = AttackWeight(fits);

        // Only use the ability if its not null
        if (this.activeAbility != null && useAbility && !this.activeAbility.IsInProgress()) {
            this.activeAbility.Use(mv, dir);
            return this.activeAbility.GetCooldown();
        }

        return 0;
    }

    public float Do(Vector2 targetPos, bool useAbility, Movable mv, Vector2 dir) {
        this.targetLocation = targetPos;
        float[] fits = AbilityFits();
        Move(fits, Time.deltaTime);
        if (useAbility)
            return Combat(fits, useAbility, mv, dir);

        return 0;
    }

    public Vector2 GetPosition() {
        return this.position;
    }

    public Vector2 GetDirection() {
        return this.TargetDir().normalized;
    }

    public bool AbilityInProgress() {
        if (this.activeAbility == null)
            return false;

        return this.activeAbility.IsInProgress();
    }

    public float GetHealth() {
        return this.health;
    }

    public void SetHealth(float hp) {
        this.health = hp;
    }

    public float GetMaxHP() {
        return this.healthMax;
    }

    public float Heal() {
        return this.healthRegen;
    }
}
