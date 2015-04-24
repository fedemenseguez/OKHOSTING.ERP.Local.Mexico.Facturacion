using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OKHOSTING.ERP.Local.Mexico.Facturacion
{
	/// <summary>
	/// Representa los tipos de impuestos que se pueden declarar en una factura, ya sea como retenciones o como traslados
	/// </summary>
	public enum Impuesto
	{
		IVA = 1,
		ISR = 2,
		IEPS = 3,
	}
}