using Shin_Megami_Tensei_View;


namespace Shin_Megami_Tensei_Models;

public class Battle
{
    private Player _player1;
    private Player _player2;
    private View view;
    
    public Battle(View view, (string, List<Unit>, string, List<Unit>) gameBoard)
    {
        this.view = view;
        _player1 = new Player(view, (gameBoard.Item1, gameBoard.Item2));
        _player2 = new Player(view, (gameBoard.Item3, gameBoard.Item4));
    }

    public void BeginBattle()
    {
        view.WriteLine("----------------------------------------");
        PlayerTurn(_player1, _player2);
    }

    public void PlayerTurn(Player player, Player opponent)
    {
        StartOfTurnLogic(player);
        while (player.IsPlayerTurn())
        {
            TurnLoopLogic(player, opponent);
            if (CheckIfGameOver(player) || CheckIfGameOver(opponent)) { return; }
        }
        PlayerTurn(opponent, player);
    }

    public void StartOfTurnLogic(Player player)
    {
        RondaDeTeamView(player.teamName);
        player.CalculateInitialTurns();
    }

    public void TurnLoopLogic(Player player, Player opponent)
    {
        PrintStartOfTurn(player);
        player.Actions(opponent);
    }

    public bool CheckIfGameOver(Player player)
    {
        for (int i = 0; i < player.activeUnits.Count; i++)
        {
            if (player.activeUnits[i].stats.HP > 0)
            {
                return false;
            }
        }
        return EndGame(player);
    }

    public bool EndGame(Player player)
    {
        if (player.teamName == _player1.teamName)
        {
            view.WriteLine($"Ganador: {_player2.teamName}");
        }
        else
        {
            view.WriteLine($"Ganador: {_player1.teamName}");
        }
        return true;
    }

    public void PrintStartOfTurn(Player player)
    {
        ListTeamsInView();
        player.ShowRemainingTurns();
        player.ShowTurnOrder();
    }

    public void RondaDeTeamView(string teamName)
    {
        view.WriteLine($"Ronda de {teamName}");
        view.WriteLine("----------------------------------------");
    }

    public void ListTeamsInView()
    {
        ListTeamStatusInView(_player1.teamName, _player1.activeUnits);
        ListTeamStatusInView(_player2.teamName, _player2.activeUnits);
        view.WriteLine("----------------------------------------");
    }

    public void ListTeamStatusInView(string teamName, List<Unit> teamUnitList)
    {
        view.WriteLine($"Equipo de {teamName}");
        ListTeamOnBoardInInitialOrder(teamName, teamUnitList);
    }

    public void ListTeamOnBoardInInitialOrder(string teamName, List<Unit> teamUnitList)
    {
        int samuraiIndex = teamUnitList.FindIndex(unit => unit is Samurai);
        
        var orderedForPrinting = teamUnitList.Skip(samuraiIndex).Concat(teamUnitList.Take(samuraiIndex));
        
        ListTeamOnView(orderedForPrinting);
    }
    
    public void ListTeamOnView(IEnumerable<Unit> orderedForPrinting)
    {
        int count = 0;
        foreach (var unit in orderedForPrinting)
        {
            if (unit.stats.HP > 0 || unit is Samurai)
            {
                view.WriteLine($"{IntToAlphabet(count)}-{unit.name} HP:{unit.stats.HP}/{unit.stats.MaxHP} MP:{unit.stats.MP}/{unit.stats.MaxMP}");
            }
            else
            {
                view.WriteLine($"{IntToAlphabet(count)}-");
            }
            count++;
        }
        while (count < 4)
        {
            view.WriteLine($"{IntToAlphabet(count)}-");
            count++;
        }
    }

    public char IntToAlphabet(int n)
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        if (n >= 0 && n < alphabet.Length)
        {
            return alphabet[n];
        }
        return '?';
    }
}