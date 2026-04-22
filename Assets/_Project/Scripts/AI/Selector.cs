using System.Collections.Generic;

namespace BehaviorTree
{
    // Вузол-селектор: виконує дітей по черзі, поки один не поверне SUCCESS або RUNNING
    public class Selector : Node
    {
        public Selector() : base() { } // Конструктор без дітей
        public Selector(List<Node> children) : base(children) { } // Конструктор з дітьми

        public override NodeState Evaluate()
        {
            foreach (Node node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        continue;
                    case NodeState.SUCCESS:
                        state = NodeState.SUCCESS;
                        return state;
                    case NodeState.RUNNING:
                        state = NodeState.RUNNING;
                        return state;
                    default:
                        continue;
                }
            }

            state = NodeState.FAILURE;
            return state;
        }
    }
}