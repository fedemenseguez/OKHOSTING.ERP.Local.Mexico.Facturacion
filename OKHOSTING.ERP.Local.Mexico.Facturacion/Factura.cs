using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.Diagnostics;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;
using OKHOSTING.ERP.Local.Mexico.Facturacion.Timbrado.FormasDigitales;
using OKHOSTING.Tools;
using WatiN.Core;
using WatiN.Core.DialogHandlers;

namespace OKHOSTING.ERP.Local.Mexico.Facturacion
{
	/// <summary>
	/// Crea facturas digitales para México siguiendo el estandar del SAT vigente a Junio del 2008
	/// </summary>
	public class Factura
	{
		/// <summary>
		/// RFC que debe usarse cuando se generewn facturas para el público en general, incluyendo extrangeros y mexicanos sin RFC
		/// </summary>
		public const string RFCPublicoEnGeneral = "XAXX010101000";

		#region Propiedades públicas

		/// <summary>
		/// Ruta de la carpeta donde se guardan las facturas
		/// </summary>
		public static readonly string CarpetaFacturas = AppDomain.CurrentDomain.BaseDirectory + @"Custom\FacturacionElectronica\Facturas\";
		
		/// <summary>
		/// Ruta de la carpeta donde se encuentran los recuros
		/// </summary>
		public static readonly string CarpetaRecursos = AppDomain.CurrentDomain.BaseDirectory + @"Custom\FacturacionElectronica\Recursos\";

		/// <summary>
		/// Ruta de la carpeta donde se guardan los archivos temporales
		/// </summary>
		public static readonly string CarpetaTemporal = AppDomain.CurrentDomain.BaseDirectory + @"Custom\FacturacionElectronica\Temporal\";
		
		/// <summary>
		/// Version del XML, debe ser 2.0
		/// </summary>
		public string Version;

		/// <summary>
		/// Serie de la factura que se va a crear
		/// </summary>
		/// <example>A, B, C o cualquier cadena</example>
		public string Serie;

		/// <summary>
		/// El número de la factura
		/// </summary>
		/// <example>100</example>
		public int Folio;

		/// <summary>
		/// Fecha y hora en que se crea la factura, debe contener hora, minutos y segundos
		/// </summary>
		/// <example>1/1/2008 10:30:00 A.M.</example>
		public DateTime Fecha;

		/// <summary>
		/// El sello digital, se genera automaticamente con la función CrearSello usando openssl.exe
		/// </summary>
		public string Sello;

		/// <summary>
		/// Número de aprocación de los folios. Este dato lo otorga la SHCP al solicitar los folios.
		/// </summary>
		/// <example>2000</example>
		//public string NoAprobacion;

		/// <summary>
		/// Año de aprobación de los folios. Este dato lo otorga la SHCP al solicitar los folios.
		/// </summary>
		/// <example>2008</example>
		//public string AnoAprobacion;

		/// <summary>
		/// La forma de pago para la factura
		/// </summary>
		/// <remarks>Obligatorio</remarks>
		/// <example>Pago en una sola excibición</example>
		public string FormaDePago;

		/// <summary>
		/// El número del certificado con el que se autentifica la factura
		/// </summary>
		/// <example>00001000000001655555</example>
		public string NoCertificado;

