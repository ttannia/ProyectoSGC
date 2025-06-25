using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Usuario
{
    [Key]
    public int IdUsuario { get; set; }

    [Required]
    public string NombreUsuario { get; set; }

    [Required]
    public string Contrasena { get; set; }

    [Required]
    public string Rol { get; set; }

    public string Email { get; set; }

    // Usuario NO tiene navegación a Socio porque la relación es opcional desde Socio
}
