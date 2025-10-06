using System.Text.Json;

namespace Shin_Megami_Tensei_Model;

public class UnitListLoader
{
    public ViewPrinter view;
    public Dictionary<string, Skill> _allSkills;
    
    public List<Unit> LoadUnits(ViewPrinter view)
    {
        this.view = view;
        LoadAllSkills();
        List<Unit> samurai = CreateUnitList("samurai");
        List<Unit> monsters = CreateUnitList("monsters");
        samurai.AddRange(monsters);
        return samurai;
    }

    private List<Unit> CreateUnitList(string unitType)
    {
        List<Unit> units = DeserializeUnits(unitType);
        AddSkillsToUnits(units);
        AddMissingAttributes(units);
        return units;
    }

    private List<Unit> DeserializeUnits(string unitType)
    {
        List<Unit> units;
        string json = ReadUnitsFile(unitType);
        if (unitType == "monsters")
        {
            units = JsonSerializer.Deserialize<List<Monster>>(json).Cast<Unit>().ToList();
        }
        else
        {
            units = JsonSerializer.Deserialize<List<Samurai>>(json).Cast<Unit>().ToList();
        }
        return units;
    }
    
    private void LoadAllSkills()
    {
        string skillsJson = ReadUnitsFile("skills");
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

    private void AddMissingAttributes(List<Unit> units)
    {
        foreach (var unit in units)
        {
            unit.SetView(view);
            AddMaxHPAndMaxMPToUnits(unit);
        }
    }

    private void AddMaxHPAndMaxMPToUnits(Unit unit)
    {
        unit.stats.MaxHP = unit.stats.HP;
        unit.stats.MaxMP = unit.stats.MP;
    }
    
    private string ReadUnitsFile(string unitType)
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", $"{unitType}.json");
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        return $"$Error: Ruta '{path}' no encontrado";
    }

    public Dictionary<string, Skill> ReturnAllSkills()
    {
        return _allSkills;
    }
}