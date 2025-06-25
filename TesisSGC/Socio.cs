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

    public int? IdUsuario { get; set; }

    [ForeignKey("IdUsuario")]
    public virtual Usuario Usuario { get; set; }

    public virtual Cuenta Cuenta { get; set; }

    public virtual ICollection<CuotaMensual> CuotasMensuales { get; set; }
}
