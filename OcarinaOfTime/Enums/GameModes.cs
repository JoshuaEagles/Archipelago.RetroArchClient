namespace Archipelago.RetroArchClient.OcarinaOfTime.Enums;

/// <summary>
/// Represents all possible game modes in which the game can find itself in during runtime.
/// </summary>
public enum GameModes
{
    /// <summary>
    /// The N64 Logo splash screen.
    /// </summary>
    N64Logo,
    
    /// <summary>
    /// The Ocarina of Time title screen.
    /// </summary>
    TitleScreen,
    
    /// <summary>
    /// The game state of the file select menu.
    /// </summary>
    FileSelect,
    
    /// <summary>
    /// The game state when Link is currently dying.
    /// </summary>
    Dying,
    
    /// <summary>
    /// The game state when the game is in a cutscene.
    /// </summary>
    Cutscene,
    
    /// <summary>
    /// The game state when Link can run around, interact with the world, etc.
    /// </summary>
    NormalGameplay,
    
    /// <summary>
    /// The game state when the pause menu is currently active.
    /// </summary>
    Paused,
    
    /// <summary>
    /// The game state when the game has been started from the death menu.
    /// </summary>
    DyingMenuStart,
    
    /// <summary>
    /// The game state when link stopped dying and is officially dead.
    /// </summary>
    Dead
}