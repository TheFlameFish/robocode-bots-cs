using FlameFishLib.Tracking;
using FlameFishLib.Units;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using FlameFishLib;
using System;
using System.Numerics;
using Robocode.TankRoyale.BotApi.Graphics;

public class LockBot : Bot
{
    TrackedBotData target;

    double scanProgress = 0;

    enum State
    {
        SEARCH,
        ATTACK,
    }

    State currentState = State.SEARCH;

    // The main method starts our bot
    static void Main(string[] args)
    {
        new LockBot().Start();
    }

    public override void OnGameStarted(GameStartedEvent gameStatedEvent)
    {
        base.OnGameStarted(gameStatedEvent);
        FieldTracker.Init(ArenaWidth, ArenaHeight);
    }

    public override void Run()
    {
        FieldTracker.RoundStart();
        target = null;
        currentState = State.SEARCH;
        AdjustRadarForGunTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
    }

    private void LookForTargets()
    {
        RadarTurnRate = MaxRadarTurnRate;

        if (scanProgress >= 360)
        {
            scanProgress = 0;
            TrackedBotData nearest = null;
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
                    nearest = pair.Value;
                    nearestDistance = distance;
                }
            }

            target = nearest;
        }
        else
        {
            scanProgress += RadarTurnRate;
        }
    }

    private void AimAndFire()
    {
        if (target != null)
        {
            var g = Graphics;
            g.SetStrokeColor(Color.Red);
            g.SetStrokeWidth(2);

            Transform targetPosition = target.EstimatePosition(TurnNumber);
            float distance = (targetPosition.translation - this.VectorPosition()).Length();

            g.DrawCircle(targetPosition.X, targetPosition.Y, 25);
            g.DrawText("TARGET", targetPosition.X + 30, targetPosition.Y + 30);

            double power = Math.Clamp(
                    -Math.Sin(distance / 800 / (0.5 * Math.PI)) * 4 + 3,
                    1,
                    3);

            Transform leadPosition = target.GetLeadPosition(TurnNumber, new Transform(X, Y, Angle.FromDegrees(GunDirection)), power);

            float leadSquareLen = 15;
            g.DrawRectangle(leadPosition.X - leadSquareLen/2, leadPosition.Y - leadSquareLen/2, 15, 15);
            g.DrawText("LEAD", leadPosition.X + 20, leadPosition.Y + 20);

            // Console.WriteLine($"Target: {nearest.Value}");
            double bearing = this.GunBearingTo(leadPosition.X, leadPosition.Y);

            GunTurnRate = bearing / 1.5;

            if (GunHeat == 0 && Math.Abs(bearing) < 1)
            {
                SetFire(power);
            }
        }
        else
        {
            GunTurnRate = 0;
        }
    }

    private void ScanTarget()
    {
        if (target == null) { return; }
        Transform targetPos = target.EstimatePosition(TurnNumber);
        double bearing = RadarBearingTo(targetPos.X, targetPos.Y);
        RadarTurnRate = bearing + (10 * Math.Sign(bearing));
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

        if (target != null)
        {
            Transform pos = target.EstimatePosition(TurnNumber);
            g.DrawCircle(pos.X, pos.Y, 25);
            g.DrawText("TARGET", pos.X + 30, pos.Y + 30);
        }

        switch (currentState)
        {
            case State.SEARCH:
                LookForTargets();
                if (target != null)
                {
                    currentState = State.ATTACK;
                }
                break;

            case State.ATTACK:
                if (target == null || target.energy <= 0)
                {
                    target = null;
                    currentState = State.SEARCH;
                    break;
                }
                ScanTarget();
                AimAndFire();
                break;
        }

        Go();
    }

    public override void OnBulletHit(BulletHitBotEvent e)
    {

    }
}