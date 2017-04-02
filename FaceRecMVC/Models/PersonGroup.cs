using System.ComponentModel.DataAnnotations;

namespace FaceRecMVC.Models
{
    public class PersonGroup
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Data { get; set; }
        public string TrainState { get; set; }
    }
}