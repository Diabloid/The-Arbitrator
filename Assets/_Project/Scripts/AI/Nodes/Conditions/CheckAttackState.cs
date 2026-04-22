using BehaviorTree;

public class CheckAttackState : Node
{
    private EnemyCombat _combat;

    public CheckAttackState(EnemyCombat combat)
    {
        _combat = combat;
    }

    public override NodeState Evaluate()
    {
        // Якщо скелет зараз махає мечем
        if (_combat != null && _combat.isAttacking)
        {
            // Повертаємо RUNNING.
            return NodeState.RUNNING;
        }

        // Якщо не атакує - дерево думає далі як завжди
        return NodeState.FAILURE;
    }
}