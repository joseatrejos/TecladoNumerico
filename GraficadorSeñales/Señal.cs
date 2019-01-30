using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace GraficadorSeñales
{
    abstract class Señal
    {
        public List<Muestra> Muestras { get; set; }
        public double AmplitudMaxima { get; set; }
        public double Amplitud { get; set; }
        public double Fase { get; set; }
        public double Frecuencia { get; set; }
        public double FrecuenciaMuestreo { get; set; }
        public double TiempoInicial { get; set; }
        public double TiempoFinal { get; set; }
        public double Alpha { get; set; }

        abstract public double evaluar(double tiempo);

        public void construirSeñalDigital()
        {
            double periodoMuestreo = 1 / FrecuenciaMuestreo;

            for (double i = TiempoInicial; i <= TiempoFinal; i += periodoMuestreo)
            {
                double valorMuestra = evaluar(i);

                if (Math.Abs(valorMuestra) > AmplitudMaxima)
                {
                    AmplitudMaxima = Math.Abs(valorMuestra);
                }

                Muestras.Add(new Muestra(i, valorMuestra));
            }
        }

        public void truncar (double n)
        {
            foreach (Muestra muestra in Muestras)
            {
                if (muestra.Y > n)
                {
                    muestra.Y = n;
                }
                else if (muestra.Y < -n)
                {
                    muestra.Y = -n;
                }
            }
        }

        public void escalar (double factor)
        {
            foreach(Muestra muestra in Muestras)
            {
                muestra.Y *= factor;
            }
        }

        public void actualizarAmplitudMaxima()
        {
            AmplitudMaxima = 0;
            foreach(Muestra muestra in Muestras)
            {
                if (Math.Abs(muestra.Y) > AmplitudMaxima)
                {
                    AmplitudMaxima = Math.Abs(muestra.Y);
                }
            }
        }

        public void desplazar(double factor)
        {
            foreach (Muestra muestra in Muestras)
            {
                muestra.Y += factor;
            }
        }

        public static Señal suma(Señal sumando1, Señal sumando2)
        {
            SeñalPersonalizada resultado = new SeñalPersonalizada();

            // Sumandos de la Señal 1
            resultado.TiempoInicial = sumando1.TiempoInicial;
            resultado.TiempoFinal = sumando1.TiempoFinal;
            resultado.FrecuenciaMuestreo = sumando1.FrecuenciaMuestreo;

            int indice = 0;
            foreach (Muestra muestra in sumando1.Muestras)
            {
                Muestra muestraResultado = new Muestra();
                muestraResultado.X = muestra.X;
                muestraResultado.Y = muestra.Y + sumando2.Muestras[indice].Y;
                indice++;
                resultado.Muestras.Add(muestraResultado);
            }
            
            return resultado;
        }

        public static Señal multiplicacion(Señal multiplicando1, Señal multiplicando2)
        {
            SeñalPersonalizada resultado = new SeñalPersonalizada();

            // Multiplicandos de la Señal 1
            resultado.TiempoInicial = multiplicando1.TiempoInicial;
            resultado.TiempoFinal = multiplicando1.TiempoFinal;
            resultado.FrecuenciaMuestreo = multiplicando1.FrecuenciaMuestreo;

            int indice = 0;
            foreach (Muestra muestra in multiplicando1.Muestras)
            {
                Muestra muestraResultado = new Muestra();
                muestraResultado.X = muestra.X;
                muestraResultado.Y = muestra.Y * multiplicando2.Muestras[indice].Y;
                indice++;
                resultado.Muestras.Add(muestraResultado);
            }

            return resultado;
        }

        public static Señal convolucion(Señal operando1, Señal operando2)
        {
            SeñalPersonalizada resultado = new SeñalPersonalizada();
            resultado.TiempoInicial = operando1.TiempoInicial + operando2.TiempoInicial;
            resultado.TiempoFinal = operando1.TiempoFinal + operando2.TiempoFinal;
            resultado.FrecuenciaMuestreo = operando1.FrecuenciaMuestreo;

            double periodoMuestreo = 1 / resultado.FrecuenciaMuestreo;
            double duracionSeñal = resultado.TiempoFinal - resultado.TiempoInicial;
            double cantidadMuestrasResultado = duracionSeñal * resultado.FrecuenciaMuestreo;
            double instanteActual = resultado.TiempoInicial;

            for (int n = 0; n < cantidadMuestrasResultado; n++)
            {
                double valorMuestra = 0;
                for (int k = 0; k < operando2.Muestras.Count; k++)
                {
                    if ((n - k) >= 0 && (n - k) < operando2.Muestras.Count)
                    {
                        valorMuestra += operando1.Muestras[k].Y * operando2.Muestras[n - k].Y;
                    }
                }
                valorMuestra /= resultado.FrecuenciaMuestreo;
                Muestra muestra = new Muestra(instanteActual, valorMuestra);
                resultado.Muestras.Add(muestra);
                instanteActual += periodoMuestreo;
            }
            return resultado;
        }

        public static Señal transformar(Señal señal)
        {
            SeñalPersonalizada transformada = new SeñalPersonalizada();

            transformada.TiempoInicial = señal.TiempoInicial;
            transformada.TiempoFinal = señal.TiempoFinal;
            transformada.FrecuenciaMuestreo = señal.FrecuenciaMuestreo;

            for (int k = 0; k < señal.Muestras.Count; k++)
            {
                Complex muestra = 0;
                for (int n = 0; n < señal.Muestras.Count; n++)
                {
                    // Fórmula de la transformada de Fourier
                    muestra += señal.Muestras[n].Y * Complex.Exp((-2 * Math.PI * Complex.ImaginaryOne * k * n) / señal.Muestras.Count);
                }
                // Magnitude permite acceder a un número imaginario como número real.
                transformada.Muestras.Add(new Muestra((double)k/(double)señal.Muestras.Count, muestra.Magnitude));
            }

            return transformada;
        }
    }
}
