// SmitePnB — Smite 2 Pick & Ban broadcasting tool
// Remade by diese | original concept from the SAL/SDL community
using System.Windows;
using SmitePnB.Services;

namespace SmitePnB;

public partial class App : Application
{
    public static ResourceLoader Resources { get; private set; } = null!;
    public static StateSerializer State { get; private set; } = null!;
    public static AudioService Audio { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Resources = new ResourceLoader();
        State     = new StateSerializer();
        Audio     = new AudioService();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Audio.Dispose();
        base.OnExit(e);
    }
}