		/// <summary>
		/// El certificado con el que se autentifica la facturas
		/// </summary>
		/// <remarks>Este certificado se obtiene convirtiendo el certificado de la FIEL a formato PEM usando openssl</remarks>
		/// <example>
		/// MIICXgIBAAKBgQDKRVP186zuDWHP9BDOGPAOfJaqBlKKaNN6FV0mkO6iyG7TlpWr
		/// O3IBRBX4lw5k5MEDBwLxFmRQJ68ZHkaPDdBfGi3SO6VA+rkt50tlH5bLcSycWDAk
		/// CJ7U72TWDypx69TcafQwpr2vrfXPRmEz/kie5vF0H3tVkVxn5WQ6YUAMeQIDAQAB
		/// AoGAH3sazjTWvVYn2w3Jb8pB0n9hk6TYQ+J8x4t7q/zypzM6zIOrV7Mw0zGdmcso
		/// 2lsgDzCQLKWnhzIl9mrX4Hvt6hhDm/eAWLQ7JDYx2sQ2OO/HmizYKU4QF8pInxCu
		/// vlFxt/a4t9+KEf2smgbJUR6wgvhq76UzJigbwy1S27DRiAECQQD2Kr2hMGa1DIpI
		/// x+nufzhgXcxBoR3IpGkKhUx6oEKswMSiInhuyNp/MvYJgJbMwxT77mXBNoobH6a6
		/// rMD3+YBZAkEA0lm2judLZk5CZ/Lkc4bha/NOeHuYQ8IOEW/eE+RcnnM6tLpDWwt7
		/// o6cl2jVY7TbKJUq8CxLqgG3dP9kuOcJpIQJBAKRKVDLu1a1BiE0Yt0TIPXz7POYU
		/// PId7SuuNmURCDx2yrckzzkLJ5CF+hnxDCOHx1OBq9BhmaPe/QQxXXZZiO0kCQQC0
		/// PnWFHEJqprKWWfZR3Aj7NGBQMy/1F6pwXJhCGVMX3ws148llkYBfahGwWjgaA/HR
		/// ZKmfH5VbeUi1tka67ZChAkEAzaiCnVtXMIuXsIuMLXgn9GKRvjKfU7y6M1jGDlha
		/// KkRe3/2W5yODqZT1CgNJK8GuHgUWXeqKeYQEEGvQUE5cVg==
		/// </example>
		public string Certificado;

		/// <summary>
		/// Condiciones para el pago
		/// </summary>
		/// <remarks>Opcional</remarks>
		public string CondicionesDePago;

		/// <summary>
		/// Subtotal de la factura sin incluir impuestos, descuentos niretenciones. Equivale a la suma del importe de todos los conceptos
		/// </summary>
		public decimal Subtotal
		{
			get
			{
				return Conceptos.Sum<Concepto>(c => c.Importe);
			}
		}

		/// <summary>
		/// Descuento que se aplica a la factura
		/// </summary>
		/// <remarks>Opcional</remarks>
		public decimal Descuento;

		/// <summary>
		/// Motivo por el cual se aplica un descuento
		/// </summary>
		/// <remarks>Opcional</remarks>
		public string MotivoDescuento;

		/// <summary>
		/// Total a pagar. Equivale al Subtotal - Descuento + Traslados - Retenciones
		/// </summary>
		public decimal Total
		{
			get
			{
				var retenciones = Retenciones.Sum<Retencion>(r => r.Importe);
				var traslados = Traslados.Sum<Traslado>(t => t.Importe);

				return Subtotal + traslados - retenciones;
			}
		}

		/// <summary>
		/// Método de pago
		/// </summary>
		/// <example>NO IDENTIFICADO, CHEQUE y TRANSFERENCIA</example>
		/// <remarks>Opcional hasta antes del 1/Julio/2012, obligatorio a partir de esa fecha</remarks>
		public string MetodoDePago;
		
		/// <summary>
		/// Ultimos 4 digitos de la cuenta a donde se realizó el pago
		/// </summary>
		/// <example>0546</example>
		/// <remarks>Opcional, se usa solo en caso de que el metodo de pago sea "Deposito bancario"</remarks>
		public string NumCtaPago;

		/// <summary>
		/// Tipo de comprobante
		/// </summary>
		public TipoDeComprobante TipoDeComprobante;

		/// <summary>
		/// Lugar donde se expide la factura
		/// </summary>
		/// <example>Guadalajara, Jalisco</example>
		/// <remarks>Obligatorio</remarks>
		public string LugarExpedicion;

		/// <summary>
		/// Namespace del archivo XML
		/// </summary>
		/// <example>http://www.sat.gob.mx/cfd/2</example>
		public string Xmlns;

		/// <summary>
		/// Esquema del XML
		/// </summary>
		/// <example>http://www.w3.org/2001/XMLSchema-instance</example>
		public string Xmlns_Xsi;

