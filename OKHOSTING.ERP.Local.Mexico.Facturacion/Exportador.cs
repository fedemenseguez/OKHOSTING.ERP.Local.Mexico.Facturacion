using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.IO;

namespace OKHOSTING.ERP.Local.Mexico.Facturacion
{
	/// <summary>
	/// Exporta una serie de facturas a Excel
	/// </summary>
	public class Exportador
	{
		public static void ExportarExcel(IEnumerable<XmlDocument> facturas, string rutaArchivo)
		{
			ExcelPackage excel = new ExcelPackage(new FileInfo(rutaArchivo));
			var hoja = excel.Workbook.Worksheets.Add("Facturas");
			
			//agregar headers
			hoja.Cells[1, 1].Value = "Folio Fiscal";
			hoja.Cells[1, 2].Value = "Fecha";
			hoja.Cells[1, 3].Value = "Forma de pago";
			hoja.Cells[1, 4].Value = "Metodo de Pago";

			hoja.Cells[1, 5].Value = "RFC Emisor";
			hoja.Cells[1, 6].Value = "Razon Social Emisor";
			hoja.Cells[1, 7].Value = "RFC Receptor";
			hoja.Cells[1, 8].Value = "Razon Social Receptor";

			hoja.Cells[1, 9].Value = "IVA 0% acreditado";
			hoja.Cells[1, 9].Value = "IVA 16% acreditado";
			hoja.Cells[1, 10].Value = "ISR acreditado";
			hoja.Cells[1, 11].Value = "IEPS acreditado";

			hoja.Cells[1, 12].Value = "IVA 0% retenido";
			hoja.Cells[1, 12].Value = "IVA 16% retenido";
			hoja.Cells[1, 13].Value = "ISR retenido";
			hoja.Cells[1, 14].Value = "IEPS retenido";
			
			hoja.Cells[1, 15].Value = "Subtotal";
			hoja.Cells[1, 16].Value = "Descuento";
			hoja.Cells[1, 17].Value = "Total";

			//fila
			int i = 2;

			foreach (XmlDocument fact in facturas)
			{
				hoja.Cells[i, 1].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Complemento/tfd:TimbreFiscalDigital@UUID"].Value;
				hoja.Cells[i, 2].Value = fact.DocumentElement["/cfdi:Comprobante@fecha"].Value;
				hoja.Cells[i, 3].Value = fact.DocumentElement["/cfdi:Comprobante@formaDePago"].Value;
				hoja.Cells[i, 4].Value = fact.DocumentElement["/cfdi:Comprobante@metodoDePago"].Value;
				
				hoja.Cells[i, 5].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Emisor@rfc"].Value;
				hoja.Cells[i, 6].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Emisor@nomber"].Value;
				hoja.Cells[i, 7].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Receptor@rfc"].Value;
				hoja.Cells[i, 8].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Receptor@nombre"].Value;

				if (fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto=IVA and @tasa=0]"] != null) hoja.Cells[i, 9].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto=IVA and @tasa=0]"].Value;
				if (fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto=IVA and @tasa=16]"] != null) hoja.Cells[i, 10].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto=IVA and @tasa=16]"].Value;
				if (fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto=ISR]"] != null) hoja.Cells[i, 11].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto=ISR]"].Value;
				if (fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto=IEPS]"] != null) hoja.Cells[i, 12].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto=IEPS]"].Value;

				if (fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto=IVA and @tasa=0]"] != null) hoja.Cells[i, 13].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto=IVA and @tasa=0]"].Value;
				if (fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto=IVA and @tasa=16]"] != null) hoja.Cells[i, 14].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto=IVA and @tasa=16]"].Value;
				if (fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto=ISR]"] != null) hoja.Cells[i, 15].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto=ISR]"].Value;
				if (fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto=IEPS]"] != null) hoja.Cells[i, 16].Value = fact.DocumentElement["/cfdi:Comprobante/cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto=IEPS]"].Value;
				
				hoja.Cells[i, 15].Value = fact.DocumentElement["/cfdi:Comprobante@subTotal"].Value;
				hoja.Cells[i, 16].Value = fact.DocumentElement["/cfdi:Comprobante@descuento"].Value;
				hoja.Cells[i, 17].Value = fact.DocumentElement["/cfdi:Comprobante@total"].Value;

				i++;
			}

			excel.Save();
		}
	}
}
