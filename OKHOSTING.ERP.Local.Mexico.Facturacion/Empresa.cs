using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OKHOSTING.ERP.Local.Mexico.Facturacion
{
	/// <summary>
	/// Empresa que puede ser usada como Emisor o Receptor dentro de una factura
	/// </summary>
	[Serializable]
	public struct Empresa
	{
		/// <summary>
		/// Razón Social de la empresa
		/// </summary>
		public string Nombre;
		
		/// <summary>
		/// Registro Federal de Constribuyentes de la empresa, sin guiones ni espacios, todo con mayúsculas
		/// </summary>
		public string RFC;
		
		/// <summary>
		/// Nodo requerido para incorporar los regímenes en los que tributa el contribuyente emisor. Puede contener más de un régimen.
		/// </summary>
		/// <example>
		/// Sociedad Civil, Sociedad Anonima, Persona Fisica con Actividad Empresarial
		/// </example>
		public string RegimenFiscal;
	}
}