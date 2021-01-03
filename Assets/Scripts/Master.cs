using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Master : MonoBehaviour {
    [SerializeField] Dummy dummy;
    [SerializeField] uint spawnCount;

    private System.Random rng;

    struct Gene : IComparer {
        public float strength;
        public float vitality;
        public float dexterity;
        public float awareness;
        public float courage;

        public float score;

        int IComparer.Compare(object x, object y) {
            Gene first = (Gene) x;
            Gene second = (Gene) y;
            return (int) ((second.score - first.score) * 1000);
        }
    }

    bool[] died;
    Gene[] genePool;

    bool checkEnd = false;

    // Start is called before the first frame update
    void Start() {
        this.rng = new System.Random();

        this.died = new bool[this.spawnCount];
        this.genePool = new Gene[this.spawnCount];

        // Reset
        this.ResetPool();

        // Fill with random
        this.FillRandom();

        // Spawn
        this.SpawnSpecimen();
    }

    private void SortBestGenes() {
        Array.Sort(this.genePool, new Gene());

        // Print out best
        Console.WriteLine("Best score: " + this.genePool[0]);
    }

    private void ResetPool() {
        for (uint i = 0; i < this.spawnCount; ++i) {
            died[i] = false;
        }
    }

    private void FillRandom() {
        for (uint i = 0; i < this.spawnCount; ++i) {
            this.genePool[i] = CreateSpecimen();
        }
    }

    private void FillBreed() {
        // First and second will stay the best
        // Others..
        for (uint i = 2; i < this.spawnCount; ++i) {
            this.genePool[i] = BreedSpecimen(this.genePool[0], this.genePool[1], (float) (i - 2) * 1.5f);
        }
    }

    void Update() {
        if (this.checkEnd) {
            bool realEnd = true;
            foreach (bool dead in died) {
                realEnd &= dead;
            }

            if (realEnd) {
                // Reset the pool
                this.ResetPool();

                // Get the best genes in front
                this.SortBestGenes();

                // Fill with bred specimen
                this.FillBreed();

                // Spawn them
                this.SpawnSpecimen();
            }

            this.checkEnd = false;
        }
    }

    private Action<float> GetScoreFunction(uint slotIndex) {
        return (score) => {this.genePool[slotIndex].score += score;};
    }

    private Action GetOnDieFunction(uint slotIndex) {
        return () => {this.died[slotIndex] = true; this.checkEnd = true;};
    }

    private Gene CreateSpecimen() {
        Gene gene = new Gene();

        gene.strength = rng.Next(1, 10);
        gene.vitality = rng.Next(1, 10);
        gene.dexterity = rng.Next(1, 10);
        gene.awareness = rng.Next(1, 10);
        gene.courage = rng.Next(7, 15);

        return gene;
    }

    private float RandNorm() {
        return rng.Next(0, 101) / 100f;
    }

    private float RandNormNeg() {
        return rng.Next(-100, 101) / 100f;
    }

    private Gene BreedSpecimen(Gene mother, Gene father, float mutation) {
        Gene gene = new Gene();

        gene.strength = Mathf.Lerp(mother.strength, father.strength, this.RandNorm()) + mutation * this.RandNormNeg();
        gene.vitality = Mathf.Lerp(mother.vitality, father.vitality, this.RandNorm()) + mutation * this.RandNormNeg();
        gene.dexterity = Mathf.Lerp(mother.dexterity, father.dexterity, this.RandNorm()) + mutation * this.RandNormNeg();
        gene.awareness = Mathf.Lerp(mother.awareness, father.awareness, this.RandNorm()) + mutation * this.RandNormNeg();
        gene.courage = Mathf.Lerp(mother.courage, father.courage, this.RandNorm()) + mutation * this.RandNormNeg();

        // Clamp
        gene.strength = Mathf.Clamp(gene.strength, 0, 30);
        gene.vitality = Mathf.Clamp(gene.vitality, 0, 30);
        gene.dexterity = Mathf.Clamp(gene.dexterity, 0, 30);
        gene.awareness = Mathf.Clamp(gene.awareness, 0, 30);
        gene.courage = Mathf.Clamp(gene.courage, 0, 30);

        return gene;
    }

    private void SpawnSpecimen() {
        for (uint i = 0; i < this.spawnCount; ++i) {
            Gene gene = this.genePool[i];
            GameObject go = Instantiate(this.dummy.gameObject, transform);
            Dummy d = go.GetComponent<Dummy>();
            d.strength = gene.strength;
            d.vitality = gene.vitality;
            d.dexterity = gene.dexterity;
            d.awareness = gene.awareness;
            d.courage = gene.courage;

            // Callbacks
            d.SetScore(this.GetScoreFunction(i));
            d.SetOnDie(this.GetOnDieFunction(i));

            Movable m = go.GetComponent<Movable>();
            m.SetPosition(new Vector3(rng.Next(-9, 9), rng.Next(-9, 9), 0));

            go.SetActive(true);
        }
    }
}
