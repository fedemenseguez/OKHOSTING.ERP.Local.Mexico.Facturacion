using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WatiN.Core;
using WatiN.Core.DialogHandlers;

namespace OKHOSTING.ERP.Local.Mexico.Facturacion
{
    public class Descargador
    {
        public enum TipoBusqueda
        {
            Emitidas,
            Recibidas,
        }

        protected IE Browser;

        /// <summary>
        /// RFC para entrar al SAT
        /// </summary>
        public readonly string RFC;

        /// <summary>
        /// Contraseña para entrar al SAT
        /// </summary>
        public readonly string Contrasena;

        /// <summary>
        /// Carpeta donde se guardarán las facturas
        /// </summary>
        public readonly string Carpeta;

        /// <summary>
        /// Desde que fecha descargar (solo se usan mes y año, el dia se ignora)
        /// </summary>
        public readonly DateTime FechaDesde;

        /// <summary>
        /// Hasta que fecha descargar (solo se usan mes y año, el dia se ignora)
        /// </summary>
        public readonly DateTime FechaHasta;

        /// <summary>
        /// Define si descargar facturas emitidas o recibidas
        /// </summary>
        public readonly TipoBusqueda Busqueda;

        public Descargador(string rfc, string contrasena, string carpeta, DateTime fechaDesde, DateTime fechaHasta, TipoBusqueda busqueda)
        {
            RFC = rfc;
            Contrasena = contrasena;
            Carpeta = carpeta;
            FechaDesde = fechaDesde;
            FechaHasta = fechaHasta;
            Busqueda = busqueda;
        }

        public void Descargar()
        {
            Browser = new IE();

            //limpiar sesion y login 
            Browser.ClearCookies();
            Thread.Sleep(1000);

            //java login
            Browser.GoTo("https://portalcfdi.facturaelectronica.sat.gob.mx");
            Browser.WaitForComplete();

            //entrar por contraseña
            Browser.GoTo("https://cfdiau.sat.gob.mx/nidp/app/login?id=SATUPCFDiCon&sid=0&option=credential&sid=0");
            Browser.TextField(Find.ByName("Ecom_User_ID")).AppendText(RFC);
            Browser.TextField(Find.ByName("Ecom_Password")).AppendText(Contrasena);
            Browser.Button("submit").Click();

            //seleccionar emitidas o recibidas
            Browser.WaitForComplete();
            if (Busqueda == TipoBusqueda.Emitidas)
            {
                Browser.RadioButton("ctl00_MainContent_RdoTipoBusquedaEmisor").Click();
            }
            else
            {
                Browser.RadioButton("ctl00_MainContent_RdoTipoBusquedaReceptor").Click();
            }

            Browser.Button("ctl00_MainContent_BtnBusqueda").Click();

            //facturas emitidas
            if (Busqueda == TipoBusqueda.Emitidas)
            {
                Browser.WaitUntilContainsText("Fecha Inicial de Emisión");
                Browser.RadioButton("ctl00_MainContent_RdoFechas").Click();

                //fecha desde
                Browser.TextField("ctl00_MainContent_CldFechaInicial2_Calendario_text").Value = FechaDesde.ToString("dd/MM/yyyy");
                //hasta
                Browser.TextField("ctl00_MainContent_CldFechaFinal2_Calendario_text").Value = FechaHasta.ToString("dd/MM/yyyy");

                //buscar
                Browser.Button("ctl00_MainContent_BtnBusqueda").Click();

                DescargarFacturasListadas();
            }
            else
            {
                DateTime mesActual = FechaDesde;

                while (mesActual < FechaHasta)
                {
                    Browser.WaitUntilContainsText("Fecha de Emisión");
                    Browser.RadioButton("ctl00_MainContent_RdoFechas").Click();

                    //seleccionar año adecuado
                    Browser.SelectList("DdlAnio").SelectByValue(mesActual.Year.ToString());

                    //seleccionar mes adecuado
                    Browser.SelectList("ctl00_MainContent_CldFecha_DdlMes").SelectByValue(mesActual.Month.ToString());

                    //buscar
                    Browser.Button("ctl00_MainContent_BtnBusqueda").Click();

                    DescargarFacturasListadas();

                    //pasar al siguiente mes
                    mesActual = mesActual.AddMonths(1);
                }
            }

        }

        protected void DescargarFacturasListadas()
        {
            //paginacion
            Thread.Sleep(2000);
            Browser.WaitUntilContainsText("Acciones");

            foreach (var link in Browser.Images.Where(img => img.Name == "BtnDescarga"))
            {
                string directory = Path.Combine(Carpeta, RFC, "Emitidas");

                //obtener folio fiscal
                string folio = link.Parent.Parent.NextSibling.Text;
                string filename = String.Format("{0}.xml", folio);

                //Creating the directory if it doesn't exists
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                //download xml
                link.Click();
                FileDownloadHandler fileDownloadHandlerPdf = new FileDownloadHandler(Path.Combine(directory, filename));
                Browser.AddDialogHandler(fileDownloadHandlerPdf);

                try
                {
                    fileDownloadHandlerPdf.WaitUntilFileDownloadDialogIsHandled(30);
                    fileDownloadHandlerPdf.WaitUntilDownloadCompleted(200);
                }
                catch
                {
                }
                finally
                {
                    Browser.RemoveDialogHandler(fileDownloadHandlerPdf);
                }
            }
        }
    }
}