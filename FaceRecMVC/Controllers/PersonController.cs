namespace FaceRecMVC.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using FaceRecMVC.Models;
    using Microsoft.ProjectOxford.Face;

    public class PersonController : Controller
    {
        private FaceServiceClient faceServiceClient;

        public PersonController()
        {
            this.faceServiceClient = new FaceServiceClient(ConfigurationManager.AppSettings["FaceAPIKey"]);
        }

        public async Task<ActionResult> Index()
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
            return View();
        }

        public async Task<JsonResult> GetPersons(string personGroupID)
        {
            var persons = await this.faceServiceClient.GetPersonsAsync(personGroupID);
            ViewBag.PersonGroupID = personGroupID;

            return Json(new
            {
                View = RenderRazorViewToString("GetPersons", persons.Select(x => new Person()
                {
                    Id = x.PersonId,
                    CompleteName = x.Name,
                    Data = x.UserData,
                    FaceCount = x.PersistedFaceIds.Count()
                }).ToList())
            }, JsonRequestBehavior.AllowGet);
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public async Task<ActionResult> Create()
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
            var personModel = new Person();

            return View(personModel);
        }

        [HttpPost]
        public async Task<ActionResult> Create(Person person)
        {
            person.CompleteName = person.CompleteName ?? this.Request["CompleteName"];

            var personResult = await faceServiceClient.CreatePersonAsync(person.PersonGroupID, person.CompleteName, person.Data);

            for (int i = 0; i < this.Request.Files.Count; i++)
            {
                await faceServiceClient.AddPersonFaceAsync(person.PersonGroupID, personResult.PersonId, this.Request.Files[i].InputStream);
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Details(string PersonGroupID, string PersonID)
        {
            var person = await faceServiceClient.GetPersonAsync(PersonGroupID, Guid.Parse(PersonID));
            var personModel = new Person()
            {
                PersonGroupID = PersonGroupID,
                Id = person.PersonId,
                CompleteName = person.Name,
                Data = person.UserData,
                FaceCount = person.PersistedFaceIds.Count()
            };

            return View(personModel);
        }

        public async Task<JsonResult> Delete(string PersonGroupID, string PersonID)
        {
            try
            {
                await faceServiceClient.DeletePersonAsync(PersonGroupID, Guid.Parse(PersonID));
                return Json(new { ok = "ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (FaceAPIException ex)
            {
                return Json(new { error = ex.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}