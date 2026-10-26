using RetoMatematico.Datos;
using RetoMatematico.UI;

namespace RetoMatematico
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Un solo registro vive durante toda la sesión y se comparte entre pantallas
            RegistroPuntajes registro = new RegistroPuntajes();

            Application.Run(new FrmInicio(registro));
        }
    }
}
