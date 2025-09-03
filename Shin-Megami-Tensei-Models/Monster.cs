namespace Shin_Megami_Tensei_Models;

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
        view.WriteLine($"Seleccione una acción para {name}\n1: Atacar\n2: Usar Habilidad\n3: Invocar\n4: Pasar Turno");
        int input = fileGetter.ReturnIntFromUserInput(view);
        if (fileGetter.CheckIfInputIsValid(input, 1, 4))
        {
            view.WriteLine("----------------------------------------");
            return input;
        }
        view.WriteLine("Error: Input inválido");
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