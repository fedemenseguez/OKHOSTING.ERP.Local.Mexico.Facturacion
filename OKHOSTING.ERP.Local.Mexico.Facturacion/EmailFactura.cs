using System;
using OKHOSTING.Tools.Net.Mail;

namespace OKHOSTING.ERP.Local.Mexico.Facturacion
{
	/// <summary>
	/// Plantilla de correo que se manda al generar una factura electronica automaticamente
	/// </summary>
	public class EmailFactura : MailTemplate
	{
		/// <summary>
		/// En el futuro puede definir tags personalizados que se escriban en la plantilla, por hoy esta vacio
		/// </summary>
		public override void Init()
		{
			//base.ReplaceTags();
		}
	}
}