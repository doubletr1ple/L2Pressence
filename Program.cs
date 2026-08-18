using System.Windows.Forms;

namespace L2Presence;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var settings = new AppSettings();
            settings.Validate();

            Application.Run(new TrayApplicationContext(settings));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"L2Presence could not start.\n\n{ex.Message}",
                "L2Presence",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
