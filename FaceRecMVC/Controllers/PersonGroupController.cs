using System.Configuration;
namespace FaceRecMVC.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using FaceRecMVC.Models;
    using Microsoft.ProjectOxford.Face;

    public class PersonGroupController : Controller
    {
        private IFaceServiceClient faceServiceClient;

        public PersonGroupController()
        {
            this.faceServiceClient = new FaceServiceClient(ConfigurationManager.AppSettings["FaceAPIKey"]);
        }

        // GET: PersonGroup
        public async Task<ActionResult> Index()
        {
            var personGroups = await faceServiceClient.ListPersonGroupsAsync();

            var personGroupsModel = personGroups.Select(x =>
                                        new PersonGroup()
                                        {
                                            Id = x.PersonGroupId,
                                            Name = x.Name,
                                            Data = x.UserData
                                        }).ToList();

            foreach (var item in personGroupsModel)
            {
                await GetTrainingStatus(item);
            }

            return View(personGroupsModel);
        }

        private async Task GetTrainingStatus(PersonGroup item)
        {
            try
            {
                var trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(item.Id);
                item.TrainState = trainingStatus.Status.ToString();
            }
            catch (FaceAPIException ex)
            {
                if (ex.ErrorCode == "PersonGroupNotTrained")
                {
                    item.TrainState = "Not trained";
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<string> GetTrainingStatus(string personGroupID)
        {
            try
            {
                var trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupID);
                return trainingStatus.Status.ToString();
            }
            catch (FaceAPIException ex)
            {
                if (ex.ErrorCode == "PersonGroupNotTrained")
                {
                    return "Not trained";
                }
                else
                {
                    throw;
                }
            }
        }

        public ActionResult Create()
        {
            var personGroupModel = new PersonGroup();

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(PersonGroup model)
        {
            await faceServiceClient.CreatePersonGroupAsync(model.Id, model.Name, model.Data);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Details(string id)
        {
            var personGroup = await faceServiceClient.GetPersonGroupAsync(id);
            var personGroupModel = new PersonGroup() { Id = personGroup.PersonGroupId, Name = personGroup.Name, Data = personGroup.UserData };
            await GetTrainingStatus(personGroupModel);

            return View(personGroupModel);
        }

        public async Task<ActionResult> Delete(string id)
        {
            await faceServiceClient.DeletePersonGroupAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Train(string id)
        {
            await faceServiceClient.TrainPersonGroupAsync(id);
            return RedirectToAction("Index");
        }
    }
}