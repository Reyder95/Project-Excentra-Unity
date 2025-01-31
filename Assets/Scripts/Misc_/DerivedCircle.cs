using UnityEngine;

public class DerivedCircle : BaseAoe
{
    public float radius = 2f;
    public GameObject circleAoe;
    public Vector2 circlePosition;

    public Vector2 frozenPosition;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        // Get the mouse position in screen coordinates (pixels)
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convert the screen position to world space (2D)
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // Set the z-value to 0 if you are working in 2D (to ignore depth)
        mouseWorldPosition.z = 0;

        if (skill is PlayerSkill)
        {
            if (skill != null && (skill as PlayerSkill).shape == Shape.CIRCLE)
            {
                radius = skill.radius;
                Vector3 newScale = transform.localScale;

                newScale.y = radius;
                newScale.x = radius;

                transform.localScale = newScale;

                if ((skill as PlayerSkill).targetMode == TargetMode.FREE)
                {
                    if (Vector2.Distance(this.attackerObject.transform.position, mouseWorldPosition) < skill.range / 2f)
                        circlePosition = mouseWorldPosition;
                    else
                    {
                        Vector2 direction = mouseWorldPosition - this.attackerObject.transform.position;
                        direction = direction.normalized * (skill.range / 2);
                        circlePosition = (Vector2)this.attackerObject.transform.position + direction;
                    }
                }
                else if ((skill as PlayerSkill).targetMode == TargetMode.SELF)
                {
                    circlePosition = originObject.transform.position;
                }
                else if ((skill as PlayerSkill).targetMode == TargetMode.SELECT)
                {
                    circlePosition = originObject.transform.position;
                }

                if (!freezeAoe)
                {
                    transform.position = circlePosition;
                }
                else
                {
                    transform.position = frozenPosition;
                }
            }
        }
        else
        {
            Vector3 newScale = transform.localScale;

            newScale.y = radius;
            newScale.x = radius;

            transform.localScale = newScale;

            transform.position = circlePosition;
        }
    }

    // Make a connected function between both initialization functions that prevent copy/pasted logic
    public override void InitializeEnemyAoe(GameObject attackerObject, MechanicAttack attack, SkillInformation info)
    {
        base.aoeData = new AoeData();

        if (skill == null && attack.aoeShape != Shape.CIRCLE)
            return;

        this.mechanicAttack = attack;
        this.attackerObject = attackerObject;
        this.radius = attack.size;

        // May need a new parameter for EnemyTargetInformation to let the aoe know what to do

        if (info.objectOrigin != null)
        {
            this.circlePosition = info.objectOrigin.transform.position;
        }
        else
        {
            this.circlePosition = attack.customOrigin;
        }

        Color newColor = Color.red;
        SpriteRenderer circleRenderer = circleAoe.GetComponent<SpriteRenderer>();
        circleRenderer.color = newColor;
        Color colorWithAlpha = circleRenderer.color;
        colorWithAlpha.a = 0.5f;
        circleRenderer.color = colorWithAlpha;
        circleAoe.SetActive(true);
    }

    public override void InitializeAoe(GameObject originObject, GameObject attackerObject, BaseSkill skill = null)
    {
        base.aoeData = new AoeData();

        if (skill is PlayerSkill)
        {
            if (skill == null || (skill as PlayerSkill).shape != Shape.CIRCLE)
                return;
        }


        this.skill = skill;
        this.mechanicAttack = null;
        this.attackerObject = attackerObject;

        Color newColor = Color.red;
        SpriteRenderer circleRenderer = circleAoe.GetComponent<SpriteRenderer>();
        circleRenderer.color = newColor;
        Color colorWithAlpha = circleRenderer.color;
        colorWithAlpha.a = 0.5f;
        circleRenderer.color = colorWithAlpha;

        this.originObject = originObject;

        // Get the mouse position in screen coordinates (pixels)
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convert the screen position to world space (2D)
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // Set the z-value to 0 if you are working in 2D (to ignore depth)
        mouseWorldPosition.z = 0;

        if (Vector2.Distance(this.attackerObject.transform.position, mouseWorldPosition) < skill.range)
            circlePosition = mouseWorldPosition;

        if (!freezeAoe)
        {
            transform.position = circlePosition;
        }
        else
        {
            transform.position = frozenPosition;
        }


        circleAoe.SetActive(true);
    }

    public override Vector2 FrozenInfo()
    {
        return this.frozenPosition;
    }

    public override void FreezeAoe()
    {
        if (skill is PlayerSkill)
        {
            if (skill != null && (skill as PlayerSkill).shape == Shape.CIRCLE)
            {
                base.FreezeAoe();
                frozenPosition = transform.position;
            }
        }

    }
}
