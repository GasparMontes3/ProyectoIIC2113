using System.Text.Json;

namespace Shin_Megami_Tensei_Model;

public class TeamBuilder
{
    private ViewPrinter view;
    private UnitListLoader unitListLoader;
    
    public (string, List<Unit>, string, List<Unit>) CreateTeamsTuple((ViewPrinter view, UnitListLoader unitListLoader) attributes, (string[] teamsArray, List<Unit> unitList) teamInfo)
    {
        DefineAttributes(attributes.view, attributes.unitListLoader);
        if (IsTeamsArrayEmpty(teamInfo.teamsArray))
        {
            return ReturnEmptyBoardTuple();
        }
        return AnalyzeTeamTxt(teamInfo.teamsArray, teamInfo.unitList);
    }

    private void DefineAttributes(ViewPrinter view, UnitListLoader unitListLoader)
    {
        this.view = view;
        this.unitListLoader = unitListLoader;
    }

    public bool IsTeamsArrayEmpty(string[] teamsArray)
    {
        if (teamsArray == null || teamsArray.Length == 0)
        {
            return true;
        }
        return false;
    }
    
    public (string, List<Unit>, string, List<Unit>) AnalyzeTeamTxt(string[] lines,  List<Unit> unitList)
    {
        (List<Unit> team1, int lastLineIndex) team1Result = CreateTeamFromLines(lines, unitList, 1);
        (List<Unit> team2, int) team2Result = CreateTeamFromLines(lines, unitList, team1Result.lastLineIndex + 1);
        string team1Name = GenerateTeamName(team1Result.Item1, 1);
        string team2Name = GenerateTeamName(team2Result.Item1, 2);
        return (team1Name, team1Result.Item1, team2Name, team2Result.Item1);
    }
    
    private (List<Unit> team, int lastLineIndex) CreateTeamFromLines(string[] lines, List<Unit> unitList, int startIndex)
    {
        var team = new List<Unit>();
        int i = startIndex;
        for (; i < lines.Length && !lines[i].StartsWith("Player"); i++)
        {
            Unit newUnit = CreateUnit(lines[i], unitList);
            team.Add(newUnit);
        }
        return (team, i);
    }
    
    private string GenerateTeamName(List<Unit> team, int playerNumber)
    {
        return $"{team[0].name} (J{playerNumber})";
    }
    
    public Unit CreateUnit(string line,  List<Unit> unitList)
    {
        string name = GetNameFromLine(line);
        Unit templateUnit = unitList.FirstOrDefault(character => character.name == name);
        return CreateCloneFromTemplate(templateUnit, line);
    }

    private string GetNameFromLine(string line)
    {
        string name;
        if (line[0] == '[') { name = GetMiddlePart(line); }
        else { name = line; }
        return name;
    }

    private Unit CreateCloneFromTemplate(Unit templateUnit, string line)
    {
        Unit clonedUnit = CloneUnit(templateUnit);
        clonedUnit.view = view;
        if (line[0] == '[' && HasSamuraiSkills(line))
        {
            AddSkillsToSamurai(line, clonedUnit);
        }
        return clonedUnit;
    }
    
    private Unit CloneUnit(Unit unitToClone)
    {
        var options = new JsonSerializerOptions();
        string json = JsonSerializer.Serialize(unitToClone, unitToClone.GetType(), options);
        return (Unit)JsonSerializer.Deserialize(json, unitToClone.GetType(), options);
    }

    public bool HasSamuraiSkills(string line)
    {
        if (line.Contains("("))
        {
            return true;
        }
        return false;
    }

    public void AddSkillsToSamurai(string line, Unit unit)
    {
        List<string> skills = ExtractSkillsFromLine(line);
        foreach (var skillName in skills) { AddIndividualSkillToListOfSkills(skillName, unit); }
    }

    public void AddIndividualSkillToListOfSkills(string skillName, Unit unit)
    {
        Dictionary<string, Skill> _allSkills = unitListLoader.ReturnAllSkills();
        if (_allSkills.TryGetValue(skillName, out Skill fullSkill)) { unit.skills.Add(fullSkill); }
        else { Console.WriteLine($"Advertencia: La skill '{skillName}' del archivo de equipos no fue encontrada."); }
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
    
    public (string, List<Unit>, string, List<Unit>) ReturnEmptyBoardTuple()
    {
        return ("", new List<Unit>(), "", new List<Unit>());
    }
}