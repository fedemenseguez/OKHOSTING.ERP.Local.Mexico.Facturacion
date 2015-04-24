using System;
using System.Collections.Generic;
using System.Text;

namespace OKHOSTING.ERP.Local.Mexico.Facturacion
{
	/// <summary>
	/// Permite converir numeros a su representacion en texto (EXPERIMENTAL)
	/// </summary>
	public class NumberToText
	{
		string resultado;

		/***************************************************************************
		 * Ejemplo: Conversion de cantidades a letras (caso general)
		 * Algunas excepciones no estan consideradas. Estudie como hacerlo!!!
		 ***************************************************************************/

		/***************************************************************************
		 * Funcion que calcula la n-esima potencia de 10. Tanto el argumento de
		 * entrada (n) como el valor de retorno son de tipo int. No se controla
		 * la entrada de argumentos invalidos (potencias negativas, por ejemplo).
		 * 10**n = 10 * 10 * 10 * ... * 10 (n veces)
		 ***************************************************************************/

		protected int potencia10(int n) {
		   int i;	 /* Control de iteraciones: numero de multiplicaciones */
		   int pot;   /* Acumulador para el resultado */

		   /* Verifica si n es cero, en cuyo caso guarda un 1 como resultado */
		   if (n == 0) {
			  pot = 1;
		   }
		   else {
			  /* Si n es distinto de cero, lleva a cabo el calculo */
			  pot = 10;   /* El acumulador inicialmente vale 10 (n=1)
			  /* Este ciclo lleva a cabo el calculo, i varia desde 2 hasta n */
			  for (i=2; i<=n; i++)
				 /* Multiplica el acumulado actual por 10 */
				 pot = pot * 10;
		   }

		   /* Devuelve como valor de retorno el resultado obtenido (guardado en pot) */
		   return(pot);
		}

		/***************************************************************************
		 * Funcion que retorna el numero de digitos en la cantidad (cant) pasada
		 * como parametro. Se hace una iteracion, eliminando cada vez el digito de
		 * la derecha, e incrementando un contador (digitos). El ciclo se detiene
		 * cuando la cantidad es cero, es decir, se terminaron los digitos.
		 ***************************************************************************/
		protected int obtener_digitos(int cant) {
		   int digitos;	/* Cantidad de digitos */

		   for (digitos=0; cant>0; digitos++)
			  cant = cant / 10;
		   return(digitos);
		}

		/***************************************************************************
		 * Funcion que extrae y retorna el digito de mas a la izquierda de una
		 * cantidad (cant) con digs digitos. Lo que hace es dividir la cantidad por
		 * la (digs-1)-esima potencia de 10. Por ej., si hay 5 digitos, se dividira
		 * la cantidad por 1000. Pruebelo!!!
		 ***************************************************************************/
		protected int obtener_digito_izq(int cant, int digs) {
			  return(cant / potencia10(digs-1));
		}

		/***************************************************************************
		 * Funcion que elimina el digito de mas a la izquierda de una cantidad
		 * (cant) pasada como parametro. La funcion recibe el numero de digitos
		 * (digs) en la cantidad y obtiene el resto de la division de la cantidad
		 * por la (digs-1)-esima potencia de 10. Por ej., si hay 5 digitos, se
		 * dividira la cantidad por 1000 y se tomara el resto. Pruebelo!!!
		 ***************************************************************************/
		protected int quitar_digito_izq(int cant, int digs) {
			  return(cant % potencia10(digs-1));
		}

		/***************************************************************************
		 * Funcion que obtiene la posicion decimal (centenas, decenas, unidades) de
		 * una cantidad con digs digitos. Esta posiciones podrian ser simples, de
		 * miles o de millones. Para esto, se aplica modulo 3 a digs, obteniendose
		 * posibles valores de 0 (centenas), 1 (unidades) y 2 (decenas). La funcion
		 * retorna este valor. Por ej., si la cantidad es 12500, digs valdra 5, por
		 * lo que se retornara un 2 que indica que la primera posicion de la
		 * izquierda es una posicion de decenas (en este caso, decenas de miles).
		 ***************************************************************************/
		protected int obtener_posicion_decimal(int digs) {
			  return(digs % 3);
		}

