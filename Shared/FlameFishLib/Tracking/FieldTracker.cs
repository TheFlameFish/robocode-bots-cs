using System.Globalization;
using System.Numerics;
using FlameFishLib.Units;
using Robocode.TankRoyale.BotApi.Events;
using Robocode.TankRoyale.BotApi.Graphics;

namespace FlameFishLib.Tracking;

public static class FieldTracker
{
    public const int STALENESS_THRESHOLD = 20;

    private static bool initialized = false;

    private static int width;
    private static int height;

    public static int Width { get => width; }
    public static int Height { get => height; }

    private static Dictionary<int, TrackedBotData> trackedBots = new Dictionary<int, TrackedBotData>();
    public static Dictionary<int, TrackedBotData> TrackedBots { get => trackedBots; }

    private static int currentTurn;

    public static void Init(int width, int height)
    {
        if (initialized) { return; }

        initialized = true;

        FieldTracker.width = width;
        FieldTracker.height = height;
    }

    public static void RoundStart()
    {
        if (currentTurn == -1)
        {
            return; // RoundStart has probably already been called.
        }

        trackedBots = new Dictionary<int, TrackedBotData>();
        currentTurn = -1;
    }

    public static void ObserveBot(int botId, int currentTurn, Transform position, double speed, double energy)
    {
        if (trackedBots.ContainsKey(botId))
        {
            trackedBots[botId].UpdateData(currentTurn, position, speed, energy);
        }
        else
        {
            trackedBots.Add(
                botId,
                new TrackedBotData(
                    botId,
                    currentTurn,
                    position,
                    MiscUtil.VectorFromPolar(position.rotation, (float)speed),
                    energy
                )
            );
        }
    }

    public static void ObserveBot(ScannedBotEvent e)
    {
        ObserveBot(e.ScannedBotId, e.TurnNumber, new Transform(e.X, e.Y, Angle.FromDegrees(e.Direction)), e.Speed, e.Energy);
    }

    public static void RemoveBot(int botId)
    {
        if (trackedBots.ContainsKey(botId))
        {
            trackedBots.Remove(botId);
        }
    }

    public static void OnTick(int currentTurn)
    {
        if (currentTurn == FieldTracker.currentTurn)
        {
            return; // Don't run twice on the same turn
        }
        FieldTracker.currentTurn = currentTurn;

        foreach (var pair in trackedBots)
        {
            if (currentTurn - pair.Value.lastSeenTime > STALENESS_THRESHOLD)
            {
                RemoveBot(pair.Key);
                Console.WriteLine($"Removed bot {pair.Key} from tracking due to staleness (>{STALENESS_THRESHOLD} turns since last seen)");
            }
        }
    }

    public static void DrawData(IGraphics graphics, int currentTurn)
    {
        graphics.SetStrokeWidth(0);
        graphics.SetFillColor(Color.FromRgba(255, 0, 0, 155));

        foreach (var pair in trackedBots)
        {
            Transform positionEstimate = pair.Value.EstimatePosition(currentTurn);
            graphics.FillCircle(positionEstimate.X, positionEstimate.Y, 20);
        }
        
        graphics.SetFillColor(Color.FromRgba(50, 50, 50, 155));

        foreach (var pair in trackedBots) {
            Transform positionLast = pair.Value.lastPosition;
            graphics.FillCircle(positionLast.X, positionLast.Y, 20);
        }
    }
}