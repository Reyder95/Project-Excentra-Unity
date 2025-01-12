using UnityEngine;

public class HandleDetection : MonoBehaviour
{
    public GameObject Entity;
    public EntityController controller;

    public void Start()
    {
        controller = Entity.GetComponent<EntityController>();
    }

    public bool IsAttackable(ConeAoe aoe, EntityStats attackerStats, EntityStats defenderStats) {
        if (ExcentraGame.battleManager.battleVariables.GetCurrentAbility().damageType == DamageType.REVIVE || ExcentraGame.battleManager.IsAlive(defenderStats.gameObject))
        {
            Ability ability = aoe.ability;

            if (ability.entityType == EntityType.ALLY)
            {
                if (defenderStats.isPlayer == attackerStats.isPlayer)
                {

                    if (ability.damageType == DamageType.REVIVE)
                    {
                        if (defenderStats.currentHP <= 0)
                            return true;

                        return false;
                    }
                    else
                    {
                        if (defenderStats.currentHP > 0)
                            return true;
                    }

                }
            }
            else if (ability.entityType == EntityType.ENEMY)
            {
                if (defenderStats.isPlayer != attackerStats.isPlayer)
                {
                    return true;
                }
            }
        }


        return false;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject aoe = collision.gameObject;
        if (aoe.tag == "aoe")
        {
            ConeAoe aoeData = aoe.GetComponent<ConeAoe>();

            if (ExcentraGame.battleManager.TargetingEligible(aoeData.attackerObject, Entity)) 
            {
                controller.HandleTarget(true);
                aoeData.HandleAddTarget(Entity);
            }

        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        GameObject aoe = collision.gameObject;

        if (aoe.tag == "aoe")
        {
            controller.HandleTarget(false);
            ConeAoe aoeData = aoe.GetComponent<ConeAoe>();
            aoeData.HandleRemoveTarget(Entity);
        }
    }
}
