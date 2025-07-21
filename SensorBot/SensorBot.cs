using FlameFishLib.Tracking;
using FlameFishLib.Units;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using FlameFishLib;
using System;
using System.Numerics;
using Robocode.TankRoyale.BotApi.Graphics;

public class SensorBot : Bot
{
    Vector2? target;
    
    // The main method starts our bot
    static void Main(string[] args)
    {
        new SensorBot().Start();
    }

    public override void OnGameStarted(GameStartedEvent gameStatedEvent)
    {
        base.OnGameStarted(gameStatedEvent);
        FieldTracker.Init(ArenaWidth, ArenaHeight);
    }

    public override void Run()
    {
        FieldTracker.RoundStart();
        AdjustRadarForGunTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
        while (IsRunning)
        {
            RadarTurnRate = MaxRadarTurnRate;

            Vector2? nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (var pair in FieldTracker.TrackedBots)
            {
                var id = pair.Key;
                var value = pair.Value;

                if (id == MyId)
                {
                    continue;
                }

                Vector2 position = value.EstimatePosition(TurnNumber).translation;
                float distance = (new Vector2((float)X, (float)Y) - position).Length();
                if (nearestDistance > distance)
                {
                    nearest = position;
                    nearestDistance = distance;
                }
            }

            target = nearest;

            if (nearest.HasValue)
            {
                // Console.WriteLine($"Target: {nearest.Value}");
                double bearing = this.GunBearingTo(nearest.Value.X, nearest.Value.Y);

                GunTurnRate = bearing / 1.5;

                if (Math.Abs(bearing) < 1)
                {
                    double power = Math.Clamp(
                        -Math.Sin(nearestDistance / 800 / (0.5 * Math.PI)) * 4 + 3,
                        1,
                        3);

                    Fire(power);
                }
            }
            else
            {
                GunTurnRate = 0;
            }
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent scannedBotEvent)
    {
        Console.WriteLine($"Scanned bot {scannedBotEvent.ScannedBotId}");
        FieldTracker.ObserveBot(scannedBotEvent);
    }

    public override void OnBotDeath(BotDeathEvent botDeathEvent)
    {
        FieldTracker.RemoveBot(botDeathEvent.VictimId);
        Console.WriteLine($"Bot {botDeathEvent.VictimId} died.");
    }

    public override void OnTick(TickEvent tickEvent)
    {
        FieldTracker.ObserveBot(MyId, tickEvent.TurnNumber, new Transform(X, Y, Angle.FromDegrees(Direction)), Speed, Energy);
        FieldTracker.OnTick(tickEvent.TurnNumber);

        var g = Graphics;

        FieldTracker.DrawData(g, TurnNumber);

        Vector2 gunRay = MiscUtil.VectorFromPolar(Angle.FromDegrees(GunDirection), (float)1e5);

        g.SetStrokeColor(Color.Red);
        g.SetStrokeWidth(2);
        g.DrawLine(X, Y, gunRay.X, gunRay.Y);

        if (target.HasValue)
        {
            Vector2 val = target.Value;

            g.DrawCircle(val.X, val.Y, 25);
            g.DrawText("TARGET", val.X + 30, val.Y + 30);
        }
    }

    public override void OnBulletHit(BulletHitBotEvent e)
    {

    }
}