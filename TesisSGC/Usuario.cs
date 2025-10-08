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
    [StringLength(100)]
    public string NombreUsuario { get; set; }

    [Required]
    [StringLength(255)]
    public string Contrasena { get; set; }

    [Required]
    public string Rol { get; set; }

    public string Email { get; set; }

    // FK opcional
    public int ? IdSocio { get; set; }

    [ForeignKey("IdSocio")]
    public Socio Socio { get; set; }
}