		/// <summary>
		/// Ubicación del esquema del XML
		/// </summary>
		/// <example>http://www.sat.gob.mx/cfd/2 http://www.sat.gob.mx/sitio_internet/cfd/2/cfdv2.xsd</example>
		public string Xsi_SchemaLocation;

		/// <summary>
		/// Empresa que emite la factura
		/// </summary>
		public Empresa Emisor;

		/// <summary>
		/// Empresa a quien va dirigida la factura
		/// </summary>
		public Empresa Receptor;

		/// <summary>
		/// Domicilio fiscal del Emisor
		/// </summary>
		public Domicilio DomicilioFiscalEmisor;

		/// <summary>
		/// Domicilio fiscal del Receptor
		/// </summary>
		public Domicilio DomicilioFiscalReceptor;

		/// <summary>
		/// Lista de conceptos, productos o servicios que se están facturando
		/// </summary>
		public List<Concepto> Conceptos = new List<Concepto>();

		/// <summary>
		/// Lista de impuestos retenidos
		/// </summary>
		public List<Retencion> Retenciones = new List<Retencion>();

		/// <summary>
		/// Lista de impuestos trasladados
		/// </summary>
		public List<Traslado> Traslados = new List<Traslado>();

		/// <summary>
		/// Ruta donde se guardó el archivo XML
		/// </summary>
		/// <remarks>Se establece automáticamente con la funcion Guardar()</remarks>
		public string RutaXML;

		/// <summary>
		/// Ruta donde se guardó el archivo HTML para impresión
		/// </summary>
		/// <remarks>Se establece automáticamente con la funcion GuardarHTML()</remarks>
		public string RutaHTML;

		/// <summary>
		/// Ruta donde se guardó el archivo PDF para impresión
		/// </summary>
		/// <remarks>Se establece automáticamente con la funcion GuardarPDF()</remarks>
		public string RutaPDF;

		/// <summary>
		/// Devuelve el número de factura incluyendo la serie
		/// </summary>
		/// <example>A100</example>
		public string NoFactura
		{
			get
			{
				return Serie + Folio;
			}
		}

		#endregion

		#region Crear XML

		/// <summary>
		/// Crea la factura en formato XML
		/// </summary>
		/// <returns>Instancia del XmlDocument conteniendo toda la información de la factura</returns>
		protected XmlDocument CrearXML()
		{
			XmlDocument xmlDoc;
			XmlAttribute schemaLocation;
			
			XmlElement xml_comprobante;
			XmlElement xml_emisor;
			XmlElement xml_receptor;
			//XmlElement xml_domicilioFiscalEmisor;
			XmlElement xml_reginemFiscalEmisor;
			XmlElement xml_domicilioFiscalReceptor;
			
			XmlElement xml_conceptos;
			XmlElement xml_concepto;
			
			XmlElement xml_impuestos;
			XmlElement xml_retenciones;
			XmlElement xml_retencion;
			XmlElement xml_traslados;
			XmlElement xml_traslado;

			//sumar traslados duplicados
			SumarTrasladosDuplicados();

			//Inicializar
			xmlDoc = new XmlDocument();
			xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));

			//Comprobante
			xml_comprobante = xmlDoc.CreateElement("cfdi:Comprobante", Xmlns);
			
			//Esquema
			//SetXmlAttribute(xml_comprobante, "xmlns:cfdi", "http://www.sat.gob.mx/cfd/3");
			//SetXmlAttribute(xml_comprobante, "xmlns:donat", "http://www.sat.gob.mx/donat");
			//SetXmlAttribute(xml_comprobante, "xmlns:implocal", "http://www.sat.gob.mx/implocal");
			//SetXmlAttribute(xml_comprobante, "xmlns:cfdi", Xmlns);
			
			SetXmlAttribute(xml_comprobante, "xmlns:xsi", Xmlns_Xsi);
			schemaLocation = xmlDoc.CreateAttribute("xsi", "schemaLocation", Xmlns_Xsi);
			schemaLocation.Value = Xsi_SchemaLocation;
			xml_comprobante.SetAttributeNode(schemaLocation);

