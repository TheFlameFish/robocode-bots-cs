using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class MyFirstBot : Bot
{
    // The main method starts our bot
    static void Main(string[] args)
    {
        new MyFirstBot().Start();
    }

    public override void Run() 
    {
        while (IsRunning) 
        {
            Forward(100);
            TurnRight(360);
            Back(100);
            TurnRight(360);
        }
    }

    public override void OnScannedBot(ScannedBotEvent scannedBotEvent)
    {
        Fire(1);
    }

    public override void OnHitByBullet(HitByBulletEvent bulletHitBotEvent)
    {
        double bearing = CalcBearing(bulletHitBotEvent.Bullet.Direction);

        // Face perpendicular to the bullet source
        TurnLeft(90 - bearing);
    }
}