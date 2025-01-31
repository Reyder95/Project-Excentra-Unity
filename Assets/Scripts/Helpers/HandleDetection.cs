// HandleDetection.cs

using UnityEngine;

// Handles detection for an AoE and an entity.
// This is on the entity, as a smaller box for aoe calculations. Can be thought of similarly for bullet hell games, as well as the tiny "dot" in MMOs like FFXIV. 
public class HandleDetection : MonoBehaviour
{
    public GameObject Entity;
    public EntityController controller;

    public void Start()
    {
        controller = Entity.GetComponent<EntityController>();
    }

    // Determines if you are even eligible to target the entity. Useful helper function for the triggers
    public bool IsAttackable(BaseAoe aoe, EntityStats attackerStats, EntityStats defenderStats) {
        if (ExcentraGame.battleManager.battleVariables.GetCurrentSkill().damageType == DamageType.REVIVE || ExcentraGame.battleManager.IsAlive(defenderStats.gameObject))
        {
            BaseSkill skill = aoe.skill;

            if (skill.entityType == EntityType.ALLY)
            {
                if (defenderStats.isPlayer == attackerStats.isPlayer)
                {

                    if (skill.damageType == DamageType.REVIVE)
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
            else if (skill.entityType == EntityType.ENEMY)
            {
                if (defenderStats.isPlayer != attackerStats.isPlayer)
                {
                    return true;
                }
            }
        }


        return false;
    }

    // For the following triggers, determines if colliding entity is an AoE. If so, check if can target, then "handleTarget" the entity
    public void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject aoe = collision.gameObject;
        if (aoe.tag == "aoe")
        {
            BaseAoe aoeData = aoe.GetComponent<BaseAoe>();
            
            if (aoeData.mechanicAttack != null)
            {
                if (Entity.GetComponent<EntityStats>().isPlayer && Entity.GetComponent<EntityStats>().currentHP > 0)
                {
                    controller.inEnemyAoe = true;
                    //controller.HandleTarget(true);
                    aoeData.HandleAddTarget(Entity);
                }
            }
            else if (ExcentraGame.battleManager.TargetingEligible(aoeData.attackerObject, Entity)) 
            {
                Debug.Log("HELLO!!");
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
            BaseAoe aoeData = aoe.GetComponent<BaseAoe>();

            controller.inEnemyAoe = true;

            if (aoeData.mechanicAttack == null)
                controller.HandleTarget(false);
            aoeData.HandleRemoveTarget(Entity);
        }
    }
}