			SetXmlAttribute(xml_comprobante, "version", Version);
			SetXmlAttribute(xml_comprobante, "serie", Serie, true);
			SetXmlAttribute(xml_comprobante, "folio", Folio);
			SetXmlAttribute(xml_comprobante, "fecha", Fecha);
			SetXmlAttribute(xml_comprobante, "sello", Sello, true); //ponemos el sello opcional, para poder generar la factura sin sello
			SetXmlAttribute(xml_comprobante, "formaDePago", FormaDePago);
			SetXmlAttribute(xml_comprobante, "noCertificado", NoCertificado);
			SetXmlAttribute(xml_comprobante, "certificado", Certificado, true);
			SetXmlAttribute(xml_comprobante, "condicionesDePago", CondicionesDePago, true);
			SetXmlAttribute(xml_comprobante, "subTotal", Subtotal);
			if(Descuento > 0) SetXmlAttribute(xml_comprobante, "descuento", Descuento, true);
			SetXmlAttribute(xml_comprobante, "motivoDescuento", MotivoDescuento, true);
			SetXmlAttribute(xml_comprobante, "total", Total);
			SetXmlAttribute(xml_comprobante, "tipoDeComprobante", TipoDeComprobante);
			SetXmlAttribute(xml_comprobante, "metodoDePago", MetodoDePago, false);
			SetXmlAttribute(xml_comprobante, "NumCtaPago", NumCtaPago, true);
			SetXmlAttribute(xml_comprobante, "LugarExpedicion", LugarExpedicion, true);
			
			xmlDoc.AppendChild(xml_comprobante);

			//Emisor
			xml_emisor = xmlDoc.CreateElement("cfdi:Emisor", Xmlns);
			SetXmlAttribute(xml_emisor, "rfc", Emisor.RFC);
			SetXmlAttribute(xml_emisor, "nombre", Emisor.Nombre);
			xml_comprobante.AppendChild(xml_emisor);

			//Regimen fiscal
			xml_reginemFiscalEmisor = xmlDoc.CreateElement("cfdi:RegimenFiscal", Xmlns);
			SetXmlAttribute(xml_reginemFiscalEmisor, "Regimen", Emisor.RegimenFiscal, false);
			xml_emisor.AppendChild(xml_reginemFiscalEmisor);

			//Domicilio del emisor
			//xml_domicilioFiscalEmisor = xmlDoc.CreateElement("DomicilioFiscal", "cfdi");
			//SetXmlAttribute(xml_domicilioFiscalEmisor, "calle", DomicilioFiscalEmisor.Calle);
			//SetXmlAttribute(xml_domicilioFiscalEmisor, "noExterior", DomicilioFiscalEmisor.NoExterior, true);
			//SetXmlAttribute(xml_domicilioFiscalEmisor, "noInterior", DomicilioFiscalEmisor.NoInterior, true);
			//SetXmlAttribute(xml_domicilioFiscalEmisor, "colonia", DomicilioFiscalEmisor.Colonia, true);
			//SetXmlAttribute(xml_domicilioFiscalEmisor, "localidad", DomicilioFiscalEmisor.Localidad, true);
			//SetXmlAttribute(xml_domicilioFiscalEmisor, "referencia", DomicilioFiscalEmisor.Referencia, true);
			//SetXmlAttribute(xml_domicilioFiscalEmisor, "municipio", DomicilioFiscalEmisor.Municipio);
			//SetXmlAttribute(xml_domicilioFiscalEmisor, "estado", DomicilioFiscalEmisor.Estado);
			//SetXmlAttribute(xml_domicilioFiscalEmisor, "pais", DomicilioFiscalEmisor.Pais);
			//SetXmlAttribute(xml_domicilioFiscalEmisor, "codigoPostal", DomicilioFiscalEmisor.CodigoPostal);
			//xml_emisor.AppendChild(xml_domicilioFiscalEmisor);

			//Receptor
			xml_receptor = xmlDoc.CreateElement("cfdi:Receptor", Xmlns);
			SetXmlAttribute(xml_receptor, "rfc", Receptor.RFC);
			SetXmlAttribute(xml_receptor, "nombre", Receptor.Nombre, true);
			xml_comprobante.AppendChild(xml_receptor);

