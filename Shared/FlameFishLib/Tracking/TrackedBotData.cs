using System.Numerics;
using FlameFishLib.Units;
using Robocode.TankRoyale.BotApi;

namespace FlameFishLib.Tracking;

public class TrackedBotData
{
    public readonly int id;

    public int lastSeenTime;

    public Transform lastPosition;
    public double speed;

    public double energy;

    /// <summary>
    /// The angle before the last update
    /// </summary>
    Angle? previousAngle = null;
    /// <summary>
    /// The time before the last update
    /// </summary>
    int? previousTime = null;

    public TrackedBotData(int id, int currentTurn, Transform position, double speed, double energy)
    {
        this.id = id;
        this.lastPosition = position;
        this.speed = speed;
        this.energy = energy;
        lastSeenTime = currentTurn;
    }

    public override string ToString()
    {
        return String.Format(
            "Bot {0}; Last Known Position: {1}; Energy: {2}; Speed: {3}",
            id, lastPosition, energy, speed
        );
    }

    public string DebugString(int currentTurn)
    {
        return String.Format(
            "Bot {0}; Last Known Position: {1}; Estimate Position: {3}; Energy: {2}; Speed: {4}",
            id, lastPosition, energy, EstimatePosition(currentTurn), speed
        );
    }

    public void UpdateData(int currentTurn, Transform position, double speed, double energy)
    {
        previousAngle = lastPosition.rotation;
        previousTime = lastSeenTime;
        lastPosition = position;
        this.speed = speed;
        this.energy = energy;
        lastSeenTime = currentTurn;
    }

    /// <summary>
    /// Gives an estimated position based on the last observed velocity of the bot.
    /// </summary>
    public Transform EstimatePosition(int currentTurn)
    {
        if (!previousAngle.HasValue | !previousTime.HasValue)
        {
            return lastPosition + (MiscUtil.VectorFromPolar(lastPosition.rotation, (float)speed) * (currentTurn - lastSeenTime));
        }

        int delta = currentTurn - lastSeenTime;

        // calc angular velocity
        int angleDeltaTime = lastSeenTime - previousTime.GetValueOrDefault(); // Default shouldn't be needed but the compiler gets angy if I don't
        Angle angleDelta = lastPosition.rotation - previousAngle.GetValueOrDefault();

        // Console.WriteLine($"{lastPosition.rotation} - {previousAngle.GetValueOrDefault()} = {angleDelta}");

        Angle angularVelocity = angleDelta / angleDeltaTime;
        // Console.WriteLine(angularVelocity.Degrees);

        // this is kinda funky
        Transform position = new(lastPosition.X, lastPosition.Y, lastPosition.rotation);
        for (int i = 0; i < delta; i++)
        {
            position.rotation += angularVelocity;

            Angle rotation = position.rotation;
            position.translation += MiscUtil.VectorFromPolar(rotation, (float)speed);
        }

        return position;
    }

    public Transform GetLeadPosition(int currentTurn, Transform turretPosition, double firepower)
    {
        Transform targetPosition = EstimatePosition(currentTurn);

        float distance = (turretPosition - targetPosition).translation.Length();

        double bulletSpeed = 20.0 - 3.0 * Math.Clamp(firepower, 0.1, 3.0); // Yoinked from BaseBot.CalcBulletSpeed
        float bulletReachTime = distance / (float)bulletSpeed;
        int bulletReachTurns = (int)Math.Round(bulletReachTime);

        Transform pos = targetPosition;
        for (int i = 0; i < bulletReachTurns; i++)
        {
            pos = EstimatePosition(currentTurn + i);
            if (pos.X < 0 | pos.X > FieldTracker.Width | pos.Y < 0 | pos.Y > FieldTracker.Height)
            {
                break;
            }
        }

        return pos;
    }
}