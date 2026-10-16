using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerTransitionRule : ScriptableObject, IPlayerTransitionRule
{
    public virtual IPlayerInputAction Action => null;

    public virtual bool CanApply(PlayerBlackBoard board)
    {
        return true;
    }

    public abstract Type GetNextState(PlayerBlackBoard board);
}