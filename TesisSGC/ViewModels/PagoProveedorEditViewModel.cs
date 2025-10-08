using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace TesisSGC.ViewModels
{
    public class PagoProveedorEditViewModel
    {
        public int IdPagoProveedor { get; set; }
        public int IdProveedor { get; set; }

        public DateTime FechaPago { get; set; }

        [Required(ErrorMessage = "El campo Monto es obligatorio.")]
        [RegularExpression(@"^\d+([,\.]\d{1,2})?$", ErrorMessage = "El Monto debe ser un número válido (ej: 123 o 123,45).")]
        [Display(Name = "Monto en pesos")]
        public string Monto { get; set; } // ⚡ String para poder validar letras manualmente

        public string FormaPago { get; set; }
        public string TipoPago { get; set; }
        public string NumeroTransferencia { get; set; }
        public string NumeroCheque { get; set; }
        public string NumeroTransaccion { get; set; }
        public string Observaciones { get; set; }

        public string NombreProveedor { get; set; }
        public string RUTProveedor { get; set; }  


        // ⚡ Método de ayuda para convertir Monto a decimal de manera segura
        public bool TryGetMontoDecimal(out decimal montoDecimal)
        {
            return decimal.TryParse(
                Monto?.Replace(',', '.'),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out montoDecimal
            );
        }
    }
}
