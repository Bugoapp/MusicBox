using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicBox.Models
{
    public class Song
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public String Artist { get; set; }
        public String Length { get; set; }
        public String Filename { get; set; }
        public long LengthSeconds { get; set; }
    }
}