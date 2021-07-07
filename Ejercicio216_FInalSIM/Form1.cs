using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ejercicio216_FInalSIM
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private double generarNumeroUniforme(double desde, double hasta, double random)
        {
            return desde + (random * (hasta - desde));
        }

        private double generarNumeroExponencial(double media, double random)
        {
            double lambda = 1 / media;
            double log = Math.Log(1 - random);
            return (-1 / lambda) * log;
        }

        private double getMillisecondsFromMinutes(double minutes)
        {
            return minutes * 60000;
        }

        private void simulacion_Click(object sender, EventArgs e)
        {
            if (validarIngresoDatos())
            {
                // Limpiar tablas
                dgvSimulacion.Rows.Clear();

                Random random = new Random();

                // Tomar datos
                double cantidadLlegadaClientesPorHora = 60/Convert.ToDouble(txtLlegadaClientes.Text);
                double minutosPromedioAtencionCliente = Convert.ToDouble(txtTiempoAtencion.Text);
                double segundosPromedioAtencionCliente = Convert.ToDouble(txtSegundosTiempoAtencion.Text);
                double tiempoPromedioAtencionCliente = minutosPromedioAtencionCliente + (segundosPromedioAtencionCliente / 60) ;
                double tiempoTurnoEmpleado = Convert.ToDouble(txtTiempoTurnoEmpleado.Text);
                double tiempoCambioTurnoA = Convert.ToDouble(txtTiempoCambioTurnoA.Text);
                double tiempoCambioTurnoB = Convert.ToDouble(txtTiempoCambioTurnoB.Text);

                if (tiempoCambioTurnoA > tiempoCambioTurnoB)
                {
                    double a = tiempoCambioTurnoA;
                    double b = tiempoCambioTurnoB;
                    tiempoCambioTurnoB = a;
                    tiempoCambioTurnoA = b;
                }

                double tiempoSimulacion = Convert.ToDouble(txtTiempoSimulacion.Text);

                double tiempoDesdeSimulacion = Convert.ToDouble(txtTiempoDesdeSimulacion.Text);
                double tiempoHastaSimulacion = Convert.ToDouble(txtTiempoHastaSimulacion.Text);

                TimeSpan tiempoDesde = new TimeSpan(0, Convert.ToInt32(tiempoDesdeSimulacion), 0);
                TimeSpan tiempoHasta = new TimeSpan(0, Convert.ToInt32(tiempoHastaSimulacion), 0);

                TimeSpan relojSimulacion = new TimeSpan(0,0,0);
                TimeSpan tiempoASimular = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(getMillisecondsFromMinutes(tiempoSimulacion)));

                string eventoActual = "";
                string proximoEvento = "";

                // 1 hora = 3.600.000 milisegundos
                double milisegundosXHora = 3600000;
                double cantidadCambiosEmpleado = Math.Floor((tiempoASimular.TotalMilliseconds / milisegundosXHora) / tiempoTurnoEmpleado);

                Queue<EventoCambioEmpleado> colaEventoCambioEmpleado = new Queue<EventoCambioEmpleado>();

                bool nuevaVentanilla = checkNuevoServidor.Checked;

                if (nuevaVentanilla)
                {
                    dgvSimulacion.Columns["RNDAtencion2"].Visible = true;
                    dgvSimulacion.Columns["TiempoAtencion2"].Visible = true;
                    dgvSimulacion.Columns["ProximoFinAtencion2"].Visible = true;
                }

                for (double i = 1; i <= cantidadCambiosEmpleado; i++)
                {
                    TimeSpan horaCambioEmpleado = new TimeSpan(Convert.ToInt32(i * tiempoTurnoEmpleado), 0, 0);
                    EventoCambioEmpleado cambioEmpleado = new EventoCambioEmpleado(horaCambioEmpleado);
                    colaEventoCambioEmpleado.Enqueue(cambioEmpleado);
                }

                Cliente clienteEnAtencion = new Cliente();
                Cliente clienteEnAtencionServidor2 = new Cliente();
                Queue<Cliente> colaAtencion = new Queue<Cliente>();
                int colaMaxima = 0;
                int contadorNumeroCliente = 1;

                TimeSpan proximaLlegadaClienteAcumulador = new TimeSpan(0, 0, 0);
                TimeSpan proximoFinAtencionAcumulador = new TimeSpan(0, 0, 0);
                TimeSpan tiempoPromedioEsperaClienteAcumulador = new TimeSpan(0, 0, 0);
                int cantidadClientesQueEsperaron = 0;

                TimeSpan proximoResumenAtencionAcumulador = new TimeSpan(0, 0, 0);

                while (relojSimulacion < tiempoASimular)
                {
                    if (relojSimulacion.TotalMilliseconds == 0)
                    {
                        eventoActual = "Inicio";

                        double rnd = random.NextDouble();

                        double llegadaCliente = generarNumeroExponencial(cantidadLlegadaClientesPorHora, rnd);

                        TimeSpan proximaLlegadaCliente = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(getMillisecondsFromMinutes(llegadaCliente)));
                        proximaLlegadaClienteAcumulador = relojSimulacion + proximaLlegadaCliente;

                        if (relojSimulacion >= tiempoDesde && relojSimulacion <= tiempoHasta)
                        {
                            int idx = dgvSimulacion.Rows.Add();
                            DataGridViewRow row = dgvSimulacion.Rows[idx];

                            row.Cells["Reloj"].Value = relojSimulacion;
                            row.Cells["Evento"].Value = eventoActual;
                            row.Cells["RNDLlegada"].Value = Math.Round(rnd,4);
                            row.Cells["TiempoLlegadaCliente"].Value = getLocalHour(proximaLlegadaCliente);
                            row.Cells["ProximaLlegadaCliente"].Value = getLocalHour(proximaLlegadaClienteAcumulador);
                        }
                        
                        relojSimulacion = proximaLlegadaClienteAcumulador;
                        proximoEvento = "Llegada cliente";

                    } else
                    {

                        if ((colaEventoCambioEmpleado.Count == 0) || (relojSimulacion < colaEventoCambioEmpleado.First().getHoraInicio()))
                        {
                            if (proximoEvento == "Llegada cliente")
                            {
                                Cliente cliente = new Cliente(contadorNumeroCliente);

                                eventoActual = "Llega " + cliente.toString();

                                contadorNumeroCliente++;

                                double rnd = random.NextDouble();
                                double llegadaCliente = generarNumeroExponencial(cantidadLlegadaClientesPorHora, rnd);
                                TimeSpan proximaLlegadaCliente = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(getMillisecondsFromMinutes(llegadaCliente)));
                                proximaLlegadaClienteAcumulador = relojSimulacion + proximaLlegadaCliente;
                            
                                if (colaAtencion.Count == 0 && (clienteEnAtencion.getId() == -1 || (nuevaVentanilla && clienteEnAtencionServidor2.getId() == -1)))
                                {
                                    if (relojSimulacion < proximoResumenAtencionAcumulador)
                                    {
                                        // No puedo atender hasta que se resuma la atencion
                                        // Cliente ingresa a la cola

                                        cliente.setHoraIngresoCola(relojSimulacion);
                                        colaAtencion.Enqueue(cliente);

                                        if (colaAtencion.Count > colaMaxima)
                                        {
                                            colaMaxima = colaAtencion.Count;
                                        }

                                        if (relojSimulacion >= tiempoDesde && relojSimulacion <= tiempoHasta)
                                        {
                                            int idx = dgvSimulacion.Rows.Add();
                                            DataGridViewRow row = dgvSimulacion.Rows[idx];

                                            row.Cells["Reloj"].Value = getLocalHour(relojSimulacion);
                                            row.Cells["Evento"].Value = eventoActual;
                                            row.Cells["RNDLlegada"].Value = Math.Round(rnd,4);
                                            row.Cells["TiempoLlegadaCliente"].Value = getLocalHour(proximaLlegadaCliente);
                                            row.Cells["ProximaLlegadaCliente"].Value = getLocalHour(proximaLlegadaClienteAcumulador);
                                            row.Cells["ColaAtencion"].Value = colaAtencion.Count;
                                            row.Cells["ColaMaxima"].Value = colaMaxima;
                                        }

                                        if (proximaLlegadaClienteAcumulador < proximoResumenAtencionAcumulador)
                                        {
                                            relojSimulacion = proximaLlegadaClienteAcumulador;
                                            proximoEvento = "Llegada cliente";
                                        }
                                        else
                                        {
                                            proximoEvento = "Reanudación atención";
                                            relojSimulacion = proximoResumenAtencionAcumulador;
                                        }

                                    }
                                    else
                                    {
                                        // Inicio atencion

                                        double rndAtencion = random.NextDouble();
                                        double atencion = generarNumeroExponencial(tiempoPromedioAtencionCliente, rndAtencion);
                                        TimeSpan tiempoAtencionCliente = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(getMillisecondsFromMinutes(atencion)));
                                        proximoFinAtencionAcumulador = relojSimulacion + tiempoAtencionCliente;

                                        bool entroEnServidor1 = false;
                                        bool entroEnServidor2 = false;

                                        if (nuevaVentanilla)
                                        {
                                            if (clienteEnAtencion.getId() == -1)
                                            {
                                                clienteEnAtencion = cliente;
                                                clienteEnAtencion.setHoraFinAtencion(proximoFinAtencionAcumulador);
                                                entroEnServidor1 = true;
                                            }
                                            else
                                            {
                                                if (clienteEnAtencionServidor2.getId() == -1)
                                                {
                                                    clienteEnAtencionServidor2 = cliente;
                                                    clienteEnAtencionServidor2.setHoraFinAtencion(proximoFinAtencionAcumulador);
                                                    entroEnServidor2 = true;
                                                }
                                            }
                                        } else
                                        {
                                            clienteEnAtencion = cliente;
                                            clienteEnAtencion.setHoraFinAtencion(proximoFinAtencionAcumulador);
                                        }

                                        if (relojSimulacion >= tiempoDesde && relojSimulacion <= tiempoHasta)
                                        {
                                            int idx = dgvSimulacion.Rows.Add();
                                            DataGridViewRow row = dgvSimulacion.Rows[idx];

                                            row.Cells["Reloj"].Value = getLocalHour(relojSimulacion);
                                            row.Cells["Evento"].Value = eventoActual;
                                            row.Cells["RNDLlegada"].Value = Math.Round(rnd,4);
                                            row.Cells["TiempoLlegadaCliente"].Value = getLocalHour(proximaLlegadaCliente);
                                            row.Cells["ProximaLlegadaCliente"].Value = getLocalHour(proximaLlegadaClienteAcumulador);
                                            row.Cells["ColaAtencion"].Value = colaAtencion.Count;
                                            if (nuevaVentanilla)
                                            {
                                                if (entroEnServidor1)
                                                {
                                                    row.Cells["RNDAtencion"].Value = rndAtencion;
                                                    row.Cells["TiempoAtencion"].Value = getLocalHour(tiempoAtencionCliente);
                                                    row.Cells["ProximoFinAtencion"].Value = getLocalHour(proximoFinAtencionAcumulador);
                                                }
                                                else
                                                {
                                                    if (entroEnServidor2)
                                                    {
                                                        row.Cells["RNDAtencion2"].Value = rndAtencion;
                                                        row.Cells["TiempoAtencion2"].Value = getLocalHour(tiempoAtencionCliente);
                                                        row.Cells["ProximoFinAtencion2"].Value = getLocalHour(proximoFinAtencionAcumulador);
                                                    }
                                                }
                                            } else
                                            {
                                                row.Cells["ClienteEnAtencion"].Value = clienteEnAtencion.toString();
                                                row.Cells["RNDAtencion"].Value = Math.Round(rndAtencion,4);
                                                row.Cells["TiempoAtencion"].Value = getLocalHour(tiempoAtencionCliente);
                                                row.Cells["ProximoFinAtencion"].Value = getLocalHour(proximoFinAtencionAcumulador);
                                            }
                                            row.Cells["ColaMaxima"].Value = colaMaxima;
                                        }

                                        if (proximaLlegadaClienteAcumulador < proximoFinAtencionAcumulador)
                                        {
                                            relojSimulacion = proximaLlegadaClienteAcumulador;
                                            proximoEvento = "Llegada cliente";
                                        }
                                        else
                                        {
                                            relojSimulacion = proximoFinAtencionAcumulador;
                                            proximoEvento = "Fin de atencion cliente";
                                        }
                                    }
                                }
                            else
                            {
                               // Hay personas en la cola ó se esta atendiendo un cliente

                                    cliente.setHoraIngresoCola(relojSimulacion);
                                    colaAtencion.Enqueue(cliente);

                                    if (colaAtencion.Count > colaMaxima)
                                    {
                                        colaMaxima = colaAtencion.Count;
                                    }
                                    if (relojSimulacion >= tiempoDesde && relojSimulacion <= tiempoHasta)
                                    {

                                        int idx = dgvSimulacion.Rows.Add();
                                        DataGridViewRow row = dgvSimulacion.Rows[idx];

                                        row.Cells["Reloj"].Value = getLocalHour(relojSimulacion);
                                        row.Cells["Evento"].Value = eventoActual;
                                        row.Cells["RNDLlegada"].Value = Math.Round(rnd,4);
                                        row.Cells["TiempoLlegadaCliente"].Value = getLocalHour(proximaLlegadaCliente);
                                        row.Cells["ProximaLlegadaCliente"].Value = getLocalHour(proximaLlegadaClienteAcumulador);
                                        row.Cells["ColaAtencion"].Value = colaAtencion.Count;
                                        row.Cells["ColaMaxima"].Value = colaMaxima;
                                    }
                                    if (proximoResumenAtencionAcumulador.TotalMilliseconds == 0)
                                    {
                                        if (proximaLlegadaClienteAcumulador < proximoFinAtencionAcumulador)
                                        {
                                            relojSimulacion = proximaLlegadaClienteAcumulador;
                                            proximoEvento = "Llegada cliente";
                                        }
                                        else
                                        {
                                            relojSimulacion = proximoFinAtencionAcumulador;
                                            proximoEvento = "Fin de atencion cliente";
                                        }
                                    } else
                                    {
                                        if (proximaLlegadaClienteAcumulador < proximoResumenAtencionAcumulador)
                                        {
                                            relojSimulacion = proximaLlegadaClienteAcumulador;
                                            proximoEvento = "Llegada cliente";
                                        }
                                        else
                                        {
                                            proximoEvento = "Reanudación atención";
                                            relojSimulacion = proximoResumenAtencionAcumulador;
                                        }

                                        
                                    }
                                    
                            }
                        }
                            else if (proximoEvento == "Fin de atencion cliente")
                            {
                                bool dejaLibreServidor1 = false;
                                bool dejaLibreServidor2 = false;

                                if (relojSimulacion < proximoResumenAtencionAcumulador)
                                {
                                    // No puedo terminar la atencion hasta que se resuma la atencion
                                    // (En realidad lo que hago es que si el fin de atencion supera al fin de turno, no lo atiendo y espera al prox empleado)

                                    if (proximaLlegadaClienteAcumulador < proximoResumenAtencionAcumulador)
                                    {
                                        relojSimulacion = proximaLlegadaClienteAcumulador;
                                        proximoEvento = "Llegada cliente";
                                    }
                                    else
                                    {
                                        proximoEvento = "Reanudación atención";
                                        relojSimulacion = proximoResumenAtencionAcumulador;
                                    }
                                }
                                else
                                {
                                    if (nuevaVentanilla)
                                    {
                                        if (clienteEnAtencion.getId() != -1 && clienteEnAtencionServidor2.getId() != -1)
                                        {
                                            if (clienteEnAtencion.getHoraFinAtencion() < clienteEnAtencionServidor2.getHoraFinAtencion())
                                            {
                                                eventoActual = "Fin atencion " + clienteEnAtencion.toString();
                                                dejaLibreServidor1 = true;
                                            }
                                            else
                                            {
                                                eventoActual = "Fin atencion " + clienteEnAtencionServidor2.toString();
                                                dejaLibreServidor2 = true;
                                            }
                                        } else
                                        {
                                            if (clienteEnAtencion.getId() == -1)
                                            {
                                                eventoActual = "Fin atencion " + clienteEnAtencionServidor2.toString();
                                                dejaLibreServidor2 = true;

                                            } else if (clienteEnAtencionServidor2.getId() == -1)
                                            {
                                                eventoActual = "Fin atencion " + clienteEnAtencion.toString();
                                                dejaLibreServidor1 = true;
                                            }
                                        }

                                    } else
                                    {
                                        eventoActual = "Fin atencion " + clienteEnAtencion.toString();
                                    }
                                    
                                    if (colaAtencion.Count > 0)
                                    {
                                        Cliente clienteAAtender = colaAtencion.Peek();

                                        clienteAAtender.setHoraInicioAtencion(relojSimulacion);

                                        TimeSpan tiempoEsperaCliente = new TimeSpan(0, 0, 0);

                                        if (nuevaVentanilla)
                                        {
                                            if (dejaLibreServidor1)
                                            {
                                                clienteEnAtencion = clienteAAtender;
                                                tiempoEsperaCliente = clienteEnAtencion.getHoraInicioAtencion() - clienteEnAtencion.getHoraIngesoCola();
                                                //clienteEnAtencion.setHoraFinAtencion(proximoFinAtencionAcumulador);
                                            }
                                            else
                                            {
                                                if (dejaLibreServidor2)
                                                {
                                                    clienteEnAtencionServidor2 = clienteAAtender;
                                                    tiempoEsperaCliente = clienteEnAtencionServidor2.getHoraInicioAtencion() - clienteEnAtencionServidor2.getHoraIngesoCola();
                                                    //clienteEnAtencionServidor2.setHoraFinAtencion(proximoFinAtencionAcumulador);
                                                }
                                            }
                                        } else
                                        {
                                            clienteEnAtencion = clienteAAtender;
                                            tiempoEsperaCliente = clienteEnAtencion.getHoraInicioAtencion() - clienteEnAtencion.getHoraIngesoCola();
                                        }
                                        
                                        //clienteEnAtencion = clienteAAtender;

                                        //TimeSpan tiempoEsperaCliente = clienteEnAtencion.getHoraInicioAtencion() - clienteEnAtencion.getHoraIngesoCola();
                                        TimeSpan tiempoPromedioEsperaCliente = new TimeSpan(0, 0, 0);
                                        cantidadClientesQueEsperaron++;

                                        if (tiempoPromedioEsperaClienteAcumulador.TotalMilliseconds == 0)
                                        {
                                            tiempoPromedioEsperaClienteAcumulador = tiempoEsperaCliente;
                                            tiempoPromedioEsperaCliente = tiempoEsperaCliente;
                                        }
                                        else
                                        {
                                            tiempoPromedioEsperaCliente = ((tiempoPromedioEsperaClienteAcumulador * (cantidadClientesQueEsperaron - 1)) + tiempoEsperaCliente) / cantidadClientesQueEsperaron;
                                            tiempoPromedioEsperaClienteAcumulador = tiempoPromedioEsperaCliente;
                                        }

                                        double rndAtencion = random.NextDouble();
                                        double atencion = generarNumeroExponencial(tiempoPromedioAtencionCliente, rndAtencion);
                                        TimeSpan tiempoAtencionCliente = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(getMillisecondsFromMinutes(atencion)));
                                        proximoFinAtencionAcumulador = relojSimulacion + tiempoAtencionCliente;

                                        int idx;
                                        DataGridViewRow row = dgvSimulacion.Rows[0];

                                        if (relojSimulacion >= tiempoDesde && relojSimulacion <= tiempoHasta)
                                        {
                                            idx = dgvSimulacion.Rows.Add();
                                            row = dgvSimulacion.Rows[idx];

                                            row.Cells["Reloj"].Value = getLocalHour(relojSimulacion);
                                            row.Cells["Evento"].Value = eventoActual;
                                        }
                                        // ...

                                        if ((colaEventoCambioEmpleado.Count == 0) || proximoFinAtencionAcumulador < colaEventoCambioEmpleado.First().getHoraInicio())
                                        {
                                            if (relojSimulacion >= tiempoDesde && relojSimulacion <= tiempoHasta)
                                            {
                                                colaAtencion.Dequeue();
                                                row.Cells["ColaAtencion"].Value = colaAtencion.Count;
                                                if (nuevaVentanilla)
                                                {
                                                    if (dejaLibreServidor1)
                                                    {
                                                        row.Cells["RNDAtencion"].Value = rndAtencion;
                                                        row.Cells["TiempoAtencion"].Value = getLocalHour(tiempoAtencionCliente);
                                                        row.Cells["ProximoFinAtencion"].Value = getLocalHour(proximoFinAtencionAcumulador);
                                                    } else if (dejaLibreServidor2)
                                                    {
                                                        row.Cells["RNDAtencion2"].Value = rndAtencion;
                                                        row.Cells["TiempoAtencion2"].Value = getLocalHour(tiempoAtencionCliente);
                                                        row.Cells["ProximoFinAtencion2"].Value = getLocalHour(proximoFinAtencionAcumulador);
                                                    }

                                                } else
                                                {
                                                    row.Cells["ClienteEnAtencion"].Value = clienteEnAtencion.toString();
                                                    row.Cells["RNDAtencion"].Value = Math.Round(rndAtencion,4);
                                                    row.Cells["TiempoAtencion"].Value = getLocalHour(tiempoAtencionCliente);
                                                    row.Cells["ProximoFinAtencion"].Value = getLocalHour(proximoFinAtencionAcumulador);
                                                }
                                               
                                                row.Cells["ColaMaxima"].Value = colaMaxima;
                                                row.Cells["TiempoEsperaCliente"].Value = getLocalHour(tiempoEsperaCliente);
                                                row.Cells["PromedioEsperaClientes"].Value = getLocalHour(tiempoPromedioEsperaCliente);
                                            }
                                        } else
                                        {
                                            if (relojSimulacion >= tiempoDesde && relojSimulacion <= tiempoHasta)
                                            {
                                                row.Cells["ColaAtencion"].Value = colaAtencion.Count;
                                                row.Cells["ColaMaxima"].Value = colaMaxima;
                                            }
                                        }

                                        if (proximaLlegadaClienteAcumulador < proximoFinAtencionAcumulador)
                                        {
                                            relojSimulacion = proximaLlegadaClienteAcumulador;
                                            proximoEvento = "Llegada cliente";
                                        }
                                        else
                                        {
                                            relojSimulacion = proximoFinAtencionAcumulador;
                                            proximoEvento = "Fin de atencion cliente";
                                        }

                                    }
                                    else
                                    {
                                        if (nuevaVentanilla)
                                        {
                                            if (dejaLibreServidor1)
                                            {
                                                clienteEnAtencion = new Cliente();
                                            }
                                            else if (dejaLibreServidor2)
                                            {
                                                clienteEnAtencionServidor2 = new Cliente();
                                            }
                                        } else
                                        { 
                                            clienteEnAtencion = new Cliente();
                                        }
                                        
                                        if (relojSimulacion >= tiempoDesde && relojSimulacion <= tiempoHasta)
                                        {
                                            int idx = dgvSimulacion.Rows.Add();
                                            DataGridViewRow row = dgvSimulacion.Rows[idx];

                                            row.Cells["Reloj"].Value = getLocalHour(relojSimulacion);
                                            row.Cells["Evento"].Value = eventoActual;
                                        }
                                        relojSimulacion = proximaLlegadaClienteAcumulador;
                                        proximoEvento = "Llegada cliente";

                                        if (relojSimulacion > tiempoASimular)
                                        {
                                            int idx = dgvSimulacion.Rows.Add();
                                            DataGridViewRow row = dgvSimulacion.Rows[idx];
                                            row.Cells["Reloj"].Value = getLocalHour(relojSimulacion);
                                            row.Cells["Evento"].Value = eventoActual;
                                        }
                                        }
                                }
                            }
                            else if (proximoEvento == "Reanudación atención")
                            {
                                eventoActual = "Reanudación atención";

                                proximoResumenAtencionAcumulador = new TimeSpan(0, 0, 0);

                                if (colaAtencion.Count > 0)
                                {
                                    // No tomar un cliente nuevo de la cola, sino seguir atendiendo al que estaba
                                    // En realidad, si el fin de atencion de un cliente supera al cambio de turno, que siga esperando y ahora lo tomo de la cola

                                    Cliente clienteAAtender = colaAtencion.Dequeue();
                                    clienteAAtender.setHoraInicioAtencion(relojSimulacion);
                                    TimeSpan tiempoEsperaCliente = new TimeSpan(0,0,0);

                                    if (nuevaVentanilla)
                                    {
                                        if (clienteEnAtencion.getId() == -1)
                                        {
                                            clienteEnAtencion = clienteAAtender;
                                            tiempoEsperaCliente = clienteEnAtencion.getHoraInicioAtencion() - clienteEnAtencion.getHoraIngesoCola();
                                            //clienteEnAtencion.setHoraFinAtencion(proximoFinAtencionAcumulador);

                                        }
                                        else
                                        {
                                            if (clienteEnAtencionServidor2.getId() == -1)
                                            {
                                                clienteEnAtencionServidor2 = clienteAAtender;
                                                tiempoEsperaCliente = clienteEnAtencionServidor2.getHoraInicioAtencion() - clienteEnAtencionServidor2.getHoraIngesoCola();
                                                //clienteEnAtencionServidor2.setHoraFinAtencion(proximoFinAtencionAcumulador);
                                            }
                                        }
                                    } else
                                    {
                                        clienteEnAtencion = clienteAAtender;
                                        tiempoEsperaCliente = clienteEnAtencion.getHoraInicioAtencion() - clienteEnAtencion.getHoraIngesoCola();
                                    }
                                    //clienteEnAtencion = clienteAAtender;

                                    //TimeSpan tiempoEsperaCliente = clienteEnAtencion.getHoraInicioAtencion() - clienteEnAtencion.getHoraIngesoCola();
                                    TimeSpan tiempoPromedioEsperaCliente = new TimeSpan(0, 0, 0);
                                    cantidadClientesQueEsperaron++;

                                    if (tiempoPromedioEsperaClienteAcumulador.TotalMilliseconds == 0)
                                    {
                                        tiempoPromedioEsperaClienteAcumulador = tiempoEsperaCliente;
                                        tiempoPromedioEsperaCliente = tiempoEsperaCliente;
                                    }
                                    else
                                    {
                                        tiempoPromedioEsperaCliente = ((tiempoPromedioEsperaClienteAcumulador * (cantidadClientesQueEsperaron - 1)) + tiempoEsperaCliente) / cantidadClientesQueEsperaron;
                                        tiempoPromedioEsperaClienteAcumulador = tiempoPromedioEsperaCliente;
                                    }

                                    double rndAtencion = random.NextDouble();
                                    double atencion = generarNumeroExponencial(tiempoPromedioAtencionCliente, rndAtencion);
                                    TimeSpan tiempoAtencionCliente = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(getMillisecondsFromMinutes(atencion)));
                                    proximoFinAtencionAcumulador = relojSimulacion + tiempoAtencionCliente;
                                    if (relojSimulacion >= tiempoDesde && relojSimulacion <= tiempoHasta)
                                    {
                                        int idx = dgvSimulacion.Rows.Add();
                                        DataGridViewRow row = dgvSimulacion.Rows[idx];

                                        row.Cells["Reloj"].Value = getLocalHour(relojSimulacion);
                                        row.Cells["Evento"].Value = eventoActual;
                                        // ...
                                        row.Cells["ColaAtencion"].Value = colaAtencion.Count;
                                        row.Cells["ClienteEnAtencion"].Value = clienteEnAtencion.toString();
                                        row.Cells["RNDAtencion"].Value = Math.Round(rndAtencion,4);
                                        row.Cells["TiempoAtencion"].Value = getLocalHour(tiempoAtencionCliente);
                                        row.Cells["ProximoFinAtencion"].Value = getLocalHour(proximoFinAtencionAcumulador);
                                        row.Cells["ColaMaxima"].Value = colaMaxima;
                                        row.Cells["TiempoEsperaCliente"].Value = getLocalHour(tiempoEsperaCliente);
                                        row.Cells["PromedioEsperaClientes"].Value = getLocalHour(tiempoPromedioEsperaCliente);
                                    }
                                    if (proximaLlegadaClienteAcumulador < proximoFinAtencionAcumulador)
                                    {
                                        relojSimulacion = proximaLlegadaClienteAcumulador;
                                        proximoEvento = "Llegada cliente";

                                    }
                                    else
                                    {
                                        relojSimulacion = proximoFinAtencionAcumulador;
                                        proximoEvento = "Fin de atencion cliente";
                                    }

                                    } else
                                {
                                    // Resumo atencion y no hay nadie en cola
                                    // Que llegue el proximo cliente
                                    relojSimulacion = proximaLlegadaClienteAcumulador;
                                    proximoEvento = "Llegada cliente";

                                }
                            }
                        } else
                        {
                            // El proximo evento es el cambio de empleado
                            // Dejo de atender hasta que termine

                            eventoActual = "Cambio empleado";

                            EventoCambioEmpleado eventoCambioEmpleado = colaEventoCambioEmpleado.Dequeue();

                            double rndCambioEmpleado = random.NextDouble();

                            double cambioEmpleado = generarNumeroUniforme(tiempoCambioTurnoA, tiempoCambioTurnoB, rndCambioEmpleado);

                            TimeSpan tiempoCambioEmpleado = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(getMillisecondsFromMinutes(cambioEmpleado)));

                            proximoResumenAtencionAcumulador = relojSimulacion + tiempoCambioEmpleado;
                            if (relojSimulacion >= tiempoDesde && relojSimulacion <= tiempoHasta)
                            {
                                int idx = dgvSimulacion.Rows.Add();
                                DataGridViewRow row = dgvSimulacion.Rows[idx];

                                row.Cells["Reloj"].Value = getLocalHour(eventoCambioEmpleado.getHoraInicio());
                                row.Cells["Evento"].Value = eventoActual;
                                row.Cells["RNDCambioEmpleado"].Value = Math.Round(rndCambioEmpleado,4);
                                row.Cells["TiempoCambioEmpleado"].Value = getLocalHour(tiempoCambioEmpleado);
                                row.Cells["ProximoResumenAtencion"].Value = getLocalHour(proximoResumenAtencionAcumulador);
                            }

                            if (proximoResumenAtencionAcumulador < relojSimulacion )
                            {
                                proximoEvento = "Reanudación atención";
                                relojSimulacion = proximoResumenAtencionAcumulador;
                            }
                            }
                    }
                }

                if (relojSimulacion >= tiempoASimular)
                {
                    relojSimulacion = tiempoASimular;
                    eventoActual = "Fin de simulación";
                    int idx = dgvSimulacion.Rows.Add();
                    DataGridViewRow row = dgvSimulacion.Rows[idx];
                    row.Cells["Reloj"].Value = getLocalHour(relojSimulacion);
                    row.Cells["Evento"].Value = eventoActual;
                    row.Cells["ColaAtencion"].Value = colaAtencion.Count;
                    row.Cells["ColaMaxima"].Value = colaMaxima;
                    row.Cells["PromedioEsperaClientes"].Value = getLocalHour(tiempoPromedioEsperaClienteAcumulador);
                }

            } else
            {
                MessageBox.Show("Por favor, ingrese todos los datos correctamente", "Atencion", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private bool validarIngresoDatos()
        {
            if (txtLlegadaClientes.Text == "" ||
                txtTiempoAtencion.Text == "" ||
                txtSegundosTiempoAtencion.Text == "" ||
                txtTiempoTurnoEmpleado.Text == "" ||
                txtTiempoCambioTurnoA.Text == "" ||
                txtTiempoCambioTurnoB.Text == "" ||
                txtTiempoSimulacion.Text == "")
            {
                return false;
            }
            else
            {
                if (double.TryParse(txtLlegadaClientes.Text, out _) &&
                    double.TryParse(txtTiempoAtencion.Text, out _) &&
                    double.TryParse(txtSegundosTiempoAtencion.Text, out _) &&
                    double.TryParse(txtTiempoTurnoEmpleado.Text, out _) &&
                    double.TryParse(txtTiempoCambioTurnoA.Text, out _) &&
                    double.TryParse(txtTiempoCambioTurnoB.Text, out _) &&
                    double.TryParse(txtTiempoSimulacion.Text, out _))
                {
                    if (Convert.ToInt32(txtLlegadaClientes.Text) < 0 ||
                        Convert.ToInt32(txtTiempoAtencion.Text) < 0 ||
                        Convert.ToInt32(txtSegundosTiempoAtencion.Text) < 0 ||
                        Convert.ToInt32(txtTiempoTurnoEmpleado.Text) < 1 ||
                        Convert.ToInt32(txtTiempoCambioTurnoA.Text) < 0 ||
                        Convert.ToInt32(txtTiempoCambioTurnoB.Text) < 0 ||
                        Convert.ToInt32(txtTiempoSimulacion.Text) < 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                } else
                {
                    return false;
                }

                      
            }
        }

        public class EventoCambioEmpleado
        {
            private TimeSpan horaInicio { get; set; }

            public EventoCambioEmpleado(TimeSpan hora)
            {
                this.horaInicio = hora;
            }

            public TimeSpan getHoraInicio()
            {
                return this.horaInicio;
            }

            public void setHoraInicio(TimeSpan hora)
            {
                this.horaInicio = hora;
            }
        }

        public class Cliente
        {
            private int id { get; set; }

            private TimeSpan ingresoACola { get; set; }

            private TimeSpan inicioAtencion { get; set; }

            private TimeSpan finAtencion { get; set; }

            public Cliente() 
            {
                this.id = -1;
            }    

            public Cliente(int id)
            {
                this.id = id;
            }

            public int getId()
            {
                return this.id;
            }

            public TimeSpan getHoraIngesoCola()
            {
                return this.ingresoACola;
            }

            public TimeSpan getHoraInicioAtencion()
            {
                return this.inicioAtencion;
            }
            public TimeSpan getHoraFinAtencion()
            {
                return this.finAtencion;
            }

            public void setHoraIngresoCola(TimeSpan hora)
            {
                this.ingresoACola = hora;
            }

            public void setHoraInicioAtencion(TimeSpan hora)
            {
                this.inicioAtencion = hora;
            }

            public void setHoraFinAtencion(TimeSpan hora)
            {
                this.finAtencion = hora;
            }

            public string toString()
            {
                return "Cliente " + this.id;
            }
        }
        public string getLocalHour(TimeSpan time)

        {
            string days = "";
            string hourString = "";
            string minuteString = "";
            string secondString = "";

            if (time.Days != 0)
            {
                days = "Dia: " + time.Days.ToString() + " ";
            }
           
            if (Convert.ToString(time.Hours).Length == 1)
            {
                hourString = "0" + time.Hours;
            }
            else
            {
                hourString = time.Hours.ToString();
            }

            if (Convert.ToString(time.Minutes).Length == 1)
            {
                minuteString = "0" + time.Minutes;
            }
            else
            {
                minuteString = time.Minutes.ToString();
            }

            if (Convert.ToString(time.Seconds).Length == 1)
            {
                secondString = "0" + time.Seconds;
            }
            else
            {
                secondString = time.Seconds.ToString();
            }

            return  days + hourString + ":" + minuteString + ":" + secondString;
        }
    }
}
