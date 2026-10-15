namespace HostTournament
{
    /// <summary>
    /// 全局配置常量。
    /// </summary>
    public static class HostTournamentConfig
    {
        // ===== 菜单/选项 ID =====
        public const string MenuOptionId = "host_tournament_host_option";
        public const string MenuOptionText = "举办竞技大赛（缴纳奖金）";

        // ===== 奖金 =====
        /// <summary>举办竞技大赛需缴纳的金币。从玩家金币中扣除。</summary>
        public const int HostGoldCost = 5000;

        // ===== 行为开关 =====
        /// <summary>是否在赛事举办后立即询问玩家是否参赛。</summary>
        public const bool AskToJoinAfterHost = true;

        // ===== 调试 =====
        public const bool VerboseLog = false;
    }
}
