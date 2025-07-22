using System.Numerics;
using Robocode.TankRoyale.BotApi;

namespace FlameFishLib.Tracking;

public class TrackedBotData
{
    public readonly int id;

    public int lastSeenTime;

    public Transform lastPosition;
    public Vector2 velocity;

    public double energy;

    public TrackedBotData(int id, int currentTurn, Transform position, Vector2 velocity, double energy)
    {
        this.id = id;
        this.lastPosition = position;
        this.velocity = velocity;
        this.energy = energy;
        lastSeenTime = currentTurn;
    }

    public override string ToString()
    {
        return String.Format(
            "Bot {0}; Last Known Position: {1}; Energy: {2}; Speed: {3}",
            id, lastPosition, energy, velocity.Length()
        );
    }

    public string DebugString(int currentTurn)
    {
        return String.Format(
            "Bot {0}; Last Known Position: {1}; Estimate Position: {3}; Energy: {2}; Speed: {4}",
            id, lastPosition, energy, EstimatePosition(currentTurn), velocity.Length()
        );
    }

    public void UpdateData(int currentTurn, Transform position, Vector2 velocity, double energy)
    {
        this.lastPosition = position;
        this.velocity = velocity;
        this.energy = energy;
        lastSeenTime = currentTurn;
    }

    public void UpdateData(int currentTurn, Transform position, double speed, double energy)
    {
        Vector2 velocity = MiscUtil.VectorFromPolar(position.rotation, (float)speed);
        UpdateData(currentTurn, position, velocity, energy);
    }

    /// <summary>
    /// Gives an estimated position based on the last observed velocity of the bot.
    /// </summary>
    public Transform EstimatePosition(int currentTurn)
    {
        return lastPosition + (velocity * (currentTurn - lastSeenTime));
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