using UnityEngine;

public class DerivedCircle : BaseAoe
{
    public float radius = 2f;
    public float height = -1f;
    public float width = -1f;
    public GameObject circleAoe;
    public Vector2 circlePosition;


    public Vector2 frozenPosition;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
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

            if (this.mechanicAttack != null && (this.mechanicAttack.nonUniformDimensions || this.mechanicAttack.raidWide))
            {
                newScale.y = height;
                newScale.x = width;
            }
            else
            {
                newScale.y = radius;
                newScale.x = radius;
            }


            transform.localScale = newScale;

            if (this.originObject != null)
                circlePosition = this.originObject.transform.position;

            transform.position = circlePosition;
        }
    }

    // Make a connected function between both initialization functions that prevent copy/pasted logic
    public override void InitializeEnemyAoe(GameObject attackerObject, EnemyMechanic mechanic, MechanicAttack attack, SkillInformation info)
    {
        base.aoeData = new AoeData();

        if (skill == null && (attack.aoeShape != Shape.CIRCLE && attack.aoeShape != Shape.DONUT && attack.aoeShape != Shape.BOX))
        {
            Debug.Log(attack.aoeShape);
            return;
        }

        this.mechanicAttack = attack;
        this.mechanic = mechanic;
        this.attackerObject = attackerObject;

        if (attack.nonUniformDimensions == false)
            this.radius = attack.size;
        else
        {
            this.height = attack.dimensions.y;
            this.width = attack.dimensions.x;
        }

        // May need a new parameter for EnemyTargetInformation to let the aoe know what to do

        if (!attack.raidWide)
        {
            if (info.objectOrigin != null)
            {
                this.originObject = info.objectOrigin;
            }
            else
            {
                this.circlePosition = attack.customOrigin;
            }

            if (attack.hasArenaPositioning)
            {
                SetAoePosition(attack.aoePositionInformation);
            }
        }
        else
        {
            this.circlePosition = ExcentraGame.battleManager.arena.GetCenter();
            this.width = ExcentraGame.battleManager.arena.width / 2;
            this.height = ExcentraGame.battleManager.arena.height / 2;
        }


        Color newColor = Color.red;

        if (this.mechanicAttack != null && this.mechanicAttack.customColor)
            newColor = this.mechanicAttack.aoeColor;

        SpriteRenderer circleRenderer = circleAoe.GetComponent<SpriteRenderer>();
        circleRenderer.color = newColor;
        Color colorWithAlpha = circleRenderer.color;
        colorWithAlpha.a = 0.2f;
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
        colorWithAlpha.a = 0.2f;
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

    public override void SetAoePosition(MechanicAoePositionHelper positionHelper)
    {
        BattleArena arena = ExcentraGame.battleManager.arena;

        switch (positionHelper.positionType)
        {
            case ArenaPositionType.CENTER:
                circlePosition = arena.GetCenter();
                break;
            case ArenaPositionType.LEFT_HALF:
                circlePosition = arena.GetLeftCenter();
                break;
            case ArenaPositionType.RIGHT_HALF:
                circlePosition = arena.GetRightCenter();
                break;
            case ArenaPositionType.TOP_HALF:
                circlePosition = arena.GetTopCenter();
                break;
            case ArenaPositionType.BOTTOM_HALF:
                circlePosition = arena.GetBottomCenter();
                break;
        }

        if (positionHelper.isHalfSize)
        {
            float halfSize = arena.GetHalfSize(positionHelper.positionType);

            switch (positionHelper.positionType)
            {
                case ArenaPositionType.LEFT_HALF:
                    this.width = halfSize;
                    break;
                case ArenaPositionType.RIGHT_HALF:
                    this.width = halfSize;
                    break;
                case ArenaPositionType.TOP_HALF:
                    this.height = halfSize;
                    break;
                case ArenaPositionType.BOTTOM_HALF:
                    this.height = halfSize;
                    break;
            }
        }

        // Adjust scale to account for padding
        this.width -= positionHelper.leftPadding + positionHelper.rightPadding;
        this.height -= positionHelper.bottomPadding + positionHelper.topPadding;

        // Adjust position to account for padding
        circlePosition.x += (positionHelper.leftPadding - positionHelper.rightPadding) / 2f;
        circlePosition.y += (positionHelper.bottomPadding - positionHelper.topPadding) / 2f;

        circlePosition += positionHelper.offset;
    }

    public override void ChangeTarget(GameObject target)
    {
        if (target != null)
        {
            this.originObject = target;
            particleEmitter.GetComponent<ParticleLine>().target = target;
            particleEmitter.GetComponent<ParticleLine>().SetContents(attackerObject, target);
            particleEmitter.GetComponent<ParticleLine>().ModifyParticleOpacity();
        }
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
