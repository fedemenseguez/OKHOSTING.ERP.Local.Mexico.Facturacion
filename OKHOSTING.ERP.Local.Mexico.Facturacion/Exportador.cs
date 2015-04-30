using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace OKHOSTING.ERP.Local.Mexico.Facturacion
{
	/// <summary>
	/// Exporta una serie de comprobanteuras a Excel
	/// </summary>
	public static class Exportador
	{
		public static void ExportarExcel(IEnumerable<XmlDocument> facturas, string rutaArchivo)
		{
			var fileInfo = new FileInfo(rutaArchivo);

			if (fileInfo.Exists)
			{
				fileInfo.Delete();
			}

			ExcelPackage excel = new ExcelPackage(fileInfo);
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
			hoja.Cells[1, 10].Value = "IVA 16% acreditado";
			hoja.Cells[1, 11].Value = "ISR acreditado";
			hoja.Cells[1, 12].Value = "IEPS acreditado";

			hoja.Cells[1, 13].Value = "IVA 0% retenido";
			hoja.Cells[1, 14].Value = "IVA 16% retenido";
			hoja.Cells[1, 15].Value = "ISR retenido";
			hoja.Cells[1, 16].Value = "IEPS retenido";
			
			hoja.Cells[1, 17].Value = "Subtotal";
			hoja.Cells[1, 18].Value = "Descuento";
			hoja.Cells[1, 19].Value = "Total";

			//fila
			int fila = 2;

			foreach (XmlDocument fact in facturas)
			{
				XmlNamespaceManager nsmgr = new XmlNamespaceManager(fact.NameTable);
				nsmgr.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
				nsmgr.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital http://www.sat.gob.mx/TimbreFiscalDigital/TimbreFiscalDigital.xsd");

				var comprobante = fact.DocumentElement;

				//buscar folio
				foreach (XmlNode nodo in comprobante.SelectSingleNode("cfdi:Complemento", nsmgr).ChildNodes)
				{
					if (nodo.Name == "tfd:TimbreFiscalDigital")
					{
						hoja.Cells[fila, 1].Value = nodo.Attributes["UUID"].Value;
						break;
					}
				}

				hoja.Cells[fila, 2].Value = comprobante.SelectSingleNode("@fecha", nsmgr).Value;
				hoja.Cells[fila, 3].Value = comprobante.SelectSingleNode("@formaDePago", nsmgr).Value;
				hoja.Cells[fila, 4].Value = comprobante.SelectSingleNode("@metodoDePago", nsmgr).Value;
				
				hoja.Cells[fila, 5].Value = comprobante.SelectSingleNode("cfdi:Emisor/@rfc", nsmgr).Value;
				hoja.Cells[fila, 6].Value = comprobante.SelectSingleNode("cfdi:Emisor/@nombre", nsmgr).Value;
				hoja.Cells[fila, 7].Value = comprobante.SelectSingleNode("cfdi:Receptor/@rfc", nsmgr).Value;
				hoja.Cells[fila, 8].Value = comprobante.SelectSingleNode("cfdi:Receptor/@nombre", nsmgr).Value;

				if (comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto='IVA' and @tasa='0.00']/@importe", nsmgr) != null) hoja.Cells[fila, 9].Value = comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto='IVA' and @tasa='0.00']/@importe", nsmgr).Value;
				if (comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto='IVA' and @tasa='16.00']/@importe", nsmgr) != null) hoja.Cells[fila, 10].Value = comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto='IVA' and @tasa='16.00']/@importe", nsmgr).Value;
				if (comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto='ISR']/@importe", nsmgr) != null) hoja.Cells[fila, 11].Value = comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto='ISR']/@importe", nsmgr).Value;
				if (comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto='IEPS']/@importe", nsmgr) != null) hoja.Cells[fila, 12].Value = comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado[@impuesto='IEPS']/@importe", nsmgr).Value;

				if (comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto='IVA' and @tasa='0.00']/@importe", nsmgr) != null) hoja.Cells[fila, 13].Value = comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto='IVA' and @tasa='0.00']/@importe", nsmgr).Value;
				if (comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto='IVA' and @tasa='16.00']/@importe", nsmgr) != null) hoja.Cells[fila, 14].Value = comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto='IVA' and @tasa='16.00']/@importe", nsmgr).Value;
				if (comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto='ISR']/@importe", nsmgr) != null) hoja.Cells[fila, 15].Value = comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto='ISR']/@importe", nsmgr).Value;
				if (comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto='IEPS']/@importe", nsmgr) != null) hoja.Cells[fila, 16].Value = comprobante.SelectSingleNode("cfdi:Impuestos/cfdi:Retenciones/cfdi:Retencion[@impuesto='IEPS']/@importe", nsmgr).Value;

				hoja.Cells[fila, 17].Value = comprobante.SelectSingleNode("@subTotal", nsmgr).Value;
				if (comprobante.SelectSingleNode("@descuento", nsmgr) != null) hoja.Cells[fila, 18].Value = comprobante.SelectSingleNode("@descuento", nsmgr).Value;
				hoja.Cells[fila, 19].Value = comprobante.SelectSingleNode("@total", nsmgr).Value;

				fila++;
			}

			excel.Save();
		}
	}
}