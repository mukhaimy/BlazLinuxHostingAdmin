using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blaz9LinuxHostingAdmin.Models.Dbases
{
    [Table(name: "the_demo")]
    public class TheDemo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int BirthYear { get; set; } = 1950;


        public static List<TheDemo> SampleSet()
        {
            List<TheDemo> hsl = new List<TheDemo>();
            hsl.Add(new TheDemo { Name = "Masahide Akagi", BirthYear = 1989 });
            hsl.Add(new TheDemo { Name = "Rosemary Thompson", BirthYear = 1997 });
            hsl.Add(new TheDemo { Name = "Nikki Fox", BirthYear = 1990 });
            hsl.Add(new TheDemo { Name = "Nick Vincent", BirthYear = 2004 });
            hsl.Add(new TheDemo { Name = "Gerald Frye", BirthYear = 1993 });
            hsl.Add(new TheDemo { Name = "Tadashi Uehara", BirthYear = 2001 });
            hsl.Add(new TheDemo { Name = "Ricardo Glass", BirthYear = 2001 });
            hsl.Add(new TheDemo { Name = "Gavin Richardson", BirthYear = 1988 });
            hsl.Add(new TheDemo { Name = "Lily Gomez", BirthYear = 1994 });
            hsl.Add(new TheDemo { Name = "Dina Murillo", BirthYear = 1997 });
            hsl.Add(new TheDemo { Name = "Tara Phillips", BirthYear = 2000 });

            return hsl;
        }
    }
}
