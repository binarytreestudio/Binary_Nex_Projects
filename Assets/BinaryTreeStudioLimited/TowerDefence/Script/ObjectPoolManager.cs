using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        [Header("Fireball Pool")]
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private List<GameObject> fireBalls;

        [Header("ICE AOE Pool")]
        [SerializeField] private GameObject iceAOEPrefab;
        [SerializeField] private List<GameObject> iceAOEs;

        [Header("Poison AOE Pool")]
        [SerializeField] private GameObject poisonAOEPrefab;
        [SerializeField] private List<GameObject> poisonAOEs;

        [Header("Enemy Particle Effect Pool")]
        [SerializeField] private GameObject enemyHitParticlePrefab;
        [SerializeField] private List<GameObject> enemyHitParticles;
        [SerializeField] private GameObject enemyDeathParticlePrefab;
        [SerializeField] private List<GameObject> enemyDeathParticles;

        public void PreloadFireBalls(int count = 20)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject fireball = Instantiate(fireballPrefab);
                fireball.SetActive(false);
                fireBalls.Add(fireball);
            }
        }
        public GameObject GetFireBall()
        {
            var fireball = fireBalls.Find(fb => !fb.activeInHierarchy);

            if (fireball == null)
            {
                fireball = Instantiate(fireballPrefab);
                fireBalls.Add(fireball);
            }

            fireball.SetActive(true);
            return fireball;
        }

        public void PreloadIceAOEs(int count = 10)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject iceAOE = Instantiate(iceAOEPrefab);
                iceAOE.SetActive(false);
                iceAOEs.Add(iceAOE);
            }
        }
        public GameObject GetIceAOE()
        {
            var iceAOE = iceAOEs.Find(aoe => !aoe.activeInHierarchy);

            if (iceAOE == null)
            {
                iceAOE = Instantiate(iceAOEPrefab);
                iceAOEs.Add(iceAOE);
            }

            iceAOE.SetActive(true);
            return iceAOE;
        }

        public void PreloadPoisonAOEs(int count = 10)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject poisonAOE = Instantiate(poisonAOEPrefab);
                poisonAOE.SetActive(false);
                poisonAOEs.Add(poisonAOE);
            }
        }
        public GameObject GetPoisonAOE()
        {
            var poisonAOE = poisonAOEs.Find(aoe => !aoe.activeInHierarchy);

            if (poisonAOE == null)
            {
                poisonAOE = Instantiate(poisonAOEPrefab);
                poisonAOEs.Add(poisonAOE);
            }

            poisonAOE.SetActive(true);
            return poisonAOE;
        }

        public void PreloadEnemyParticles(int count = 20)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject hitParticle = Instantiate(enemyHitParticlePrefab);
                hitParticle.SetActive(false);
                enemyHitParticles.Add(hitParticle);

                GameObject deathParticle = Instantiate(enemyDeathParticlePrefab);
                deathParticle.SetActive(false);
                enemyDeathParticles.Add(deathParticle);
            }
        }

        public GameObject GetEnemyHitParticle()
        {
            var particle = enemyHitParticles.Find(p => !p.activeInHierarchy);

            if (particle == null)
            {
                particle = Instantiate(enemyHitParticlePrefab);
                enemyHitParticles.Add(particle);
            }

            particle.SetActive(true);
            return particle;
        }
        public GameObject GetEnemyDeathParticle()
        {
            var particle = enemyDeathParticles.Find(p => !p.activeInHierarchy);

            if (particle == null)
            {
                particle = Instantiate(enemyDeathParticlePrefab);
                enemyDeathParticles.Add(particle);
            }

            particle.SetActive(true);
            return particle;
        }
    }
}