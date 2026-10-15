using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace HostTournament
{
    /// <summary>
    /// 让玩家在未举办竞技大赛的城镇，缴纳金币来主动发起一次竞技大赛。
    /// 赛事奖品由 vanilla 自动生成，不再尝试覆盖（避免 vanilla 多处覆盖 Prize 的问题）。
    ///
    /// 核心流程：
    /// 1. 城镇菜单加"举办竞技大赛"选项（条件：该城无赛事 + 玩家金币足够）
    /// 2. 玩家点击 → 扣除 HostGoldCost 金币
    /// 3. 创建 FightTournamentGame(town) → AddTournament(game) → vanilla 自动生成奖品
    /// 4. 询问玩家是否立即参赛
    /// 5. 竞技场菜单文本增强：显示当前正在举办竞技大赛的城镇列表
    /// </summary>
    public class HostTournamentBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore) { }

        // ===== 会话启动：注册菜单 =====
        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            try
            {
                // 城镇菜单选项
                campaignGameStarter.AddGameMenuOption(
                    "town",
                    HostTournamentConfig.MenuOptionId,
                    HostTournamentConfig.MenuOptionText,
                    OnHostOptionCondition,
                    OnHostOptionConsequence,
                    isLeave: false,
                    index: -1,
                    isRepeatable: true);

                // 竞技场菜单文本增强：隐藏选项，其 condition 覆盖 ADDITIONAL_STRING
                campaignGameStarter.AddGameMenuOption(
                    "town_arena",
                    "host_tournament_arena_text_enhancer",
                    "",
                    OnArenaTextEnhancerCondition,
                    null,
                    isLeave: false,
                    index: -1,
                    isRepeatable: true);

                HostTournamentLogger.Log("OnSessionLaunched: 菜单选项 + 竞技场文本增强已注册");
            }
            catch (Exception ex)
            {
                HostTournamentLogger.LogErr("OnSessionLaunched 异常: " + ex);
            }
        }

        // ===== 竞技场文本增强：覆盖 ADDITIONAL_STRING 追加赛事状态 =====
        private bool OnArenaTextEnhancerCondition(MenuCallbackArgs args)
        {
            try
            {
                var sb = new StringBuilder();

                // 列出正在举办竞技大赛的城镇
                var hosting = new List<string>();
                foreach (Town town in Town.AllTowns)
                {
                    if (town.HasTournament)
                    {
                        string name = town.Name?.ToString() ?? town.StringId;
                        hosting.Add(name);
                    }
                }

                if (hosting.Count > 0)
                {
                    sb.AppendLine("正在举办竞技大赛的城镇：");
                    foreach (var name in hosting)
                    {
                        sb.AppendLine("  • " + name);
                    }
                }
                else
                {
                    sb.AppendLine("当前没有城镇在举办竞技大赛。");
                }

                // 覆盖 ADDITIONAL_STRING（注意：不要包含"你在竞技场附近"，那是菜单模板自带的）
                MBTextManager.SetTextVariable("ADDITIONAL_STRING", sb.ToString(), false);
            }
            catch (Exception ex)
            {
                HostTournamentLogger.LogErr("OnArenaTextEnhancerCondition 异常: " + ex.Message);
            }

            args.IsEnabled = false;
            return false;
        }

        // ===== 菜单条件：玩家在城镇 + 该城无赛事 + 金币足够 =====
        private bool OnHostOptionCondition(MenuCallbackArgs args)
        {
            try
            {
                // 设置图标（PracticeFight = 竞技场战斗图标）
                args.optionLeaveType = GameMenuOption.LeaveType.PracticeFight;

                Settlement settlement = GetCurrentSettlement();
                if (settlement == null || !settlement.IsTown)
                {
                    args.IsEnabled = false;
                    args.Tooltip = new TextObject("需要在城镇内");
                    return true;
                }

                Town town = settlement.Town;
                if (town == null) { args.IsEnabled = false; return true; }

                if (town.HasTournament)
                {
                    args.IsEnabled = false;
                    args.Tooltip = new TextObject("本城正在举办竞技大赛");
                    return true;
                }

                if (Hero.MainHero.Gold < HostTournamentConfig.HostGoldCost)
                {
                    args.IsEnabled = false;
                    args.Tooltip = new TextObject("金币不足，需要 " + HostTournamentConfig.HostGoldCost + " 第纳尔");
                    return true;
                }

                args.IsEnabled = true;
                args.Tooltip = new TextObject("缴纳 " + HostTournamentConfig.HostGoldCost + " 第纳尔举办竞技大赛");
                return true;
            }
            catch (Exception ex)
            {
                HostTournamentLogger.LogErr("OnHostOptionCondition 异常: " + ex.Message);
                args.IsEnabled = false;
                return true;
            }
        }

        private Settlement GetCurrentSettlement()
        {
            MobileParty mainParty = MobileParty.MainParty;
            if (mainParty == null) return null;
            return mainParty.CurrentSettlement;
        }

        // ===== 菜单后果：扣除金币 + 创建赛事 =====
        private void OnHostOptionConsequence(MenuCallbackArgs args)
        {
            try
            {
                Settlement settlement = GetCurrentSettlement();
                if (settlement == null || !settlement.IsTown || settlement.Town == null)
                {
                    InformationManager.DisplayMessage(new InformationMessage("需要在城镇内才能举办竞技大赛。"));
                    return;
                }

                if (settlement.Town.HasTournament)
                {
                    InformationManager.DisplayMessage(new InformationMessage("本城已经在举办竞技大赛了。"));
                    return;
                }

                if (Hero.MainHero.Gold < HostTournamentConfig.HostGoldCost)
                {
                    InformationManager.DisplayMessage(new InformationMessage("金币不足。"));
                    return;
                }

                Town town = settlement.Town;

                // 1. 扣除金币（用 GiveGoldAction 转给城镇，模拟赞助赛事）
                GiveGoldAction.ApplyForPartyToSettlement(
                    MobileParty.MainParty.Party,
                    settlement,
                    HostTournamentConfig.HostGoldCost,
                    true);
                HostTournamentLogger.Log("扣除金币: " + HostTournamentConfig.HostGoldCost + " 给 " + settlement.Name);

                // 2. 创建赛事并注册（vanilla 自动生成奖品）
                FightTournamentGame game = new FightTournamentGame(town);
                Campaign.Current.TournamentManager.AddTournament(game);
                HostTournamentLogger.Log("赛事已创建并注册: town=" + settlement.Name);

                // 3. 刷新城镇菜单（让按钮变灰）
                GameMenu.SwitchToMenu("town");

                // 4. 提示
                InformationManager.DisplayMessage(new InformationMessage(
                    "你花费 " + HostTournamentConfig.HostGoldCost + " 第纳尔在 " + settlement.Name + " 举办了竞技大赛。"));

                // 5. 询问是否参赛
                if (HostTournamentConfig.AskToJoinAfterHost)
                {
                    ShowJoinInquiry(settlement, game);
                }
            }
            catch (Exception ex)
            {
                HostTournamentLogger.LogErr("OnHostOptionConsequence 异常: " + ex);
                InformationManager.DisplayMessage(new InformationMessage("举办竞技大赛时出错: " + ex.Message));
            }
        }

        // ===== 询问玩家是否立即参赛 =====
        private void ShowJoinInquiry(Settlement settlement, TournamentGame game)
        {
            var inquiry = new InquiryData(
                "参加竞技大赛？",
                "竞技大赛已举办。你要现在参加吗？",
                true,
                true,
                "参加",
                "稍后",
                () =>
                {
                    try
                    {
                        HostTournamentLogger.Log("玩家选择参加 → OpenMission");
                        game.OpenMission(settlement, true);
                    }
                    catch (Exception ex)
                    {
                        HostTournamentLogger.LogErr("OpenMission 异常: " + ex);
                        InformationManager.DisplayMessage(new InformationMessage("进入竞技场时出错: " + ex.Message));
                    }
                },
                () => { HostTournamentLogger.LogV("玩家选择稍后参加"); },
                "",
                0f,
                null,
                null,
                null);

            InformationManager.ShowInquiry(inquiry, true, true);
        }
    }
}
