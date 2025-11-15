using UnityEngine;

public class DerivedDonut : DerivedCircle
{
    public GameObject circleMask;
    public float innerRadius = 1f;
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        Vector3 newScale = circleAoe.transform.localScale;
        Vector2 circleMaskScale = circleMask.transform.localScale;

        if (this.mechanicAttack != null && (this.mechanicAttack.nonUniformDimensions || this.mechanicAttack.raidWide))
        {
            newScale.y = height * 2;
            newScale.x = width * 2;
        }
        else
        {
            newScale.y = radius;
            newScale.x = radius;
        }

        circleMaskScale.y = innerRadius;
        circleMaskScale.x = innerRadius;


        circleAoe.transform.localScale = newScale;
        indicatorCircle.transform.localScale = newScale;
        circleMask.transform.localScale = circleMaskScale;

        if (this.originObject != null)
            circlePosition = this.originObject.transform.position;

        transform.position = circlePosition;

        SpriteRenderer circleRenderer = indicatorCircle.GetComponent<SpriteRenderer>();

        Color circleColor = circleRenderer.color;

        if (activatingAttack)
        {

            circleColor.a += 0.5f * Time.deltaTime;

            if (circleColor.a > 0.3f)
                circleColor.a = 0.3f;

            if (circleColor.a >= 0.3f && circleColor.a >= 0.3f)
            {
                activatingAttack = false;
                queueEndTurn = true;

                BossMechanicHandler.ActivateAoeAttack(mechanic, mechanicAttack, ExcentraGame.battleManager, attackerObject, this);
            }
        }

        if (circleColor.a > 0 && !activatingAttack)
            circleColor.a -= 0.5f * Time.deltaTime;
        circleRenderer.color = circleColor;

        if (queueEndTurn && circleRenderer.color.a <= 0)
        {
            ExcentraGame.battleManager.EndCurrentAoeTurn();
            queueEndTurn = false;
        }
    }

    public override void InitializeEnemyAoe(GameObject attackerObject, EnemyMechanic mechanic, MechanicAttack attack, SkillInformation info)
    {
        base.InitializeEnemyAoe(attackerObject, mechanic, attack, info);

        innerRadius = attack.innerDonutSize;
    }
}
