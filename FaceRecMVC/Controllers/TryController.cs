using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.ProjectOxford.Face;

namespace FaceRecMVC.Controllers
{
    public class TryController : Controller
    {
        private FaceServiceClient faceServiceClient;

        public TryController()
        {
            this.faceServiceClient = new FaceServiceClient(ConfigurationManager.AppSettings["FaceAPIKey"]);
        }

        // GET: Try
        public async Task<ActionResult> Index()
        {
            await SetPersonGroups();
            return View();
        }

        private async Task SetPersonGroups()
        {
            var personGroup = await this.faceServiceClient.ListPersonGroupsAsync();
            List<SelectListItem> listModel = new List<SelectListItem>
            {
                new SelectListItem() { Value = string.Empty, Text = "-" }
            };
            listModel.AddRange(personGroup.Select(x => new SelectListItem()
            {
                Value = x.PersonGroupId,
                Text = x.Name
            }).ToList());

            ViewBag.PersonGroups = listModel;
        }

        [HttpPost]
        public async Task<ActionResult> Index(string PersonGroupID)
        {
            if (this.Request.Files.Count > 0)
            {
                var faces = await faceServiceClient.DetectAsync(this.Request.Files[0].InputStream, true, true);
                var identities = await faceServiceClient.IdentifyAsync(PersonGroupID, faces.Select(x => x.FaceId).ToArray());

                ViewBag.Identities = identities;
                await SetPersonGroups();

                foreach (var identity in identities)
                {
                }
            }
            return View();
        }
    }
}