using System.Collections.Generic;

namespace BehaviorTree
{
    // Вузол-послідовність: виконує дітей по черзі, поки один не поверне FAILURE або RUNNING
    public class Sequence : Node
    {
        public Sequence() : base() { } // Конструктор без дітей
        public Sequence(List<Node> children) : base(children) { } // Конструктор з дітьми

        public override NodeState Evaluate()
        {
            bool anyChildRunning = false; // Прапорець для відстеження, чи є діти в стані RUNNING

            foreach (Node node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        state = NodeState.FAILURE;
                        return state;
                    case NodeState.SUCCESS:
                        continue;
                    case NodeState.RUNNING:
                        anyChildRunning = true;
                        continue;
                    default:
                        state = NodeState.SUCCESS;
                        return state;
                }
            }

            state = anyChildRunning ? NodeState.RUNNING : NodeState.SUCCESS;
            return state;
        }
    }
}