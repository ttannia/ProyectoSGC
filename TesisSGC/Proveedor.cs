using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisSGC
{
    public class Proveedor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProveedor { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; }

        [Required]
        [Range(1000000, 999999999999, ErrorMessage = "El RUT debe tener entre 7 y 12 dígitos.")]
        public long RUT { get; set; }

        public string Direccion { get; set; }

        [Required]
        public string Telefono { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public bool Estado { get; set; } = true; // Soft delete

    }
}
