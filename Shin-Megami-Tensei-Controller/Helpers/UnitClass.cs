namespace Shin_Megami_Tensei.Helpers;

public abstract class Unit
{
    public string Name { get; set; }
    public Stats Stats { get; set; }
    public Affinity Affinity { get; set; }
    public List<string> Skills { get; set; }
    public string UnitType;
    
    public Unit(string name, Stats stats, Affinity affinity, List<string> skills, string unitType)
    {
        Name = name;
        Stats = stats;
        Affinity = affinity;
        Skills = skills;
        UnitType = unitType;
    }
    
    protected Unit() { }
}

public class Stats {
    public int HP { get; set; }
    public int MP { get; set; }
    public int Str { get; set; }
    public int Skl { get; set; }
    public int Mag { get; set; } 
    public int Spd { get; set; } 
    public int Lck { get; set; } 
}

public class Affinity
{
    public string Phys { get; set; }
    public string Gun { get; set; }
    public string Fire { get; set; }
    public string Ice { get; set; }
    public string Elec { get; set; }
    public string Force { get; set; }
    public string Light { get; set; }
    public string Dark { get; set; }
    public string Bind { get; set; }
    public string Sleep { get; set; }
    public string Sick { get; set; }
    public string Panic { get; set; }
    public string Poison { get; set; }
}

public class Samurai : Unit
{
    public Samurai(string name, Stats stats, Affinity affinity, List<string> skills, string unitType) : base(name, stats, affinity,
        skills, "samurai") { }
    
    public Samurai() { }
}

public class Monster : Unit
{
    public Monster(string name, Stats stats, Affinity affinity, List<string> skills, string unitType) : base(name, stats, affinity,
        skills, "monster") { }
    
    public Monster() { }
}