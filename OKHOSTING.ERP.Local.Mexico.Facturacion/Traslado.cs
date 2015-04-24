using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OKHOSTING.ERP.Local.Mexico.Facturacion
{
	/// <summary>
	/// Representa un impuesto trasladado (que le genera una obligacion de pago al Emisor) en una factura
	/// </summary>
	public struct Traslado
	{
		public Impuesto Impuesto;
		public decimal Tasa;
		public decimal Importe;
	}
}