using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Threading;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Dsp;
using System.Diagnostics;

namespace DetectorDispositivos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //El programa te lee los microfonos que tiene la computadora y te los pone en el combobox y se conecta con el programa que hicimos en clase de microfono
        //Solo que no sé como validar que si esta agarrando ese microfono

        WaveIn waveIn; //Conexion con microfono
        WaveFormat formato; //Formato de audio
        public MainWindow()
        {
            InitializeComponent();
            cbDispositivosEntrada();
        }

        public void cbDispositivosEntrada()
        {
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                WaveInCapabilities capacidades = WaveIn.GetCapabilities(i);
                cbMicrofonos.Items.Add(capacidades.ProductName);
            }
            cbMicrofonos.SelectedIndex = 0;
        }

        private void cbMicrofonos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnIniciar_Click_1(object sender, RoutedEventArgs e)
        {
            //Inicializar la conexion
            waveIn = new WaveIn();

            //Establecer el formato
            waveIn.WaveFormat =
                new WaveFormat(44100, 16, 1);
            formato = waveIn.WaveFormat;

            //Duracion del buffer
            waveIn.BufferMilliseconds = 500;

            //Con que funcion respondemos
            //cuando se llena el buffer
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveIn.StartRecording();
        }
        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesGrabados = e.BytesRecorded;

            int numMuestras = bytesGrabados / 2;

            int exponente = 0;
            int numeroBits = 0;

            do
            {
                exponente++;
                numeroBits = (int)
                    Math.Pow(2, exponente);
            } while (numeroBits < numMuestras);
            exponente -= 1;

            Complex[] muestrasComplejas =
                new Complex[numeroBits];

            for (int i = 0; i < bytesGrabados; i += 2)
            {
                short muestra =
                    (short)(buffer[i + 1] << 8 | buffer[i]);
                float muestra32bits =
                    (float)muestra / 32768.0f;
                if (i / 2 < numeroBits)
                {
                    muestrasComplejas[i / 2].X = muestra32bits;
                }
            }

            FastFourierTransform.FFT(true,
                exponente, muestrasComplejas);

            float[] valoresAbsolutos =
                new float[muestrasComplejas.Length];

            for (int i = 0; i < muestrasComplejas.Length;
                i++)
            {
                valoresAbsolutos[i] = (float)
                    Math.Sqrt(
                    (muestrasComplejas[i].X * muestrasComplejas[i].Y) +
                    (muestrasComplejas[i].X * muestrasComplejas[i].Y));
            }

            int indiceValorMaximo =
                valoresAbsolutos.ToList().IndexOf(
                    valoresAbsolutos.Max());

            float frecuenciaFundamental =
                (float)(indiceValorMaximo * formato.SampleRate)
                / (float)valoresAbsolutos.Length;

            lblHertz.Text =
                frecuenciaFundamental.ToString("n") + "Hz";
        }

        private void btnDetener_Click(object sender, RoutedEventArgs e)
        {
            waveIn.StopRecording();
        }
    }
}

