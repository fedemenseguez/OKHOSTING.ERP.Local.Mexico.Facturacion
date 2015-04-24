using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OKHOSTING.ERP.Local.Mexico.Facturacion
{
	/// <summary>
	/// Representa un concepto, producto o servicio que se factura
	/// </summary>
	public struct Concepto
	{
		/// <summary>
		/// Cantidad de elementos que se facturan
		/// </summary>
		public decimal Cantidad;
		/// <summary>
		/// Unidad con la que se mide este concepto
		/// </summary>
		/// <example>Pieza, Kilo, Litro</example>
		public string Unidad;
		/// <summary>
		/// Código del concepto
		/// </summary>
		public string NoIdentificacion;
		/// <summary>
		/// Descripción del concepto
		/// </summary>
		public string Descripcion;
		/// <summary>
		/// Valor unitario o precio del concepto
		/// </summary>
		public decimal ValorUnitario;
		/// <summary>
		/// Importe total que se factura por el concepto
		/// </summary>
		/// <remarks>Debe ser equivalente a la multiplicación Cantidad x ValorUnitario</remarks>
		public decimal Importe
		{
			get
			{
				return Cantidad * ValorUnitario;
			}
		}
	}
}