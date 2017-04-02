using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.ProjectOxford.Face;

namespace FaceRecMVC.Controllers
{
    public class HomeController : Controller
    {
        private IFaceServiceClient faceServiceClient = new FaceServiceClient(ConfigurationManager.AppSettings["FaceAPIKey"]);

        public async Task<ActionResult> Index()
        {
            try
            {
                /* await CreatePerson("sw-devs", "Sebastian Gambolati", new string[] {
                    @"https://scontent-eze1-1.xx.fbcdn.net/v/t1.0-9/11694841_10206117605591471_7703376993283907563_n.jpg?oh=50a35c8017e11110f2a283ca698f4eb8&oe=59545148", 
                    //@"https://scontent-eze1-1.xx.fbcdn.net/v/t1.0-9/17703_10205676504804227_7443063180723020580_n.jpg?oh=1733daa41d8329523967ee503fb436c6&oe=595018A4", 
                    //@"https://scontent-eze1-1.xx.fbcdn.net/v/t1.0-9/10947164_10204739129450429_5362351769264811306_n.jpg?oh=541b46a210ffea7dc54d7eb5c0c17776&oe=595A18DB",
                    @"https://scontent-eze1-1.xx.fbcdn.net/v/t31.0-8/s960x960/10604507_10203764522245858_4182557418727001718_o.jpg?oh=63f963e22128c02865a63deaae907bd8&oe=59629BAA",
                    @"https://scontent-eze1-1.xx.fbcdn.net/v/t1.0-9/534972_10201088744153078_1214339502_n.jpg?oh=9e4d2eadcc0b189d5e27d33f585fd27a&oe=5955C398"
                }); */
                /*
                await CreatePerson("sw-devs", "Saira Ruiz", new string[]
                {
                    @"https://scontent-eze1-1.xx.fbcdn.net/v/t1.0-9/936609_10200577543075926_1474857779_n.jpg?oh=9a8dad484e62f29e2f9d9593debf6819&oe=5967C9AD",
                    @"https://scontent-eze1-1.xx.fbcdn.net/v/t1.0-9/580309_10201972716931845_175089767_n.jpg?oh=11fcb2a07d96c687c518fc8ad7c56df7&oe=5955D948",
                    @"https://scontent-eze1-1.xx.fbcdn.net/v/t1.0-9/1928279_1101877742143_528588_n.jpg?oh=d649245017b9841e4b9ffc3c6d3ad280&oe=5950BBC9",
                    @"https://scontent-eze1-1.xx.fbcdn.net/v/t1.0-9/223206_1049544953856_9242_n.jpg?oh=a1d16da1b8b4bd82f83da11760a5d485&oe=59967F3C",
                    @"https://scontent-eze1-1.xx.fbcdn.net/v/t1.0-9/222631_1049544913855_8670_n.jpg?oh=623492c0b4ed76e09376611d6c5eab13&oe=599A4B06"
                }); */
            }
            catch (FaceAPIException ex)
            {
                Console.WriteLine($"Error: ({ex.ErrorCode}) {ex.ErrorMessage}");
            }

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

        public async Task CreatePerson(string personGroupID, string personName, string[] imageURLs)
        {
            try
            {
                var personGroup = await faceServiceClient.GetPersonGroupAsync(personGroupID);
            }
            catch (Exception ex)
            {
                //if (personGroup != null)
                //{
                    await faceServiceClient.CreatePersonGroupAsync(personGroupID, "Developers");
                    Console.WriteLine($"Person group created: {personGroupID}");
                //}
            }

            Guid personID;
            var persons = await faceServiceClient.GetPersonsAsync(personGroupID);
            if (persons != null && persons.Any(x => x.Name == personName))
            {
                personID = persons.First(x => x.Name == "Sebastian Gambolati").PersonId;
                Console.WriteLine("Already exists you.");
            }
            else
            {
                var personResult = await faceServiceClient.CreatePersonAsync(personGroupID, "Sebastian Gambolati", "Thats me");
                Console.WriteLine($"You are the PersonID: {personResult.PersonId}");
                personID = personResult.PersonId;

                foreach (var imageURL in imageURLs)
                {
                    try
                    {
                        await faceServiceClient.AddPersonFaceAsync(personGroupID, personID, imageURL);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"We encounter the error '{ex.Message}' while processing the imageURL: '{imageURL}'");
                    }
                }
            }

            await faceServiceClient.TrainPersonGroupAsync(personGroupID);
            
            var groupTrainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupID);
            while (groupTrainingStatus.Status == Microsoft.ProjectOxford.Face.Contract.Status.Running)
            {
                Thread.Sleep(1000);
                groupTrainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupID);
            }

            var DectectedFaces = await faceServiceClient.DetectAsync(
                //@"https://scontent-eze1-1.xx.fbcdn.net/v/t1.0-9/10347644_10204389179341895_3195444796875027458_n.jpg?oh=4acb69a5147626183f04a7dcf1c2e3b5&oe=59541E29" //yo
                @"https://scontent-eze1-1.xx.fbcdn.net/v/t1.0-9/1012662_10202236880975781_61695851_n.jpg?oh=06ad3e51af0a6a028d9c63f86fac0769&oe=595BABEC"
                );

            var identityResults = await faceServiceClient.IdentifyAsync(personGroupID, DectectedFaces.Select(x => x.FaceId).ToArray());

            foreach (var identity in identityResults)
            {
                Console.WriteLine($"FaceID: {identity.FaceId}");
                foreach (var candidate in identity.Candidates)
                {
                    Console.WriteLine($"\t\t{candidate.PersonId}: {candidate.Confidence} %");
                }
            }
        }
    }
}