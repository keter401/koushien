// §ìÒ ûM“c –F’C

using UnityEngine;

[System.Serializable]
public class GameState
{
    [SerializeField] private string name = "None Name";

    public GameState(string name = "None Name")
    {
        this.name = name;
    }

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }

    public virtual void Update()
    {
    }

    public virtual void LateUpdate()
    {
    }
}
