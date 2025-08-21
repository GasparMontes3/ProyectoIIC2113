using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Helpers;
using System.Collections.Generic;

namespace Shin_Megami_Tensei;

public class Game
{
    private string teamsLocation;
    private View view;
    private TeamBuilder teamBuilder;
    public Game(View view, string teamsFolder)
    {
        teamsLocation =  teamsFolder;
        view = view;
        teamBuilder = new TeamBuilder();
    }
    
    public void Play()
    {
        Console.WriteLine($"Empezando... \nUsando la ruta {teamsLocation}");
        (string, List<Unit>, string, List<Unit>) teamList = teamBuilder.BuildTeams(teamsLocation);
        if (teamList.Item1 != "")
        {
            throw new NotImplementedException();
        }
        else view.WriteLine("Archivo de equipos no válido");
    }
}