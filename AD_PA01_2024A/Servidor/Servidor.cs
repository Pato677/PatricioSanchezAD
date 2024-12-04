// ************************************************************************
// Practica 07
// Patricio Vladimir Sánchez Espinoza 
// Fecha de realización: 27/11/2024
// Fecha de entrega: 04/12/2024
// Resultados:
// La práctica ha permitido entender cómo se maneja la comunicación en red utilizando TcpClient y NetworkStream,
//y cómo estructurar una aplicación para procesar comandos y respuestas mediante clases como Protocolo, Pedido y Respuesta.

// Conclusiones:
// Patricio Sanchez
//1. En conclusion en la práctica de usar GitHub permitió gestionar las versiones del proyecto de manera eficiente, proporcionando un
//control adecuado sobre el historial de cambios. Además, facilitó la colaboración en equipo, permitiendo realizar
//un seguimiento detallado de las modificaciones y revertir cambios cuando fue necesario, asegurando una estructura
//ordenada y organizada del código fuente.

//2.En conclusion integrar GitHub con Visual Studio, se mejoró la implementación de la integración continua, facilitando el manejo de ramas y
//la fusión de cambios sin conflictos. Esto optimizó el proceso de desarrollo al permitir pruebas continuas y una mayor trazabilidad
//de los errores y las mejoras realizadas, mejorando la productividad del equipo de trabajo.

// Recomendaciones:
// Patricio Sanchez
//1.  Se recomienda seguir una estrategia de ramas como Git Flow o Feature Branches para gestionar las distintas fases del desarrollo, lo que
//permitirá integrar nuevas funcionalidades de manera controlada y sin interferir con el código principal. Esto facilita la colaboración y mejora
//el control sobre el desarrollo de nuevas características o correcciones.

//2. Se recomienda implementar GitHub Actions para automatizar las pruebas unitarias y la integración continua. Esto asegurará que el código se
//valide automáticamente con cada cambio realizado, mejorando la calidad del software y reduciendo los errores en etapas posteriores del desarrollo.
//Además, la automatización de pruebas facilita un flujo de trabajo más eficiente y reduce la intervención manual.
// ********************************************************************************************************************
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Protocolo;

namespace Servidor
{
    class Servidor
    {
        private static TcpListener escuchador;
        private static Dictionary<string, int> listadoClientes = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            try
            {
                escuchador = new TcpListener(IPAddress.Any, 8080);
                escuchador.Start();
                Console.WriteLine("Servidor iniciado en el puerto 8080...");

                while (true)
                {
                    TcpClient cliente = escuchador.AcceptTcpClient();
                    Console.WriteLine("Cliente conectado desde: " + cliente.Client.RemoteEndPoint);
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket: " + ex.Message);
            }
            finally
            {
                escuchador?.Stop();
            }
        }

        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;

            try
            {
                flujo = cliente.GetStream();
                byte[] bufferRx = new byte[1024];
                int bytesRx;

                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                    string direccionCliente = cliente.Client.RemoteEndPoint.ToString();

                    Respuesta respuesta = Protocolos.ResolverPedido(mensaje, direccionCliente, ref listadoClientes);
                    Console.WriteLine($"Pedido: {mensaje} | Respuesta: {respuesta}");

                    byte[] bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de cliente: " + ex.Message);
            }
            finally
            {
                flujo?.Close();
                cliente?.Close();
            }
        }
    }
}
