public static class BattleTempo
{
    // Keep frame rendering at 60 FPS while battle time advances at half speed.
    public const float SimulationSpeed = 0.5f;

    public static float ScaleDeltaTime(float deltaTime)
    {
        return deltaTime * SimulationSpeed;
    }
}
