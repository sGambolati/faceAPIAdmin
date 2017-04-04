using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using FaceRecMVC.Models;
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

        private Color[] colors = new Color[] {
            Color.Chocolate, Color.Aqua, Color.Beige,
            Color.Blue, Color.Chartreuse, Color.Coral,
            Color.Crimson, Color.Cyan, Color.DarkKhaki,
            Color.Green
        };

        [HttpPost]
        public async Task<ActionResult> Index(string PersonGroupID)
        {
            if (this.Request.Files.Count > 0)
            {
                var faceDetectImageStream = new MemoryStream();
                var ImageResultStream = new MemoryStream();

                await this.Request.Files[0].InputStream.CopyToAsync(faceDetectImageStream);
                faceDetectImageStream.Position = 0;

                await faceDetectImageStream.CopyToAsync(ImageResultStream);
                ImageResultStream.Position = 0;
                faceDetectImageStream.Position = 0;

                var faces = await faceServiceClient.DetectAsync(faceDetectImageStream, true, true);
                var identities = await faceServiceClient.IdentifyAsync(PersonGroupID, faces.Select(x => x.FaceId).ToArray());

                IList<FaceDetectedViewModel> facesDetected = new List<FaceDetectedViewModel>();

                int colorIndex = 0;
                foreach (var face in faces)
                {
                    facesDetected.Add(new FaceDetectedViewModel()
                    {
                        FaceID = face.FaceId,
                        RectColor = colors[colorIndex++]
                    });
                }

                Image image = Image.FromStream(ImageResultStream);
                using (Graphics g = Graphics.FromImage(image))
                {
                    foreach (var identity in identities)
                    {
                        var faceDetected = facesDetected.First(x => x.FaceID == identity.FaceId);
                        var face = faces.First(x => x.FaceId == identity.FaceId);

                        var pen = new Pen(faceDetected.RectColor, 5);
                        g.DrawRectangle(pen, new Rectangle(face.FaceRectangle.Left, face.FaceRectangle.Top, face.FaceRectangle.Width, face.FaceRectangle.Height));

                        foreach (var candidate in identity.Candidates)
                        {
                            var person = await faceServiceClient.GetPersonAsync(PersonGroupID, candidate.PersonId);

                            faceDetected.PersonDetected.Add(new PersonDetectedViewModel()
                            {
                                PersonID = candidate.PersonId,
                                PersonName = person.Name,
                                UserData = person.UserData,
                                Coincidence = candidate.Confidence
                            });
                        }
                    }
                }

                await SetPersonGroups();

                ViewBag.ImageResult = DataUriContent(image, this.Request.Files[0].FileName);
                return View(facesDetected);
            }

            return RedirectToAction("Index");
        }

        public static string DataUriContent(Image image, string fileName)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                
                return $"data:image/" +
                        Path.GetExtension(fileName).Replace(".", "")
                        + ";base64,"
                        + Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}