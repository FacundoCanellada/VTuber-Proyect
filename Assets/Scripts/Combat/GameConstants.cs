namespace UndertaleEncounter
{
    public enum BattleState
    {
        INTRO,
        PLAYER_MENU,
        FIGHT_TARGET,
        ACT_TARGET,
        ACT_MENU,
        ITEM_MENU,
        MERCY_MENU,
        MESSAGE,
        ATTACK_TIMING,
        ENEMY_MONOLOGUE,
        ENEMY_TURN,
        VICTORY,
        GAME_OVER
    }

    public enum SoulMode
    {
        RED_FREE_MOVE,
        BLUE_GRAVITY,
        GREEN_SHIELD,
        YELLOW_SHOOTER
    }

    public enum ActionType
    {
        FIGHT,
        ACT,
        ITEM,
        MERCY
    }
}
