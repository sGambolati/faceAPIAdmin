namespace FaceRecMVC.Controllers
{
    using System.Configuration;
    using System.Web.Mvc;
    using Microsoft.ProjectOxford.Face;

    public class HomeController : Controller
    {
        private IFaceServiceClient faceServiceClient = new FaceServiceClient(ConfigurationManager.AppSettings["FaceAPIKey"]);

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}