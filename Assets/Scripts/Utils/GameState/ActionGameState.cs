// §ìÒ: ûM“c –F’C

using UnityEngine;

public class ActionGameState : GameState
{
    public ActionGameState()
        : base("ActionGameState")
    {
    }


    public override void Enter()
    {
        base.Enter();

        GManager.instance.StartTimer(GManager.instance.maxActionTime);
    }
}
