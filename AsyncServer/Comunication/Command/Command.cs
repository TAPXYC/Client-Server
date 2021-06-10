using System;


class Command
{
    public string Answer
    {
        get;
        private set;
    }

    private Action Action;


    public Command(string answer, Action action)
    {
        Answer = answer;
        Action = action;
    }


    public void Execute() => Action?.Invoke();
}
