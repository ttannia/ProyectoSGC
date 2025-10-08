using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TesisSGC.ViewModels
{
    public class CuotaMensualEditViewModel
    {
        // Propiedades esenciales para identificar y procesar la cuota
        public int IdCuotaMensual { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un socio.")]
        [Display(Name = "Socio")]
        public int IdSocio { get; set; }

        [Required(ErrorMessage = "El campo Mes es obligatorio.")]
        [DataType(DataType.Date)]
        [Display(Name = "Mes")]
        public DateTime Mes { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de cuota.")]
        [Display(Name = "Tipo de Cuota")]
        public string Tipo { get; set; }

        // --- EL CAMBIO CLAVE ESTÁ AQUÍ ---
        [Required(ErrorMessage = "El campo Monto es obligatorio.")]
        [RegularExpression(@"^\d+([,\.]\d{1,2})?$", ErrorMessage = "El Monto debe ser un número válido (ej: 123 o 123.45).")]
        [Display(Name = "Monto")]
        public string Monto { get; set; } // ¡Ahora es un string!

        [Required(ErrorMessage = "Debe indicar si la cuota está pagada.")]
        [Display(Name = "Estado de Pago")]
        public bool EstaPagada { get; set; }

        // Propiedades adicionales para llenar los DropDownLists en la vista
        public IEnumerable<SelectListItem> Socios { get; set; }
        public IEnumerable<SelectListItem> TiposDeCuota { get; set; }
        public IEnumerable<SelectListItem> EstadosDePago { get; set; }
    }
}