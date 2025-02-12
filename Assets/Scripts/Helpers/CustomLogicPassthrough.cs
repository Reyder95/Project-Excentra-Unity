using UnityEngine;

public class CustomLogicPassthrough
{
    public BaseAoe aoe;
    public GameObject attacker;
    public float entityDamage;
    public GameObject target;
    public EnemyMechanic mechanic;

    public CustomLogicPassthrough(BaseAoe aoe, GameObject attacker, float entityDamage, GameObject target, EnemyMechanic mechanic)
    {
        this.aoe = aoe;
        this.attacker = attacker;
        this.entityDamage = entityDamage;
        this.target = target;
        this.mechanic = mechanic;
    }
}