			//Domicilio del receptor
			xml_domicilioFiscalReceptor = xmlDoc.CreateElement("cfdi:Domicilio", Xmlns);
			SetXmlAttribute(xml_domicilioFiscalReceptor, "calle", DomicilioFiscalReceptor.Calle, true);
			SetXmlAttribute(xml_domicilioFiscalReceptor, "noExterior", DomicilioFiscalReceptor.NoExterior, true);
			SetXmlAttribute(xml_domicilioFiscalReceptor, "noInterior", DomicilioFiscalReceptor.NoInterior, true);
			SetXmlAttribute(xml_domicilioFiscalReceptor, "colonia", DomicilioFiscalReceptor.Colonia, true);
			SetXmlAttribute(xml_domicilioFiscalReceptor, "localidad", DomicilioFiscalReceptor.Localidad, true);
			SetXmlAttribute(xml_domicilioFiscalReceptor, "referencia", DomicilioFiscalReceptor.Referencia, true);
			SetXmlAttribute(xml_domicilioFiscalReceptor, "municipio", DomicilioFiscalReceptor.Municipio, true);
			SetXmlAttribute(xml_domicilioFiscalReceptor, "estado", DomicilioFiscalReceptor.Estado, true);
			SetXmlAttribute(xml_domicilioFiscalReceptor, "pais", DomicilioFiscalReceptor.Pais);
			SetXmlAttribute(xml_domicilioFiscalReceptor, "codigoPostal", DomicilioFiscalReceptor.CodigoPostal, true);
			xml_receptor.AppendChild(xml_domicilioFiscalReceptor);

			//Concepto
			xml_conceptos = xmlDoc.CreateElement("cfdi:Conceptos", Xmlns);
			xml_comprobante.AppendChild(xml_conceptos);

			//Todos los conceptos
			foreach (Concepto c in Conceptos)
			{
				xml_concepto = xmlDoc.CreateElement("cfdi:Concepto", Xmlns);
				
				SetXmlAttribute(xml_concepto, "cantidad", c.Cantidad);
				SetXmlAttribute(xml_concepto, "unidad", c.Unidad, false);
				SetXmlAttribute(xml_concepto, "noIdentificacion", c.NoIdentificacion, true);
				SetXmlAttribute(xml_concepto, "descripcion", c.Descripcion);
				SetXmlAttribute(xml_concepto, "valorUnitario", c.ValorUnitario);
				SetXmlAttribute(xml_concepto, "importe", c.Importe);
				xml_conceptos.AppendChild(xml_concepto);
			}

			//Impuestos
			xml_impuestos = xmlDoc.CreateElement("cfdi:Impuestos", Xmlns);
			xml_comprobante.AppendChild(xml_impuestos);

			//Retenciones
			if (Retenciones.Count > 0)
			{
				xml_retenciones = xmlDoc.CreateElement("cfdi:Retenciones", Xmlns);
				xml_impuestos.AppendChild(xml_retenciones);

				//Todas las retenciones
				foreach (Retencion r in Retenciones)
				{
					xml_retencion = xmlDoc.CreateElement("cfdi:Retencion", Xmlns);
					SetXmlAttribute(xml_retencion, "impuesto", r.Impuesto);
					SetXmlAttribute(xml_retencion, "importe", r.Importe);
					xml_retenciones.AppendChild(xml_retencion);
				}
			}

			//Traslados
			if (Traslados.Count > 0)
			{
				xml_traslados = xmlDoc.CreateElement("cfdi:Traslados", Xmlns);
				xml_impuestos.AppendChild(xml_traslados);

				//Todos los traslados
				foreach (Traslado t in Traslados)
				{
					xml_traslado = xmlDoc.CreateElement("cfdi:Traslado", Xmlns);
					SetXmlAttribute(xml_traslado, "impuesto", t.Impuesto);
					SetXmlAttribute(xml_traslado, "tasa", t.Tasa);
					SetXmlAttribute(xml_traslado, "importe", t.Importe);
					xml_traslados.AppendChild(xml_traslado);
				}
			}

