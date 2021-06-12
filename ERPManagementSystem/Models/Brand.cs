﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ERPManagementSystem.Models
{
    public class Brand
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string BrandStatus { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
