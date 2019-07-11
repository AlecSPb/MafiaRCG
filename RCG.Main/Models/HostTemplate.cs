using System;
using System.Collections.Generic;
using System.Text;

namespace RCG.Main.Models
{
    public class HostTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsDoctor { get; set; } = false;
        public bool IsDetective { get; set; } = false;
        public bool IsGirl { get; set; } = false;
        public bool IsManiac { get; set; } = false;
        public bool IsImmortal { get; set; } = false;
        public bool IsDon { get; set; } = false;
    }
}
