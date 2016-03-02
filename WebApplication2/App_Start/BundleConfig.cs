using System.Web;
using System.Web.Optimization;

namespace WebApplication2
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery.unobtrusive-ajax.min.js",
                    "~/Scripts/jquery.validate*",
                    "~/Scripts/moment.min.js"
                   ));

            bundles.Add(new ScriptBundle("~/bundles/ratings").Include(
                        "~/Theme/rating/js/star-rating.min.js",
                        "~/Theme/js/prettify.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/respond.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-rtl").Include(
           "~/Scripts/bootstrap-rtl.js",
           "~/Scripts/respond.min.js"));

            bundles.Add(new StyleBundle("~/Content/css-rtl").Include(
                      "~/Content/bootstrap-rtl.css"));

            //bundles.Add(new StyleBundle("~/Theme/css").Include(
            //          "~/Theme/dropdownMultiple/semantic.min.css",
            //          "~/Theme/css/bootstrap.min.css",
            //          "~/Theme/css/freelancer.css",
            //          "~/Theme/css/custom.css",
            //          "~/Theme/font-awesome/css/font-awesome.min.css",
            //          "~/Content/bootstrap-datetimepicker.min.css",
            //          "~/Theme/rating/css/star-rating.css"));

            BundleTable.EnableOptimizations = true;
        }
    }
}