			return xmlDoc;
		}

		/// <summary>
		/// Formatea un valor decimal para ser almacenado en el XML, deacuerdo al estándar del SAT
		/// </summary>
		/// <param name="valor">Número decimal que será escrito en el XML</param>
		/// <returns>Representación en cadena de texto del valor decimal</returns>
		protected string ParseXML(decimal valor)
		{
			return valor.ToString("F2").Replace(',', '.');
		}

		/// <summary>
		/// Formatea una enumeración para ser almacenada en el XML, deacuerdo al estándar del SAT
		/// </summary>
		/// <param name="valor">Enumeración que será escrito en el XML</param>
		/// <returns>Representación en cadena de texto de la enumeración</returns>
		protected string ParseXML(Enum valor)
		{
			return valor.ToString();
		}

		/// <summary>
		/// Formatea un valor de tipo texto para ser almacenado en el XML, deacuerdo al estándar del SAT
		/// </summary>
		/// <param name="valor">Cadena de texto que será escrita en el XML</param>
		/// <returns>Representación de la cadena de texto sin espacios en los extremos, ni caracteres no permitidos</returns>
		protected string ParseXML(string valor)
		{
			if (valor == null) return null;
			
			valor = valor.Trim();
			if (valor.Trim() == "") return null;

			valor = valor.Replace("\n", "");
			valor = valor.Replace("	", "");
			valor = valor.Replace("\r", "");

			while (valor.Contains("  "))
			{
				valor = valor.Replace("  ", " ");
			}

			return valor;
		}

		/// <summary>
		/// Formatea un valor de tipo fecha para ser almacenado en el XML, deacuerdo al estándar del SAT
		/// </summary>
		/// <param name="valor">Fecha-hora que será escrita en el XML</param>
		/// <returns>Representación en cadena de texto de la fecha-hora</returns>
		protected string ParseXML(DateTime value)
		{
			return value.ToString("yyyy-MM-ddTHH:mm:ss");
		}

		/// <summary>
		/// Establece el valor de un atributo a un elemento de XML
		/// </summary>
		/// <param name="elemento">El elemento de XML al cual se le establecerá el valor de un atributo</param>
		/// <param name="nombre">Nombre del atributo</param>
		/// <param name="valor">Valor del atributo</param>
		protected void SetXmlAttribute(XmlElement elemento, string nombre, object valor)
		{
			SetXmlAttribute(elemento, nombre, valor, false);
		}

		/// <summary>
		/// Establece el valor de un atributo a un elemento de XML
		/// </summary>
		/// <param name="elemento">El elemento de XML al cual se le establecerá el valor de un atributo</param>
		/// <param name="nombre">Nombre del atributo</param>
		/// <param name="valor">Valor del atributo</param>
		/// <param name="opcional">
		///		Define si el atributo es opcional o no
		///		<remarks>Si es opcional y el valor es NULL o vacío, el atributo no se escribe. Si no es opcional y el valor es NULL o vacío, se manda un error</remarks>
		/// </param>
		protected void SetXmlAttribute(XmlElement elemento, string nombre, object valor, bool opcional)
		{
			string _valor = null;

			if (valor == null && !opcional) throw new ArgumentNullException(nombre, String.Format("La propiedad '{0}' del elemento {1} no puede ser NULL", nombre, elemento.Name));

			if (valor is string) _valor = ParseXML((string)valor);
			else if (valor is decimal) _valor = ParseXML((decimal)valor);
			else if (valor is Enum) _valor = ParseXML((Enum)valor);
			else if (valor is DateTime) _valor = ParseXML((DateTime)valor);
			else if (valor != null) _valor = valor.ToString();

			//Si despues del Parse, es NULL, marcar error
			if (_valor == null && !opcional) throw new ArgumentNullException(nombre, String.Format("La propiedad '{0}' del elemento {1} no puede ser NULL", nombre, elemento.Name));

			//Si es nulo y es opcional, no hacer nada
			if (_valor == null && opcional) return;

			elemento.SetAttribute(nombre, _valor);
		}

		/// <summary>
		/// Busca traslados duplicados por concepto de IVA, ISR u otro tipo de traslado, suma todos los totales y deja
		/// 1 solo traslado con el total. Se usa para evitar tener multiples registros de traslado con IVA en la factura
		/// </summary>
		public void SumarTrasladosDuplicados()
		{
			//arreglo que contiene los totales de retenciones agrupados por impuesto y tasa
			//el primer campo es el impuesto, el segundo la tasa y el tercero el total
			decimal[, ,] totales = new decimal[4, 100, 1];

			//sumar traslados similares
			foreach (Traslado t in Traslados)
			{
				totales[(int) t.Impuesto, (int) (t.Tasa * 100), 0] += t.Importe;
			}

			//limpiar traslados
			Traslados.Clear();

			//crear traslados con los totales
			for (int impuesto = 1; impuesto <= 3 ; impuesto++)
			{
				for (int tasa = 0; tasa < 100; tasa++)
				{
					if (totales[impuesto, tasa, 0] > 0)
					{
						Traslado t = new Traslado();
						t.Impuesto = (Impuesto)impuesto;
						t.Tasa = ((decimal) tasa) / 100;
						t.Importe = totales[impuesto, tasa, 0];

						Traslados.Add(t);
					}
				}
			}
		}

		#endregion

		/// <summary>
		/// Devuelve el número de factura incluyendo la serie
		/// </summary>
		/// <example>A100</example>
		public override string ToString()
		{
			return NoFactura;
		}

		/// <summary>
		/// Crea la cadena original de la factura y la guarda en la carpeta temporal
		/// </summary>
		protected void CrearCadenaOriginal()
		{
			XPathDocument pathDoc;
			XslCompiledTransform xslTrans;
			XmlTextWriter writer;
			string cadena;

			pathDoc = new XPathDocument(String.Format("{0}{1}_sin_sello.xml", CarpetaTemporal, NoFactura));
			xslTrans = new XslCompiledTransform();
			writer = new XmlTextWriter(String.Format("{0}{1}_cadena_original_temp.txt", CarpetaTemporal, NoFactura), Encoding.UTF8);

			//Guardar como texto
			xslTrans.Load(CarpetaRecursos + "CadenaOriginal.xslt");
			xslTrans.Transform(pathDoc, null, writer);
			writer.Close();

			//Abrir, convertir a UTF8 y volver a guardar como bytes
			cadena = File.ReadAllText(String.Format("{0}{1}_cadena_original_temp.txt", CarpetaTemporal, this));
			cadena = cadena.Trim();

			//Corregir probolema con RFC que contiene & por ejemplo P&M030108GL1, se genera el sello como "P &amp; M030108GL1"
			//debido al XML, hay que revertir eso
			cadena = cadena.Replace("&amp;", "&");

			File.WriteAllBytes(String.Format("{0}{1}_cadena_original.txt", CarpetaTemporal, NoFactura), Encoding.UTF8.GetBytes(cadena));
		}

		/// <summary>
		/// Crea el sello de la factura yu lo guyarda en la carpeta temporal
		/// </summary>
		protected void CrearSello()
		{
			ProcessStartInfo processStart;
			System.Diagnostics.Process process;
			string prefijoTemporal = CarpetaTemporal + NoFactura;

			#region Crear cadena original, convertirla a UTF-8 y guardarla

			CrearCadenaOriginal();

			#endregion

			#region Generar archivo MD5

			processStart = new ProcessStartInfo();
			processStart.WindowStyle = ProcessWindowStyle.Normal;

			processStart.FileName = CarpetaRecursos + "openssl.exe";

			processStart.Arguments =
				"dgst -sha1 -sign " +
				"\"" +
				CarpetaRecursos +
				"LlavePrivada.key.pem" +
				"\"" +
				" -out " +
				"\"" +
				prefijoTemporal + "_sha1.txt" +
				"\" " +
				"\"" +
				prefijoTemporal + "_cadena_original.txt" +
				"\"";

			process = new System.Diagnostics.Process();
			process.StartInfo = processStart;
			process.EnableRaisingEvents = true;
			process.Start();


			while (!process.HasExited)
			{
				System.Threading.Thread.Sleep(1000);
			}

			
			//check to see what the exit code was
			if (process.ExitCode != 0)
			{
				throw new Exception("Error al generar el sello");
			}

			#endregion

			#region Generar sello

			processStart.Arguments =
				"enc -base64 -in " +
				"\"" +
				prefijoTemporal + "_sha1.txt" +
				"\"" +
				" -out " +
				"\"" +
				prefijoTemporal + "_sello.txt" +
				"\"";


			process = new System.Diagnostics.Process();
			process.StartInfo = processStart;
			process.EnableRaisingEvents = true;
			process.Start();

			while (!process.HasExited)
			{
				System.Threading.Thread.Sleep(1000);
			}

			//check to see what the exit code was
			if (process.ExitCode != 0)
			{
				throw new Exception("Error al generar el sello");
			}

			#endregion
		}

		/// <summary>
		/// Crea una nueva instancia de esta clase
		/// </summary>
		public Factura()
		{
		}

		/// <summary>
		/// Genera un archivo XML con la factura electrónica
		/// </summary>
		/// <returns>La ruta donde fué generada la factura en formato XML</returns>
		public string GuardarXml()
		{
			XmlDocument xml;
			RutaXML = String.Format("{0}{1}.xml", CarpetaFacturas, NoFactura);

			//Primero crear XML sin el sello
			xml = CrearXML();
			xml.Save(String.Format("{0}{1}_sin_sello.xml", CarpetaTemporal, NoFactura));

			//Crear sello
			CrearSello();
			this.Sello = File.ReadAllText(String.Format("{0}{1}_sello.txt", CarpetaTemporal, NoFactura));

			//Crear y guardar XML sellado
			xml = CrearXML();
			xml.Save(RutaXML);
			
			//timbrado
			WSForcogsaClient timbradoWS = new WSForcogsaClient();
			wsAutenticarResponse auth = timbradoWS.Autenticar("RIAP671201", "O062SC4FC");
			wsTimbradoResponse timbre = timbradoWS.Timbrar(xml.OuterXml, auth.token);

			if (string.IsNullOrWhiteSpace(timbre.cfdi))
			{
				throw new Exception("No se pudo timbrar la factura: " + timbre.mensaje);
			}

			xml.LoadXml(timbre.cfdi);
			xml.Save(RutaXML);

			return RutaXML;
		}

		/// <summary>
		/// Genera un archivo imprimible HTML
		/// </summary>
		/// <returns>La ruta donde fué generada la factura en formato HTML</returns>
		public string GuardarHtml()
		{
			XPathDocument pathDoc;
			XslCompiledTransform xslTrans;
			XmlTextWriter writer;
			string html;
			string cadenaOriginal;

			pathDoc = new XPathDocument(String.Format("{0}{1}.xml", CarpetaFacturas, NoFactura));
			xslTrans = new XslCompiledTransform();
			writer = new XmlTextWriter(String.Format("{0}{1}.html", CarpetaFacturas, NoFactura), Encoding.Unicode);

			//Guardar como texto
			xslTrans.Load(CarpetaRecursos + "ConvertirHTML.xslt");
			xslTrans.Transform(pathDoc, null, writer);
			writer.Close();

			RutaHTML = String.Format("{0}{1}.html", CarpetaFacturas, NoFactura);
			
			//Poner la cadena original
			html = File.ReadAllText(RutaHTML);
			cadenaOriginal = File.ReadAllText(String.Format("{0}{1}_cadena_original.txt", CarpetaTemporal, NoFactura), Encoding.UTF8);
			html = html.Replace("<CadenaOriginal />", cadenaOriginal);
			File.WriteAllText(RutaHTML, html, Encoding.Unicode);

			return RutaHTML;
		}

		private class ComparadorDeTraslados: IEqualityComparer<Traslado>
		{
			#region IEqualityComparer<Traslado> Members

			public bool Equals(Traslado x, Traslado y)
			{
				return x.Impuesto == y.Impuesto && x.Tasa == y.Tasa;
			}

			public int GetHashCode(Traslado obj)
			{
				decimal suma = ((decimal) obj.Impuesto) + obj.Tasa;
				return suma.GetHashCode();
			}

			#endregion
		}
	}
}