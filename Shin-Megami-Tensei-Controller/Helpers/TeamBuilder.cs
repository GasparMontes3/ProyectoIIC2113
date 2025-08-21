using System.Text.Json;

namespace Shin_Megami_Tensei.Helpers;

public class TeamBuilder
{
    public (string, List<Unit>, string, List<Unit>) BuildTeams(string teamsLocation)
    {
        Console.WriteLine("Cargando units desde json...");
        List<Unit> unitList = LoadUnits();
        (string, List<Unit>, string, List<Unit>) teamsTuple = CreateTeamsTuple(teamsLocation, unitList);
        if (ValidateTeams(teamsTuple))
        {
            ReadTeamsOnConsole(teamsTuple); //No necesario hasta que parte la batalla.
            return teamsTuple;
        }
        else
            return ReturnEmptyTuple();
    }
    
    public List<Unit> LoadUnits()
    {
        List<Unit> samurai = DeserializeUnits("samurai");
        List<Unit> monsters = DeserializeUnits("monsters");
        samurai.AddRange(monsters);
        return samurai;
    }

    public List<Unit> DeserializeUnits(string unitType)
    {
        List<Unit> units;
        string json = ReadFile(unitType);
        if (unitType == "monster")
        {
            units = JsonSerializer.Deserialize<List<Monster>>(json).Cast<Unit>().ToList();
        }
        else
        {
            units = JsonSerializer.Deserialize<List<Samurai>>(json).Cast<Unit>().ToList();
        }
        return units;
    }

    public string ReadFile(string unitType)
    {
        string path = GetAbsolutePath(unitType);
        if (!Directory.Exists(path))
        {
            Console.WriteLine($"Entrando al path: {path}");
            return File.ReadAllText(path);
        }
        return "Error";
    }
    
    //IA
    public string GetAbsolutePath(string unitType)
    {
        // Get the base directory of the application
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Navigate up from the bin/Debug folder to the solution root
        string solutionRoot = Path.GetFullPath(Path.Combine(baseDirectory, @"..","..","..",".."));

        // Combine with the data folder and file name
        string absolutePath = Path.Combine(solutionRoot, "data", $"{unitType}.json");

        return absolutePath;
    }

    public (string, List<Unit>, string, List<Unit>) CreateTeamsTuple(string teamsLocation, List<Unit> unitList)
    {
        Console.WriteLine($"Ruta recibida: {teamsLocation}");
        string finalTeamsLocation = AddBackstepsToTeamsLocation(teamsLocation);
        Console.WriteLine(finalTeamsLocation);
        string[] lines = File.ReadAllLines(finalTeamsLocation);
        return AnalyzeTeamTxt(lines, unitList);
    }

    public string AddBackstepsToTeamsLocation(string fixedTeamsLocation)
    {
        string finalTeamsLocation = Path.GetFullPath(Path.Combine("..","..",/*"..","..",*/ fixedTeamsLocation));
        return finalTeamsLocation;
    }

    public (string, List<Unit>, string, List<Unit>) AnalyzeTeamTxt(string[] lines,  List<Unit> unitList)
    {
        string team1Name = lines[0];
        int line = 1;
        List<Unit> team1 = new List<Unit>();
        while (lines[line] != "Player 2 Team")
        {
            team1.Add(CreateUnit(lines[line], unitList));
            line++;
        }
        string team2Name = lines[line];
        line++;
        List<Unit> team2  = new List<Unit>();
        while (line < lines.Length)
        {
            team2.Add(CreateUnit(lines[line], unitList));
            line++;
        }
        return (team1Name, team1, team2Name, team2);
    }
    
    public Unit CreateUnit(string line,  List<Unit> unitList)
    {
        Unit unit;
        string[] words = line.Split(' ');
        if (words[0] == "[Samurai]")
        {
            string name = GetMiddlePart(line);
            unit = unitList.FirstOrDefault(character => character.Name == name);
            AddSkillsToSamurai(line, unit);
        }
        else
        {
            unit = unitList.FirstOrDefault(character => character.Name == line);}
        return unit;
    }

    public void AddSkillsToSamurai(string line, Unit unit)
    {
        List<string> skills = ExtractSkillsFromLine(line);
        unit.Skills.AddRange(skills);
    }

    //función con ayuda de IA
    public List<string> ExtractSkillsFromLine(string line)
    {
        int startIndex = line.IndexOf("(");
        int endIndex = line.IndexOf(")");
        string skillsList = line.Substring(startIndex + 1, endIndex - startIndex - 1);
        List<string> skills = skillsList.Split(',').Select(skill => skill.Trim()).ToList();
        return skills;
    }
    
    //función con ayuda de IA
    public string GetMiddlePart(string input)
    {
        int startIndex = input.IndexOf("] ") + 2;
        string remainingString = input.Substring(startIndex);
        int endIndex = remainingString.IndexOf(" (");
    
        if (endIndex != -1)
        {
            return remainingString.Substring(0, endIndex).Trim();
        }
        return remainingString.Trim();
    }

    public bool ValidateTeams((string team1Name, List<Unit> team1, string team2Name, List<Unit> team2) teamsTuple)
    {
        if (AnalyzeTeamConditions(teamsTuple.team1))
        {
            if (AnalyzeTeamConditions(teamsTuple.team2))
            {
                return true;
            }
        }
        return false;
    }

    public bool AnalyzeTeamConditions(List<Unit> team)
    {
        if (CountDifferentUnits(team) < 8)
        {
            if (CountSamurai(team) == 1)
            {
                return true;
            }
        }
        return false;
    }
    
    public int CountDifferentUnits(List<Unit> team)
    {
        int count = 0;
        foreach (Unit character in team)
        {
            if (CheckIfCharacterIsRepeated(character, team))
            {
                return 100;
            }
            count++;
        }
        return count;
    }

    public bool CheckIfCharacterIsRepeated(Unit character, List<Unit> team)
    {
        int count = 0;
        for (int i = 0; i < team.Count; i++)
        {
            if (character.Name == team[i].Name)
            {
                count++;
            }
        }
        return ReturnTrueIfOverOne(count);
    }

    public bool ReturnTrueIfOverOne(int n)
    {
        if (n > 1)
        {
            return true;
        }
        return false;
    }


    public int CountSamurai(List<Unit> team)
    {
        int samuraiCount = 0;
        for (int i = 0; i < team.Count; i++)
        {
            if (team[i].UnitType == "samurai")
            {
                samuraiCount++;
                if (CountDifferentSamuraiAbilities(team[i]) > 8)
                {
                    return 100;
                }
            }
        }
        return samuraiCount;
    }

    public int CountDifferentSamuraiAbilities(Unit samurai)
    {
        int count = 0;
        for (int i = 0; i < samurai.Skills.Count; i++)
        {
            if (CheckIfAbilityIsRepeated(samurai.Skills[i], samurai))
            {
                return 100;
            }
            count++;
        }
        return count;
    }

    public bool CheckIfAbilityIsRepeated(string skill, Unit samurai)
    {
        int count = 0;
        for (int i = 0; i < samurai.Skills.Count; i++)
        {
            if (skill == samurai.Skills[i])
            {
                count++;
            }
        }
        return ReturnTrueIfOverOne(count);
    }

    public void ReadTeamsOnConsole((string, List<Unit>, string, List<Unit>) teamsTuple)
    {
        //leer equipos (view) No se debe hacer hasta la batalla
    }

    public (string, List<Unit>, string, List<Unit>) ReturnEmptyTuple()
    {
        return ("", new List<Unit>(), "", new List<Unit>());
    }
}