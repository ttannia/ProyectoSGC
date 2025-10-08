using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TesisSGC;

    public class Socio
    {
        [Key]
        public int IdSocio { get; set; }

        [Required]
        public string NombreSocio { get; set; }

        [Required]
        public int CI { get; set; }

        public string Direccion { get; set; }

        public int TelSocio { get; set; }

        [Required]
        //[Display(Name = "Activo")]
        public bool Estado { get; set; } = true;

        public virtual Cuenta Cuenta { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>(); //xq da error el scaffolding

    }