		/***************************************************************************
		 * Funcion que procesa un digito dig, dependiendo de su posicion pos en la
		 * cantidad (unidades, decenas o centenas). Para esto se emplea un switch
		 * que selecciona segun la posicion. Para el caso de las unidades es
		 * necesario ademas analizar el grupo decimal (unidades simples, miles,
		 * millones), para poder desplegar el texto adicional necesario.
		 ***************************************************************************/
		protected void procesar_digito(int dig, int pos, int digitos) {
		   int grupo; /* Grupo decimal a que pertenece el digito si es de unidades */
		   switch (pos) {
			  case 1: { /* unidades */
				 switch (dig) {
					case 0: {printf(""); break;}
					case 1: {printf("uno "); break;}
					case 2: {printf("dos "); break;}
					case 3: {printf("tres "); break;}
					case 4: {printf("cuatro "); break;}
					case 5: {printf("cinco "); break;}
					case 6: {printf("seis "); break;}
					case 7: {printf("siete "); break;}
					case 8: {printf("ocho "); break;}
					case 9: {printf("nueve "); break;}
				 }

				 /* Se estima el grupo en que se esta trabajando, para saber asi si es */
				 /* necesario incluir palabras como mil, millones, etc. Esto se logra  */
				 /* dividiendo (dig-1) entre 3. Si el resultado es 0, se trata del	 */
				 /* primer grupo de tres digitos (unidades simples), si es 1 se trata  */
				 /* del segundo (miles), 2 (millones), 3 (miles de millones).		  */
				 /* Este estudio se requiere solo en las unidades (por que?).		  */
				 grupo = (digitos-1) / 3;
				 switch (grupo) {
					case 0: {printf("unidades\n"); break;}  /* Puede cambiarse x PESOS */
					case 1: {printf("mil "); break;}
					case 2: {printf("millones "); break;}
					case 3: {printf("mil "); break;}
				 }
				 break;
			  }
			  case 2: { /* decenas */
				 switch (dig) {
					case 0: {printf(""); break;}
					case 1: {printf("dieci"); break;}
					case 2: {printf("veinte y "); break;}
					case 3: {printf("treinta y "); break;}
					case 4: {printf("cuarenta y "); break;}
					case 5: {printf("cincuenta y "); break;}
					case 6: {printf("sesenta y "); break;}
					case 7: {printf("setenta y "); break;}
					case 8: {printf("ochenta y "); break;}
					case 9: {printf("noventa y "); break;}
				 }
				 break;
			  }
			  case 0: { /* centenas */
				 switch (dig) {
					case 0: {printf(""); break;}
					case 1: {printf("ciento "); break;}
					case 2: {printf("doscientos "); break;}
					case 3: {printf("trescientos "); break;}
					case 4: {printf("cuatrocientos "); break;}
					case 5: {printf("quinientos "); break;}
					case 6: {printf("seiscientos "); break;}
					case 7: {printf("setecientos "); break;}
					case 8: {printf("ochocientos "); break;}
					case 9: {printf("novecientos "); break;}
				 }
				 break;
			  }
		   }
		}


		/***************************************************************************
		 * Programa que convierte una cantidad numerica en su equivalente en letras.
		 * Solicita al usuario la cantidad en cuestion y le devuelve el equivalente.
		 * Existen algunas excepciones que no fueron consideradas para no complicar
		 * demasiado el ejemplo. Ademas, se permiten cantidades de hasta 9 digitos.
		 * Estudie los cambios necesarios para considerar todas las excepciones!.
		 ***************************************************************************/

		public string Convertir(int cantidad)
		{
			//int cantidad;   /* La cantidad a convertir, dada por el usuario */
			int digitos;	/* Cantidad de digitos en la cantidad dada */
			int dig;		/* Digito con el que se esta trabajando actualmente */
			int posicion;   /* Indica si se esta trabajando sobre unidades, decenas o centenas */
			int cant;	   /* Cantidad a la que se iran quitando uno a uno los digitos */

			resultado = "";

			/* Determina la cantidad de digitos en la cantidad */
			digitos = obtener_digitos(cantidad);

			/* Ciclo de procesamiento de la cantidad. En cada iteracion se trabaja con un digito. */
			/* El ciclo se lleva a cabo tantas veces como digitos existen (la variable digitos se */
			/* decrementa en cada iteracion, y el ciclo para cuando llega a cero). Se trabaja con */
			/* una copia de la cantidad hecha en la variable cant (para no destruir la original). */
			for (cant=cantidad; digitos>0; digitos--) {
			  /* Se obtiene en dig el digito mas a la izquierda */
			  dig = obtener_digito_izq(cant, digitos);
			  
			  /* Se obtiene la posicion decimal del digito (unidades, decenas, centenas) */
			  posicion = obtener_posicion_decimal(digitos);
			  
			  /* Se procesa el digito (se convierte a letras) */
			  procesar_digito(dig, posicion, digitos);
			  
			  /* Se quita el digito de la cantidad */
			  cant = quitar_digito_izq(cant, digitos);
			}

			return resultado;
		}

		protected void printf(string s)
		{
			resultado += s;
		}
	}
}
