using UnityEngine;

public abstract class Ability
{
    public string Name;
    public string Description;

    protected Creature owner;

    protected Ability(Creature owner)
    {
        this.owner = owner;
    }

    public void SetOwner(Creature owner)
    {
        this.owner = owner;
    }

    public virtual void OnStart() { }
    public virtual void OnDeath() { }
    public virtual void OnMove() { }
    public virtual void OnAttack() { }
}
