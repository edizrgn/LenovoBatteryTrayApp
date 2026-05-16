namespace LenovoBatteryTray.Battery
{
    public interface IBatteryModeController
    {
        LenovoBatteryMode GetMode();

        void SetMode(LenovoBatteryMode mode);
    }
}
