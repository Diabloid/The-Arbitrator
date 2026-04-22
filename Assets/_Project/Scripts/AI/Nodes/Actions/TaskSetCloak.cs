using BehaviorTree;
using UnityEngine;

public class TaskSetCloak : Node
{
    private CloakController _cloak;
    private bool _enableCloak;

    public TaskSetCloak(CloakController cloak, bool enableCloak)
    {
        _cloak = cloak;
        _enableCloak = enableCloak;
    }

    public override NodeState Evaluate()
    {
        if (_cloak != null)
        {
            _cloak.SetCloak(_enableCloak);
        }

        return NodeState.SUCCESS;
    }
}