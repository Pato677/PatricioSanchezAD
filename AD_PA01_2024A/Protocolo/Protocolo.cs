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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Protocolo
{
    public class Pedido
    {
        public string Comando { get; set; }
        public string[] Parametros { get; set; }

        public static Pedido Procesar(string mensaje)
        {
            var partes = mensaje.Split(' ');
            return new Pedido
            {
                Comando = partes[0].ToUpper(),
                Parametros = partes.Skip(1).ToArray()
            };
        }

        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }

    public class Respuesta
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }

        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }

    public class Protocolos
    {
        private NetworkStream flujo;

        public Protocolos(NetworkStream flujo)
        {
            this.flujo = flujo;
        }

        public Respuesta HazOperacion(string comando, string[] parametros)
        {
            if (flujo == null)
                throw new InvalidOperationException("No hay conexión establecida.");

            try
            {
                // Crear y enviar pedido
                var pedido = new Pedido { Comando = comando, Parametros = parametros };
                byte[] bufferTx = Encoding.UTF8.GetBytes(pedido.ToString());
                flujo.Write(bufferTx, 0, bufferTx.Length);

                // Recibir respuesta
                byte[] bufferRx = new byte[1024];
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);
                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                var partes = mensaje.Split(' ');
                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };
            }
            catch (SocketException ex)
            {
                throw new InvalidOperationException($"Error al intentar transmitir: {ex.Message}", ex);
            }
        }

        public static Respuesta ResolverPedido(string mensaje, string direccionCliente, ref Dictionary<string, int> listadoClientes)
        {
            Pedido pedido = Pedido.Procesar(mensaje);
            Respuesta respuesta = new Respuesta { Estado = "NOK", Mensaje = "Comando no reconocido" };

            switch (pedido.Comando)
            {
                case "INGRESO":
                    if (pedido.Parametros.Length == 2 &&
                        pedido.Parametros[0] == "root" &&
                        pedido.Parametros[1] == "admin20")
                    {
                        respuesta = new Random().Next(2) == 0
                            ? new Respuesta { Estado = "OK", Mensaje = "ACCESO_CONCEDIDO" }
                            : new Respuesta { Estado = "NOK", Mensaje = "ACCESO_NEGADO" };
                    }
                    else
                    {
                        respuesta.Mensaje = "ACCESO_NEGADO";
                    }
                    break;

                case "CALCULO":
                    if (pedido.Parametros.Length == 3)
                    {
                        string placa = pedido.Parametros[2];
                        if (Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$"))
                        {
                            byte indicadorDia = ObtenerIndicadorDia(placa);
                            respuesta = new Respuesta
                            { Estado = "OK", Mensaje = $"{placa} {indicadorDia}" };

                            if (!listadoClientes.ContainsKey(direccionCliente))
                                listadoClientes[direccionCliente] = 0;

                            listadoClientes[direccionCliente]++;
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;

                case "CONTADOR":
                    respuesta = listadoClientes.ContainsKey(direccionCliente)
                        ? new Respuesta
                        { Estado = "OK", Mensaje = listadoClientes[direccionCliente].ToString() }
                        : new Respuesta { Estado = "NOK", Mensaje = "No hay solicitudes previas" };
                    break;
            }

            return respuesta;
        }

        private static byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(6, 1));
            switch (ultimoDigito)
            {
                case 1:
                case 2:
                    return 0b00100000; // Lunes
                case 3:
                case 4:
                    return 0b00010000; // Martes
                case 5:
                case 6:
                    return 0b00001000; // Miércoles
                case 7:
                case 8:
                    return 0b00000100; // Jueves
                case 9:
                case 0:
                    return 0b00000010; // Viernes
                default:
                    return 0;
            }
        }
    }
}
