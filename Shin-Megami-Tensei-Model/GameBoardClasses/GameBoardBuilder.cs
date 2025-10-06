namespace Shin_Megami_Tensei_Model;

public class GameBoardBuilder
{
    private ViewPrinter view;
    private UnitListLoader unitListLoader;
    private TeamBuilder teamBuilder;
    private TeamValidator teamValidator;
    
    public GameBoardBuilder()
    {
        teamValidator = new TeamValidator();
        unitListLoader = new UnitListLoader();
        teamBuilder = new TeamBuilder();
    }
    
    public (string, List<Unit>, string, List<Unit>) BuildTeams(ViewPrinter view, string[] teamsArray)
    {
        this.view = view;
        List<Unit> unitList = unitListLoader.LoadUnits(view);
        (string, List<Unit>, string, List<Unit>) teamsTuple = teamBuilder.CreateTeamsTuple((view, unitListLoader),(teamsArray, unitList));
        Console.WriteLine($"GameBoard: {teamsTuple.Item1}, {teamsTuple.Item2[0].name}, {teamsTuple.Item3}, {teamsTuple.Item4[0].name}");
        if (teamValidator.IsTeamsValidated(teamsTuple))
        {
            return teamsTuple;
        }
        return teamBuilder.ReturnEmptyBoardTuple();
    }
}