using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ParticleLine : MonoBehaviour
{
    public ParticleSystem _particleSystem;
    public GameObject attacker;
    public GameObject target;

    private ParticleSystem.ShapeModule _shapeModule;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        //_shapeModule = _particleSystem.shape;
    }

    // Update is called once per frame
    void Update()
    {
        if (attacker != null && target != null && _particleSystem != null)
        {
            gameObject.transform.position = attacker.transform.position;
            gameObject.transform.LookAt(target.transform.position);
        }
    }

    public void SetContents(GameObject attacker, GameObject target)
    {
        this.attacker = attacker;
        this.target = target;

        if (target == null)
            return;

        _particleSystem = GetComponent<ParticleSystem>();
        var triggerModule = _particleSystem.trigger;

        // Set new collider
        var newCollider = target.GetComponent<BoxCollider2D>();
        if (newCollider != null)
        {
            triggerModule.SetCollider(0, newCollider);
        }
        else
        {
            Debug.LogWarning("Target does not have a BoxCollider2D!");
        }
    }

    public void ModifyParticleOpacity()
    {
        ParticleSystem ps = _particleSystem;
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
        int numParticles = ps.GetParticles(particles);

        Debug.Log(numParticles);

        for (int i = 0; i < numParticles; i++)
        {
            ParticleSystem.Particle p = particles[i];
            Vector3 particleWorldTransform = transform.TransformPoint(p.position);
            float attackerToParticle = Vector2.Distance(particleWorldTransform, attacker.transform.position);
            float attackerToTarget = Vector2.Distance(attacker.transform.position, target.transform.position);

            if (attackerToParticle > attackerToTarget)
            {
                Color32 pColor = p.startColor;
                pColor.a = 0;
                p.startColor = pColor;
            }

            particles[i] = p;
        }

        ps.SetParticles(particles, numParticles);
    }

    void OnParticleTrigger()
    {
        ParticleSystem ps = _particleSystem;

        // particles
        List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
        List<ParticleSystem.Particle> exit = new List<ParticleSystem.Particle>();

        // get
        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        int numExit = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);

        // iterate
        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle p = enter[i];
            Color32 pColor = p.startColor;
            pColor.a = 0;
            p.startColor = pColor;
            enter[i] = p;
        }
        for (int i = 0; i < numExit; i++)
        {
            ParticleSystem.Particle p = exit[i];
            Vector3 particleWorldTransform = transform.TransformPoint(p.position);
            float attackerToParticle = Vector2.Distance(particleWorldTransform, attacker.transform.position);
            float attackerToTarget = Vector2.Distance(attacker.transform.position, target.transform.position);

            if (attackerToParticle < attackerToTarget)
            {
                Color32 pColor = p.startColor;
                pColor.a = 255;
                p.startColor = pColor;
            }

            exit[i] = p;
        }

        // set
        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);
    }
}
