
using System;
using System.ComponentModel.DataAnnotations;

namespace TesisSGC.ViewModels
{
    public class UREditViewModel
    {
        public int IdUR { get; set; }

        [Required(ErrorMessage = "El campo Mes es obligatorio.")]
        [DataType(DataType.Date)]
        [Display(Name = "Mes")]
        public DateTime Mes { get; set; }

        
        [Required(ErrorMessage = "El campo Monto es obligatorio.")]
        [RegularExpression(@"^\d+([,\.]\d{1,2})?$", ErrorMessage = "El Monto debe ser un número válido")]
        [Display(Name = "Monto")]
        public string Monto { get; set; } // se cambia a string para que funcione regularexpression
    }
}