namespace Shin_Megami_Tensei_Model;

public class Monster : Unit
{
    public Monster(string name, Stats stats, Affinity affinity, List<Skill> skills) : base(name, stats, affinity,
        skills) { }
    
    public Monster() { }
    
    public override List<int> TakeAction(Player opponent)
    {
        List<int> turnList;
        do
        {
            int input = PrintActionSelection();
            turnList = ActionDelegator(input, opponent);
        }
        while (turnList == null || turnList[0] == 70); 
        return turnList;
    }

    public override int PrintActionSelection()
    {
        view.PrintMonsterActionSelection(name);
        int input = view.ReturnIntFromUserInput();
        if (view.IsInputValid(input, 1, 4))
        {
            view.PrintFortyHyphen();
            return input;
        }
        view.PrintInvalidInput();
        return -1;
    }
    
    protected override Dictionary<int, Func<Player, List<int>>> InitializeActionMap()
    {
        return new Dictionary<int, Func<Player, List<int>>>
        {
            { 1, Attack },
            { 2, UseSkill },
            { 3, Summon },
            { 4, PassTurn }
        };
    }
    
    public List<int> ActionDelegator(int input, Player opponent)
    {
        return actionMap[input].Invoke(opponent);
    }
}