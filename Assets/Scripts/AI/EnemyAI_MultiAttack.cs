using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_MultiAttack : AI_NavMesh_Base
{
    public enum AttackType
    {
        Melee,
        Ranged
    }

    public AttackType attackType { get; private set; } = AttackType.Melee;

    public void SetAttackState(AttackType inType)
    {
        attackType = inType;
        SetState(State.Attack);
    }

    protected override void EnterAttack()
    {
        base.EnterAttack();

        switch(attackType)
        {
            case AttackType.Melee:
                Debug.Log("Melee attack!");
                break;
            case AttackType.Ranged:
                Debug.Log("Ranged attack!");
                break;
        }

        SetStateTimer(previousState, 3f);
    }
}
