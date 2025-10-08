using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace TesisSGC.ViewModels
{
    public class PagoSocioEditViewModel
    {
        public int IdPagoSocio { get; set; }
        public int IdSocio { get; set; }
        public int IdCuotaMensual { get; set; }

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

        // ⚡ Datos de la cuota
        public DateTime MesCuota { get; set; }
        public string TipoCuota { get; set; }
        public decimal MontoCuota { get; set; } // ← aquí lo agregamos

        public string NombreSocio { get; set; }
        public string CI { get; set; }

        public decimal SaldoPendienteURSinEste { get; set; }
        public decimal ValorUR { get; set; }

        // ⚡ Método de ayuda para convertir Monto a decimal de manera segura
        public bool TryGetMontoDecimal(out decimal montoDecimal)
        {
            return decimal.TryParse(
                Monto?.Replace(',', '.'), // reemplaza coma por punto si viene así
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out montoDecimal
            );
        }
    }




}