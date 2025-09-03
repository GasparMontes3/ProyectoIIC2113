using System.Text.Json;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei_Models;

public class TeamBuilder
{
    private Dictionary<string, Skill> _allSkills;
    private View view;
    
    public (string, List<Unit>, string, List<Unit>) BuildTeams(string[] teamsArray, View view)
    {
        this.view = view;
        List<Unit> unitList = LoadUnits(view);
        (string, List<Unit>, string, List<Unit>) teamsTuple = CreateTeamsTuple(teamsArray, unitList);
        if (ValidateTeams(teamsTuple))
        {
            return teamsTuple;
        }
        return ReturnEmptyTuple();
    }
    
    public List<Unit> LoadUnits(View view)
    {
        LoadAllSkills();
        List<Unit> samurai = DeserializeUnits("samurai", view);
        List<Unit> monsters = DeserializeUnits("monsters", view);
        samurai.AddRange(monsters);
        return samurai;
    }

    public List<Unit> DeserializeUnits(string unitType, View view)
    {
        List<Unit> units;
        string json = ReadFile(unitType);
        if (unitType == "monsters")
        {
            units = JsonSerializer.Deserialize<List<Monster>>(json).Cast<Unit>().ToList();
        }
        else
        {
            units = JsonSerializer.Deserialize<List<Samurai>>(json).Cast<Unit>().ToList();
        }
        AddSkillsToUnits(units);
        AddMissingAttributes(units, view);
        return units;
    }
    
    private void LoadAllSkills()
    {
        string skillsJson = ReadFile("skills");
        List<Skill> skillList = JsonSerializer.Deserialize<List<Skill>>(skillsJson);
        _allSkills = skillList.ToDictionary(skill => skill.name, skill => skill);
    }
    
    private void AddSkillsToUnits(List<Unit> units)
    {
        foreach (var unit in units)
        {
            CreateSkillsAsObjects(unit);
        }
    }

    private void CreateSkillsAsObjects(Unit unit)
    {
        unit.skills = new List<Skill>();
        if (unit.skillNames != null)
        {
            TraverseSkillList(unit);
        }
    }

    private void TraverseSkillList(Unit unit)
    {
        foreach (var skillName in unit.skillNames)
        {
            CheckIfSkillExists(unit, skillName);
        }
    }

    private void CheckIfSkillExists(Unit unit, string skillName)
    {
        if (_allSkills.TryGetValue(skillName, out Skill fullSkill))
        {
            unit.skills.Add(fullSkill);
        }
    }

    public void AddMissingAttributes(List<Unit> units, View view)
    {
        foreach (var unit in units)
        {
            AddViewToUnits(unit, view);
            AddMaxHPAndMaxMPToUnits(unit);
        }
    }

    public void AddViewToUnits(Unit unit, View view)
    {
        unit.view = view; 
    }

    public void AddMaxHPAndMaxMPToUnits(Unit unit)
    {
        unit.stats.MaxHP = unit.stats.HP;
        unit.stats.MaxMP = unit.stats.MP;
    }

    public string ReadFile(string unitType)
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", $"{unitType}.json");
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        return $"$Error: Ruta '{path}' no encontrado";
    }

    public (string, List<Unit>, string, List<Unit>) CreateTeamsTuple(string[] teamsArray, List<Unit> unitList)
    {
        if (CheckIfTeamsArrayIsEmpty(teamsArray))
        {
            return ReturnEmptyTuple();
        }
        return AnalyzeTeamTxt(teamsArray, unitList);
    }

    public bool CheckIfTeamsArrayIsEmpty(string[] teamsArray)
    {
        if (teamsArray == null || teamsArray.Length == 0)
        {
            return true;
        }
        return false;
    }

    /*
    public (string, List<Unit>, string, List<Unit>) AnalyzeTeamTxt(string[] lines,  List<Unit> unitList)
    {
        int line = 1;
        
        List<Unit> team1 = new List<Unit>();
        while (lines[line] != "Player 2 Team")
        {
            team1.Add(CreateUnit(lines[line], unitList));
            line++;
        }
        string team1Name = $"{team1[0].name} (J1)";
        
        line++;
        List<Unit> team2  = new List<Unit>();
        while (line < lines.Length)
        {
            team2.Add(CreateUnit(lines[line], unitList));
            line++;
        }
        string team2Name = $"{team2[0].name} (J2)";
        return (team1Name, team1, team2Name, team2);
    }
    */
    
    
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
        if (line[0] == '[' && SamuraiHasSkills(line))
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

    public bool SamuraiHasSkills(string line)
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
        if (CountUnitsAmount(team) <= 8)
        {
            return CheckSamuraiConditions(team);
        }
        return false;
    }
    
    public int CountUnitsAmount(List<Unit> team)
    {
        int count = 0;
        foreach (Unit character in team)
        {
            if (CheckIfCharacterIsRepeated(character, team)) { return 100; }
            count++;
        }
        return count;
    }

    public bool CheckIfCharacterIsRepeated(Unit character, List<Unit> team)
    {
        int count = 0;
        for (int i = 0; i < team.Count; i++)
        {
            if (character.name == team[i].name)
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

    public bool CheckSamuraiConditions(List<Unit> team)
    {
        if (CountSamuraiAmount(team) == 1) { return true; }
        return false;
    }
    
    public int CountSamuraiAmount(List<Unit> team)
    {
        int samuraiCount = 0;
        for (int i = 0; i < team.Count; i++)
        {
            if (team[i] is Samurai)
            {
                samuraiCount++;
                if (CountDifferentSamuraiAbilities(team[i]) > 8) { return 100; }
            }
        }
        return samuraiCount;
    }

    public int CountDifferentSamuraiAbilities(Unit samurai)
    {
        int count = 0;
        for (int i = 0; i < samurai.skills.Count; i++)
        {
            if (CheckIfAbilityIsRepeated(samurai.skills[i].name, samurai))
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
        for (int i = 0; i < samurai.skills.Count; i++)
        {
            if (skill == samurai.skills[i].name)
            {
                count++;
            }
        }
        return ReturnTrueIfOverOne(count);
    }
    
    public (string, List<Unit>, string, List<Unit>) ReturnEmptyTuple()
    {
        return ("", new List<Unit>(), "", new List<Unit>());
    }
}