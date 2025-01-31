using UnityEngine;

public abstract class BaseAoe : MonoBehaviour
{
    public BaseSkill skill;
    public MechanicAttack mechanicAttack;
    public AoeData aoeData;
    public bool enemy = false;
    public int arenaAoeIndex = -1;

    public GameObject originObject;
    public GameObject attackerObject;

    public bool freezeAoe = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        aoeData = new AoeData();
    }

    protected virtual void Update() { }

    public abstract void InitializeAoe(GameObject originObject, GameObject attackerObject, BaseSkill skill = null);
    public abstract void InitializeEnemyAoe(GameObject attackerObject, MechanicAttack attack, SkillInformation info);
    
    public virtual void FreezeAoe()
    {
        freezeAoe = true;
    }

    public abstract Vector2 FrozenInfo();

    public void HandleAddTarget(GameObject target)
    {
        aoeData.AddTarget(target);
    }

    public void HandleRemoveTarget(GameObject target)
    {
        aoeData.RemoveTarget(target);
    }
}
