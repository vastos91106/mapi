using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MAPI.ViewModels
{
    public class MarkViewModel
    {
        [Required]
        public string name;
        public string avatar;
        [Required]
        [Range(double.MinValue, double.MaxValue)]
        public double lat;
        [Required]
        [Range(double.MinValue, double.MaxValue)]
        public double lon;
        [Required]
        public string description;
    }

    public class GeoPoint
    {
        [Required]
        public double lat;
        [Required]
        public double lon;
        [Required]
        public double radius;
    }
}