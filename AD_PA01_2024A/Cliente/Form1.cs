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
using System.Net.Sockets;
using System.Windows.Forms;
using Protocolo;

namespace Cliente
{
    public partial class FrmValidador : Form
    {
        private TcpClient remoto;
        private NetworkStream flujo;
        private Protocolos protocolo;

        public FrmValidador()
        {
            InitializeComponent();
        }

        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();
                protocolo = new Protocolos(flujo);
                panPlaca.Enabled = false;
                
            }
            catch (SocketException ex)
            {
                MessageBox.Show("No se pudo establecer conexión: " + ex.Message, "ERROR");
            }
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text;
            string contraseña = txtPassword.Text;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contraseña))
            {
                MessageBox.Show("Se requiere el ingreso de usuario y contraseña", "ADVERTENCIA");
                return;
            }

            try
            {
                var respuesta = protocolo.HazOperacion("INGRESO", new[] { usuario, contraseña });

                if (respuesta.Estado == "OK" && respuesta.Mensaje == "ACCESO_CONCEDIDO")
                {
                    panPlaca.Enabled = true;
                    panLogin.Enabled = false;
                    txtModelo.Focus();

                    MessageBox.Show("Acceso concedido", "INFORMACIÓN");
                }
                else
                {
                    MessageBox.Show("No se pudo ingresar, revise credenciales", "ERROR");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "ERROR");
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            try
            {
                var respuesta = protocolo.HazOperacion("CALCULO", new[] { txtModelo.Text, txtMarca.Text, txtPlaca.Text });
                MessageBox.Show("Respuesta recibida: " + respuesta.Mensaje, "INFORMACIÓN");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "ERROR");
            }
        }

        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            try
            {
                var respuesta = protocolo.HazOperacion("CONTADOR", new string[0]);
                MessageBox.Show($"Número de consultas: {respuesta.Mensaje}", "INFORMACIÓN");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "ERROR");
            }
        }

        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            flujo?.Close();
            remoto?.Close();
        }
    }
}
