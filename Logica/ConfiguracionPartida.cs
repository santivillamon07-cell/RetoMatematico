namespace RetoMatematico.Logica
{
    public enum ModoJuego
    {
        Individual,
        DosJugadores
    }

    public enum Dificultad
    {
        Facil,
        Normal,
        Dificil
    }

    /// <summary>
    /// Datos que se eligen en la pantalla de inicio. No tiene lógica, solo viaja entre pantallas.
    /// </summary>
    public class ConfiguracionPartida
    {
        public ModoJuego Modo { get; set; }
        public string[] Nombres { get; set; }          // 1 o 2 nombres según el modo
        public Dificultad DificultadInicial { get; set; }
        public int Vidas { get; set; }
        public int RondasPorJugador { get; set; }      // solo aplica en modo DosJugadores

        public int CantidadJugadores
        {
            get { return Modo == ModoJuego.Individual ? 1 : 2; }
        }

        public static string TextoModo(ModoJuego modo)
        {
            return modo == ModoJuego.Individual ? "Individual" : "Dos jugadores";
        }
    }
}
