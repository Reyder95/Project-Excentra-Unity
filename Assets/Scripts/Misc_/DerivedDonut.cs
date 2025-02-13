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
        circleMask.transform.localScale = circleMaskScale;

        if (this.originObject != null)
            circlePosition = this.originObject.transform.position;

        transform.position = circlePosition;
    }

    public override void InitializeEnemyAoe(GameObject attackerObject, EnemyMechanic mechanic, MechanicAttack attack, SkillInformation info)
    {
        base.InitializeEnemyAoe(attackerObject, mechanic, attack, info);

        innerRadius = attack.innerDonutSize;
    }
}
