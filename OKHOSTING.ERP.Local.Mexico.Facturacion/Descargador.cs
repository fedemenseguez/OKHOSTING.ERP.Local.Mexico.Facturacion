using OKHOSTING.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WatiN.Core;
using WatiN.Core.DialogHandlers;
using WatiN.Core.Native.Windows;

namespace OKHOSTING.ERP.Local.Mexico.Facturacion
{
    public static class Descargador
    {
        public enum TipoBusqueda
        {
            Emitidas,
            Recibidas,
        }

		public static void Descargar(string rfc, string contrasena, string carpeta, DateTime fechaDesde, DateTime fechaHasta, TipoBusqueda busqueda)
        {
			using (IE browser = new IE())
			{
				//limpiar sesion y login 
				browser.ClearCookies();
				Thread.Sleep(1000);

				//java login
				browser.GoTo("https://portalcfdi.facturaelectronica.sat.gob.mx");
				browser.WaitForComplete();

				//entrar por contraseña
				browser.GoTo("https://cfdiau.sat.gob.mx/nidp/app/login?id=SATUPCFDiCon&sid=0&option=credential&sid=0");
				browser.TextField(Find.ByName("Ecom_User_ID")).AppendText(rfc);
				browser.TextField(Find.ByName("Ecom_Password")).AppendText(contrasena);
				browser.Button("submit").Click();

				browser.WaitForComplete();

				//ver si nos pudimos loggear
				if (browser.ContainsText("Login failed, please try again") || browser.ContainsText("La entrada no se ha completado"))
				{
					browser.Close();
					throw new Exception("Los datos de acceso son incorrectos para: " + rfc);
				}

				//seleccionar emitidas o recibidas
				if (busqueda == TipoBusqueda.Emitidas)
				{
					browser.RadioButton("ctl00_MainContent_RdoTipoBusquedaEmisor").Click();
				}
				else
				{
					browser.RadioButton("ctl00_MainContent_RdoTipoBusquedaReceptor").Click();
				}

				browser.Button("ctl00_MainContent_BtnBusqueda").Click();

				Log.Write("Tipo busqueda", Log.Information);

				//Creating the directory if it doesn't exists
				if (!System.IO.Directory.Exists(carpeta))
				{
					System.IO.Directory.CreateDirectory(carpeta);
				}

				//facturas emitidas
				if (busqueda == TipoBusqueda.Emitidas)
				{
					browser.WaitUntilContainsText("Fecha Inicial de Emisión");
					browser.RadioButton("ctl00_MainContent_RdoFechas").Click();
					Thread.Sleep(1000);

					//fecha desde
					browser.TextField("ctl00_MainContent_CldFechaInicial2_Calendario_text").Value = fechaDesde.ToString("dd/MM/yyyy");
					//hasta
					browser.TextField("ctl00_MainContent_CldFechaFinal2_Calendario_text").Value = fechaHasta.ToString("dd/MM/yyyy");
					Thread.Sleep(1000);

					//buscar
					browser.Button("ctl00_MainContent_BtnBusqueda").Click();

					Log.Write("Buscando", Log.Information);

					DescargarFacturasListadas(browser, carpeta);
				}
				else
				{
					DateTime mesActual = fechaDesde;
					bool primeraVez = true;

					while (mesActual < fechaHasta)
					{
						browser.WaitUntilContainsText("Fecha de Emisión");
						browser.RadioButton("ctl00_MainContent_RdoFechas").Click();
						Thread.Sleep(1000);

						//seleccionar año adecuado
						browser.SelectList("DdlAnio").SelectByValue(mesActual.Year.ToString());
						//seleccionar mes adecuado
						browser.SelectList("ctl00_MainContent_CldFecha_DdlMes").SelectByValue(mesActual.Month.ToString());

						if (mesActual.Day < 10 && primeraVez)
						{
							//seleccionar dia adecuado
							//click en buscar por que si no on jala
							browser.Button("ctl00_MainContent_BtnBusqueda").Click();
							Thread.Sleep(1000);
							primeraVez = false;
						}

						browser.SelectList("ctl00_MainContent_CldFecha_DdlDia").SelectByValue(mesActual.Day.ToString("00"));
						Thread.Sleep(1000);

						//buscar
						browser.Button("ctl00_MainContent_BtnBusqueda").Click();

						Log.Write("Buscando", Log.Information);

						DescargarFacturasListadas(browser, carpeta);

						//pasar al siguiente mes
						mesActual = mesActual.AddDays(1);
					}
				}

				browser.Link("ctl00_LnkBtnCierraSesion").Click();
				Thread.Sleep(2000);
				browser.Close();
			}
        }

        private static void DescargarFacturasListadas(IE browser, string carpeta)
        {
			//esperar resultados
			int intentos = 0;

			//Creating the directory if it doesn't exists
			if (!System.IO.Directory.Exists(carpeta))
			{
				System.IO.Directory.CreateDirectory(carpeta);
			}

			while (intentos < 20)
			{
				Log.Write("Intento " + intentos, Log.Information);

				//ya hay resultados
				if (browser.Div("ctl00_MainContent_PnlResultados").Style.Display == "inline")
				{
					break;
				}
				
				//no hay resultados
				if (browser.Div("ctl00_MainContent_PnlNoResultados").Style.Display == "inline")
				{
					return;
				}

				intentos++;
				Thread.Sleep(1000);
			}

			Log.Write("Descargando", Log.Information);

            foreach (var link in browser.Images.Where(img => img.Name == "BtnDescarga"))
            {
                //obtener folio fiscal
                string folio = link.Parent.Parent.NextSibling.Text;
                string archivo = String.Format("{0}.xml", folio);
				string rutaCompleta = Path.Combine(carpeta, archivo);
				
				//si ya esta descargada, no la brincamos
				if (File.Exists(rutaCompleta))
				{
					continue;
				}

                //download xml
                link.Click();

				Log.Write("Click Descargando " + archivo, Log.Information);

				
				FileDownloadHandler fileDownloadHandler = new FileDownloadHandler(rutaCompleta);
				browser.AddDialogHandler(fileDownloadHandler);

				try
				{
					fileDownloadHandler.WaitUntilFileDownloadDialogIsHandled(30);
					fileDownloadHandler.WaitUntilDownloadCompleted(200);
				}
				catch
				{
					throw;
				}
				finally
				{
					browser.RemoveDialogHandler(fileDownloadHandler);
				}
            }
        }
    }
}