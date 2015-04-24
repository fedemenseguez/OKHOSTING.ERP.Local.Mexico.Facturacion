using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OKHOSTING.ERP.Local.Mexico.Facturacion
{
	/// <summary>
	/// Domicilio que puede ser usado para domicilios fiscales o de sucursal dentro de una factura
	/// </summary>
	[Serializable]
	public struct Domicilio
	{
		public string Calle;
		public string NoExterior;
		public string NoInterior;
		public string Colonia;
		public string Localidad;
		public string Referencia;
		public string Municipio;
		public string Estado;
		public string Pais;
		public string CodigoPostal;
	}
}