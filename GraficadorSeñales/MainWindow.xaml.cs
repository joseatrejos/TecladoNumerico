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
using Microsoft.Win32;
using NAudio.Wave;
using System.Linq;

namespace GraficadorSeñales
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Variables Globales
        double amplitudMaxima = 1;
        Señal señal;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BotonGraficar_Click(object sender, RoutedEventArgs e)
        {
            var reader = new AudioFileReader(txt_RutaArchivo.Text);

            double tiempoInicial = 0;
            double tiempoFinal = reader.TotalTime.TotalSeconds;
            double frecuenciaMuestreo = reader.WaveFormat.SampleRate;
            
            txt_TiempoInicial.Text = "0";
            txt_TiempoFinal.Text = tiempoFinal.ToString();
            txt_FrecuenciaDeMuestreo.Text = frecuenciaMuestreo.ToString();

            señal = new SeñalPersonalizada();

            // Primer Señal
            señal.TiempoInicial = tiempoInicial;
            señal.TiempoFinal = tiempoFinal;
            señal.FrecuenciaMuestreo = frecuenciaMuestreo;

            // Construir nuestra señal a trávés del archivo de audio
            var bufferLectura = new float[reader.WaveFormat.Channels];
            int muestrasLeidas = 1;
            double instanteActual = 0;
            double intervaloMuestra = 1.0 / frecuenciaMuestreo;

            do
            {
                muestrasLeidas = reader.Read(bufferLectura, 0, reader.WaveFormat.Channels);
                if(muestrasLeidas > 0)
                {
                    double max = bufferLectura.Take(muestrasLeidas).Max();
                    señal.Muestras.Add(new Muestra(instanteActual, max));
                }
                instanteActual += intervaloMuestra;
            } while (muestrasLeidas > 0);
            
            // Actualizar
            señal.actualizarAmplitudMaxima();

            // Definición de la amplitud máxima en función de la señal de mayor amplitud
            amplitudMaxima = señal.AmplitudMaxima;

            // Limpieza de polylines
            plnGrafica.Points.Clear();

            // Impresión de la amplitud máxima en los labels de la ventana.
            lbl_AmplitudMaxima.Text = amplitudMaxima.ToString("F");
            lbl_AmplitudMinima.Text = "-" + amplitudMaxima.ToString("F");

            if (señal != null)
            {
                // Sirve para recorrer una coleccion o arreglo
                foreach (Muestra muestra in señal.Muestras)
                {
                    plnGrafica.Points.Add(new Point((muestra.X - tiempoInicial) * scrContenedor.Width, (muestra.Y / amplitudMaxima * ((scrContenedor.Height / 2) - 30) * -1 + (scrContenedor.Height / 2))));
                }
            }

            // Línea del Eje X
            plnEjeX.Points.Clear();
            plnEjeX.Points.Add(new Point(0, scrContenedor.Height / 2));
            plnEjeX.Points.Add(new Point((tiempoFinal - tiempoInicial) * scrContenedor.Width, scrContenedor.Height / 2));

            // Línea del Eje Y
            plnEjeY.Points.Clear();
            plnEjeY.Points.Add(new Point((-tiempoInicial) * scrContenedor.Width, 0));
            plnEjeY.Points.Add(new Point((-tiempoInicial) * scrContenedor.Width, scrContenedor.Height));
        }

        private void BotonTransformadadeFourier_Click(object sender, RoutedEventArgs e)
        {
            Señal transformada = Señal.transformar(señal);

            transformada.actualizarAmplitudMaxima();

            // Limpieza de polylines
            plnGrafica_Resultado.Points.Clear();

            // Impresión de la amplitud máxima en los labels de la ventana.
            lbl_AmplitudMaxima_Resultado.Text = transformada.AmplitudMaxima.ToString("F");
            lbl_AmplitudMinima_Resultado.Text = "-" + transformada.AmplitudMaxima.ToString("F");

            if (transformada != null)
            {
                // Sirve para recorrer una coleccion o arreglo
                foreach (Muestra muestra in transformada.Muestras)
                {
                    plnGrafica_Resultado.Points.Add(new Point((muestra.X - transformada.TiempoInicial) * scrContenedor_Resultado.Width, (muestra.Y / transformada.AmplitudMaxima * ((scrContenedor_Resultado.Height / 2) - 30) * -1 + (scrContenedor_Resultado.Height / 2))));
                }
            }

            int indiceFrecuenciaBaja1 = 0;
            int indiceFrecuenciaBaja2 = 0;
            int indiceFrecuenciaAlta1 = 0;
            int indiceFrecuenciaAlta2 = 0;

            indiceFrecuenciaBaja1 = 680 * transformada.Muestras.Count / (int)señal.FrecuenciaMuestreo;
            indiceFrecuenciaBaja2 = 1000 * transformada.Muestras.Count / (int)señal.FrecuenciaMuestreo;
            indiceFrecuenciaAlta1 = 1200 * transformada.Muestras.Count / (int)señal.FrecuenciaMuestreo;
            indiceFrecuenciaAlta2 = 1500 * transformada.Muestras.Count / (int)señal.FrecuenciaMuestreo;

            double valorMaximoFrecuenciaBaja = 0;
            int indiceMaximoFrecuenciaBaja = 0;

            for (int indiceActual = indiceFrecuenciaBaja1; indiceActual < indiceFrecuenciaBaja2; indiceActual++)
            {
                if (transformada.Muestras[indiceActual].Y > valorMaximoFrecuenciaBaja)
                {
                    valorMaximoFrecuenciaBaja = transformada.Muestras[indiceActual].Y;
                    indiceMaximoFrecuenciaBaja = indiceActual;
                }
            }

            double valorMaximoFrecuenciaAlta = 0;
            int indiceMaximoFrecuenciaAlta = 0;

            for (int indiceActual = indiceFrecuenciaAlta1; indiceActual < indiceFrecuenciaAlta2; indiceActual++)
            {
                if (transformada.Muestras[indiceActual].Y > valorMaximoFrecuenciaAlta)
                {
                    valorMaximoFrecuenciaAlta = transformada.Muestras[indiceActual].Y;
                    indiceMaximoFrecuenciaAlta = indiceActual;
                }
            }

            double frecuenciaFundamentalBaja = (double)indiceMaximoFrecuenciaBaja * señal.FrecuenciaMuestreo / (double)transformada.Muestras.Count;
            double frecuenciaFundamentalAlta = (double)indiceMaximoFrecuenciaAlta * señal.FrecuenciaMuestreo / (double)transformada.Muestras.Count;

            if (frecuenciaFundamentalBaja > 695 && frecuenciaFundamentalBaja < 699)
            {
                if (frecuenciaFundamentalAlta > 1206 && frecuenciaFundamentalAlta < 1215)
                {
                    lbl_PadNumerico.Text = "Tecla 1";
                }
                else
                if (frecuenciaFundamentalAlta > 1330 && frecuenciaFundamentalAlta < 1340)
                {
                    lbl_PadNumerico.Text = "Tecla 2";
                }
                else
                if (frecuenciaFundamentalAlta > 1470 && frecuenciaFundamentalAlta < 1480)
                {
                    lbl_PadNumerico.Text = "Tecla 3";
                }
            }
            else
            if (frecuenciaFundamentalBaja > 768 && frecuenciaFundamentalBaja < 772)
            {
                if (frecuenciaFundamentalAlta > 1206 && frecuenciaFundamentalAlta < 1215)
                {
                    lbl_PadNumerico.Text = "Tecla 4";
                }
                else
                if (frecuenciaFundamentalAlta > 1330 && frecuenciaFundamentalAlta < 1340)
                {
                    lbl_PadNumerico.Text = "Tecla 5";
                }
                else
                if (frecuenciaFundamentalAlta > 1470 && frecuenciaFundamentalAlta < 1480)
                {
                    lbl_PadNumerico.Text = "Tecla 6";
                }
            }
            else
            if (frecuenciaFundamentalBaja > 850 && frecuenciaFundamentalBaja < 854)
            {
                if (frecuenciaFundamentalAlta > 1206 && frecuenciaFundamentalAlta < 1215)
                {
                    lbl_PadNumerico.Text = "Tecla 7";
                }
                else
                if (frecuenciaFundamentalAlta > 1330 && frecuenciaFundamentalAlta < 1340)
                {
                    lbl_PadNumerico.Text = "Tecla 8";
                }
                else
                if (frecuenciaFundamentalAlta > 1470 && frecuenciaFundamentalAlta < 1480)
                {
                    lbl_PadNumerico.Text = "Tecla 9";
                }
            }
            else
            if (frecuenciaFundamentalBaja > 939 && frecuenciaFundamentalBaja < 943)
            {
                if (frecuenciaFundamentalAlta > 1206 && frecuenciaFundamentalAlta < 1215)
                {
                    lbl_PadNumerico.Text = "Tecla *";
                }
                else
                if (frecuenciaFundamentalAlta > 1330 && frecuenciaFundamentalAlta < 1340)
                {
                    lbl_PadNumerico.Text = "Tecla 0";
                }
                else
                if (frecuenciaFundamentalAlta > 1470 && frecuenciaFundamentalAlta < 1480)
                {
                    lbl_PadNumerico.Text = "Tecla #";
                }
            }


            // Línea del Eje X
            plnEjeX_Resultado.Points.Clear();
            plnEjeX_Resultado.Points.Add(new Point(0, scrContenedor_Resultado.Height / 2));
            plnEjeX_Resultado.Points.Add(new Point((transformada.TiempoFinal - transformada.TiempoInicial) * scrContenedor_Resultado.Width, scrContenedor_Resultado.Height / 2));

            // Línea del Eje Y
            plnEjeY_Resultado.Points.Clear();
            plnEjeY_Resultado.Points.Add(new Point((-transformada.TiempoInicial) * scrContenedor_Resultado.Width, 0));
            plnEjeY_Resultado.Points.Add(new Point((-transformada.TiempoInicial) * scrContenedor_Resultado.Width, scrContenedor_Resultado.Height));
        }

        private void Examinar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            if((bool)fileDialog.ShowDialog())
            {
                txt_RutaArchivo.Text = fileDialog.FileName;
            }

        }
    }
}