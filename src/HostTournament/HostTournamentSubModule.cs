using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;

namespace HostTournament
{
    /// <summary>
    /// 模块入口。注册 HostTournamentBehavior 到战役系统。
    /// </summary>
    public class HostTournamentSubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            HostTournamentLogger.Log("HostTournament 模块加载");
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (!(gameStarterObject is CampaignGameStarter campaignStarter)) return;

            // 注册核心 CampaignBehavior：城镇菜单选项 + 物品选择 + 创建赛事
            campaignStarter.AddBehavior(new HostTournamentBehavior());
            HostTournamentLogger.Log("HostTournamentBehavior 已注册");
        }
    }
}
