using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace XelsCombatAI.Game;

internal static class PositionalTargetPolicy
{
    public static bool ShouldSuppressForTargetOfTarget(IBattleChara? player, IBattleChara? target, IDataManager dataManager, out string reason)
    {
        reason = string.Empty;
        if (player == null || target == null)
        {
            return false;
        }

        if (IsTrainingDummy(target, dataManager))
        {
            return false;
        }

        if (target.TargetObjectId != player.GameObjectId)
        {
            return false;
        }

        reason = "target is targeting player";
        return true;
    }

    public static bool CanApplyPositionals(IBattleChara? target, IDataManager dataManager)
    {
        if (target is not IBattleNpc npc ||
            npc.BattleNpcKind != BattleNpcSubKind.Combatant ||
            target.IsDead ||
            target.CurrentHp == 0)
        {
            return false;
        }

        return !HasDirectionalDisregard(target) &&
               !IsOmnidirectional(npc, dataManager);
    }

    private static bool HasDirectionalDisregard(IBattleChara target)
        => target.StatusList.Any(status =>
            status.StatusId == ActionUse.DirectionalDisregardStatusId &&
            status.RemainingTime > 0f);

    private static bool IsOmnidirectional(IBattleNpc target, IDataManager dataManager)
        => dataManager.GetExcelSheet<BNpcBase>().TryGetRow(target.BaseId, out var npcBase) &&
           npcBase.IsOmnidirectional;

    private static bool IsTrainingDummy(IBattleChara target, IDataManager dataManager)
        => target is ICharacter character &&
           dataManager.GetExcelSheet<BNpcName>(ClientLanguage.English).TryGetRow(character.NameId, out var npcName) &&
           npcName.Singular.ExtractText().Contains("dummy", StringComparison.OrdinalIgnoreCase);
}
