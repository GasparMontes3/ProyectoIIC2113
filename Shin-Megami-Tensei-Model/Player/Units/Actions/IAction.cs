namespace Shin_Megami_Tensei_Model.Actions;

public interface IAction
{
    List<int> Execute(Unit actor, Player player);
}