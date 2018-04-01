using System.Web;
using System.Web.Optimization;

namespace MusicBox
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.unobtrusive-ajax.js",
                        "~/Scripts/jquery-ui.min.js"));

            bundles.Add(new StyleBundle("~/Media/jqueryui").Include(
                        "~/Content/jquery-ui.css"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/musicbox").Include(
                        "~/Scripts/musicbox.js"));

            // Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información. De este modo, estará
            // para la producción, use la herramienta de compilación disponible en https://modernizr.com para seleccionar solo las pruebas que necesite.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Media/bootstrap").Include(
                        "~/Content/bootstrap.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Media/css").Include(
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Media/jplayer").Include(          
                    "~/Content/JPlayer/blue.monday/css/jplayer.blue.monday.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/jplayer").Include(           
                    "~/Scripts/jquery.jplayer.min.js"));

            bundles.Add(new StyleBundle("~/Media/datatable").Include(
                    "~/Content/DataTable/datatables.min.css",
                    "~/Content/DataTable/dataTables.material.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/datatable").Include(
                    "~/Scripts/datatables.min.js",
                    "~/Scripts/dataTables.material.min.js"));

            bundles.Add(new StyleBundle("~/Media/pure").Include(
                "~/Content/base-min.css",
                "~/Content/pure-min.css",
                "~/Content/buttons-min.css",
                "~/Content/forms-min.css"));

            bundles.Add(new ScriptBundle("~/bundles/uploader").Include(
                "~/Scripts/jquery.dm-uploader.min.js",
                "~/Scripts/uploader.js"));

            bundles.Add(new StyleBundle("~/Media/uploader").Include(
                "~/Content/jquery.dm-uploader.min.css"));
        }
    }
}
