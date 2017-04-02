namespace FaceRecMVC.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Person
    {
        [Required]
        public string PersonGroupID { get; set; }
        public Guid Id { get; internal set; }
        [Required]
        public string CompleteName { get; internal set; }
        public string Data { get; set; }
        public int FaceCount { get; internal set; }
    }
}