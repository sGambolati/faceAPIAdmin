using System;
using System.Collections.Generic;
using System.Drawing;

namespace FaceRecMVC.Models
{
    public class PersonDetectedViewModel
    {
        public Guid PersonID { get; set; }
        public string PersonName { get; set; }
        public string UserData { get; set; }
        public double Coincidence { get; set; }
    }

    public class FaceDetectedViewModel
    {
        public Color RectColor { get; set; }
        public Guid FaceID { get; set; }

        public IList<PersonDetectedViewModel> PersonDetected { get; set; }

        public FaceDetectedViewModel()
        {
            this.PersonDetected = new List<PersonDetectedViewModel>();
        }
    }
}