using UnityEngine;

public abstract class Ability
{
    public abstract void OnStart();
    public abstract void OnDeath();
    public abstract void OnMove();
    public abstract void OnAttack();
}