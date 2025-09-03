namespace Shin_Megami_Tensei_Models;

public class Samurai : Unit
{
    public Samurai(string name, Stats stats, Affinity affinity, List<Skill> skills) : base(name, stats, affinity,
        skills)
    {
    }

    public Samurai()
    {
    }

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
        view.WriteLine($"Seleccione una acción para {name}\n1: Atacar\n2: Disparar\n3: Usar Habilidad\n4: Invocar\n5: Pasar Turno\n6: Rendirse");
        int input = fileGetter.ReturnIntFromUserInput(view);
        if (fileGetter.CheckIfInputIsValid(input, 1, 6))
        {
            view.WriteLine("----------------------------------------");
            return input;
        }

        view.WriteLine("Error: Input inválido");
        return -1;
    }
    
    public List<int> ActionDelegator(int input, Player opponent)
    {
        if (actionMap.TryGetValue(input, out var actionToExecute))
        {
            return actionToExecute(opponent);
        }
        view.WriteLine("Input no corresponde a una acción válida.");
        return [1,0]; //Asume que se pierde el turno (no se cual sería la respuesta esperada)
    }

protected override Dictionary<int, Func<Player, List<int>>> InitializeActionMap()
    {
        return new Dictionary<int, Func<Player, List<int>>>
        {
            { 1, Attack },
            { 2, Shoot },
            { 3, UseSkill },
            { 4, Summon },
            { 5, PassTurn },
            { 6, Surrender }
        };
    }

    //Funciones de Shoot
    private List<int> Shoot(Player opponent)
    {
        view.WriteLine($"Seleccione un objetivo para {name}");
        (int, Unit) attackData = PrepareAttackData(opponent, (stats.Skl, 80));
        if (attackData.Item2.name == "Mr. Cancel") { return [70,0]; }
        return DelegateDamageData((attackData.Item1, attackData.Item2, "Gun"));
    }

    //Funciones de Surrender
    private List<int> Surrender(Player opponent)
    {
        var result = new List<int> { -100, -100 };
        return result;
    }
}