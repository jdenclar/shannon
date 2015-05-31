using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escaneador
{
    static class Limpiador
    {
        public static IEnumerable <char> Limpia (IEnumerable <string> cadenas)
        {
            char anterior = ' ';

            foreach (var cadena in cadenas)
            {
                if (anterior != ' ')
                {
                    anterior = ' ';
                    yield return anterior;
                }

                foreach (var caracter in cadena)
                {
                    var caracterLimpio = Limpia (caracter);

                    if (caracterLimpio != anterior || anterior != ' ')
                    {
                        yield return caracterLimpio;
                        anterior = caracterLimpio;
                    }
                }
            }
        }

        public static char Limpia (char caracter)
        {
            if (caracter == 'ñ')
            {
                return 'Ñ';
            }

            caracter = Char.ToUpper(caracter);

            if (caracter >= 'A' && caracter <= 'Z')
            {
                return caracter;
            }

            else if (caracter == 'Ñ')
            {
                return 'Ñ';
            }
            else if (caracter == 'Á')
            {
                return 'Á';
            }
            else if (caracter == 'É')
            {
                return 'É';
            }
            else if (caracter == 'Í')
            {
                return 'Í';
            }
            else if (caracter == 'Ó')
            {
                return 'Ó';
            }
            else if (caracter == 'Ú')
            {
                return 'Ú';
            }
            
            /*
            
            if (caracter == '.' || caracter == ',')
            {
                return caracter;
            }
            
             */

            return ' ';

        }

        

    }

    interface ITokenizador
    {
        IEnumerable <string> Tokens (IEnumerable <char> fuente);
        string Separador { get; }
    }

    class TokenizadorLetras : ITokenizador
    {
        public IEnumerable <string> Tokens (IEnumerable <char> fuente)
        {
            return fuente.Select (caracter => String.Empty + caracter);
        }

        public string Separador
        {
            get { return String.Empty; }
        }
    }
    class TokenizadorDigramas : ITokenizador
    {
        public IEnumerable<string> Tokens(IEnumerable<char> fuente)
        {
            char c1 = (char) 0;
            char c2 = (char) 0;

            foreach (var caracter in fuente)
            {
                c1 = c2;
                c2 = caracter;

                if (c1 != (char) 0)
                {
                    yield return c1 + "" + c2;
                }
            }
        }

        public string Separador
        {
            get { return String.Empty; }
        }
    }

    class TokenizadorPalabras : ITokenizador
    {
        public IEnumerable <string> Tokens (IEnumerable <char> fuente)
        {
            return new String (fuente.ToArray ()).Split (' ');
        }
        public string Separador
        {
            get { return " "; }
        }
    }

    class Estado
    {
        public string Valor { get; set; }

        public int Ocurrencias { get; set; }

        public IDictionary<string, Estado> Transiciones { get; set; }

        public Estado ()
        {
            Transiciones = new Dictionary <string, Estado> ();
            Ocurrencias = 0;
        }

        public Estado NuevaTransicion (string nuevoValor)
        {
            Estado estado;
            if (!Transiciones.TryGetValue (nuevoValor, out estado))
            {
                estado = new Estado ()
                         {
                             Valor = nuevoValor
                         };

                Transiciones.Add (estado.Valor, estado);
            }

            estado.Ocurrencias = estado.Ocurrencias + 1;

            return estado;
        }

        public string ElementoAleatorio (Random aleatorizador)
        {
            int total = Transiciones.Values.Sum (transicion => transicion.Ocurrencias);
            var cadenas = Transiciones.Values.OrderBy(transicion => transicion.Valor).SelectMany (transicion => Enumerable.Repeat (transicion.Valor, transicion.Ocurrencias));

            return cadenas.ElementAt (aleatorizador.Next (total));
        }

        public IEnumerable <string> ToEnumerable ()
        {
            yield return String.Format ("{0}|{1}|{2}", Valor, Ocurrencias, Transiciones.Count);

            foreach (var transicion in Transiciones)
            {
                foreach (var elemento in transicion.Value.ToEnumerable ())
                {
                    yield return elemento;
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var lineas = System.IO.File.ReadLines (@"..\..\data\el_misterio_de_la_cripta_embrujada.txt");
            var fuente = Limpiador.Limpia (lineas);

            // Selección del tokenizador (por letras o palabras)
            var tokenizador = new TokenizadorLetras ();
            //var tokenizador = new TokenizadorDigramas();
            //var tokenizador = new TokenizadorPalabras();

            var estadoInicial = new Estado ();
            bool digrama = false;

            Estado estadoAnterior = estadoInicial;
            foreach (var token in tokenizador.Tokens (fuente))
            {
                if (estadoAnterior != estadoInicial)
                {
                    estadoAnterior.NuevaTransicion (token);
                }
                    
                var nuevoEstado = estadoInicial.NuevaTransicion (token);

                if (digrama)
                {
                    estadoAnterior = nuevoEstado;
                }
            }

            // depuración -- volcado a fichero del grafo de transiciones
            // System.IO.File.WriteAllLines ("c:\\tmp\\orden-1.txt", estadoInicial.ToEnumerable ());

            Random aleatorio = new Random ();
            int total = 75;

            estadoAnterior = estadoInicial;

            for (int i = 0; i < total; i++)
            {
                string elemento = estadoAnterior.ElementoAleatorio (aleatorio);

                if (digrama)
                {
                    estadoAnterior = estadoInicial.Transiciones [elemento];
                }

                if (i > 0)
                {
                    Console.Write (tokenizador.Separador);
                }
                Console.Write (elemento);
            }

            Console.ReadKey ();



        }

    }
}
