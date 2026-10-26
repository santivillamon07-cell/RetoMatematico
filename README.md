# Reto Matemático

Proyecto de Windows Forms en C#. Es un juego de operaciones matemáticas que va subiendo de dificultad mientras uno acierta, y se puede jugar solo o con otra persona por turnos.

Lo hice a partir del programa de consola que teníamos antes, pero esta vez lo separé en varias pantallas y en clases para que no quedara todo metido en un solo archivo.

## Cómo correrlo
- Abrir `RetoMatematico.sln` en Visual Studio (yo usé Insiders, necesita .NET 8).
- Darle F5.

Nota: las ventanas las hice por código y no con el diseñador, entonces para ver el código de los formularios hay que darles clic derecho → Ver código (o F7).

## Cómo lo organicé

Decidí dividir el proyecto en tres carpetas para que la parte de las matemáticas no dependa de la interfaz:

**Logica** (todo lo que es el juego en sí)
- `GeneradorOperaciones.cs`: crea las operaciones dependiendo del nivel.
- `ControlDificultad.cs`: tiene los rangos, el tiempo y la descripción de cada nivel. Cada 3 aciertos se sube un nivel.
- `MotorJuego.cs`: lleva el control de la partida (turnos, vidas, puntos, rachas y quién gana).
- `Validador.cs`: revisa que los nombres y respuestas sean válidos.
-