using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using static System.Web.Razor.Parser.SyntaxConstants;
using System.Threading.Tasks;
using System.Threading;


namespace TesisSGC
{
    public class Context : DbContext
    {
        public Context() : base("name=CooperativaConnectionString") { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Socio> Socios { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<CuotaMensual> CuotasMensuales { get; set; }
        public DbSet<UR> URs { get; set; }
        public DbSet<PagoSocio> PagosSocios { get; set; }
        public DbSet<Comprobante> Comprobantes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<PagoProveedor> PagosProveedores { get; set; }
        public DbSet<ComprobanteProveedor> ComprobantesProveedores { get; set; }
        public DbSet<HistorialPagoSocio> HistorialPagosSocios { get; set; }
        public DbSet<HistorialPagoProveedor> HistorialPagosProveedores { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ✅ Socio - Cuenta (1:1 con clave compartida)
            modelBuilder.Entity<Socio>()
                .HasRequired(s => s.Cuenta)
                .WithRequiredPrincipal(c => c.Socio);


            modelBuilder.Entity<Usuario>()
                .HasOptional(u => u.Socio)
                .WithMany(s => s.Usuarios)
                .HasForeignKey(u => u.IdSocio)
                .WillCascadeOnDelete(false);


            // ✅ Cuenta - CuotasMensuales (1 a muchos)
            modelBuilder.Entity<Cuenta>()
                .HasMany(c => c.CuotasMensuales)
                .WithRequired(cm => cm.Cuenta)
                .HasForeignKey(cm => cm.IdCuenta);

            // ✅ UR - CuotasMensuales (1 a muchos)
            modelBuilder.Entity<UR>()
                .HasMany(u => u.CuotasMensuales)
                .WithRequired(cm => cm.UR)
                .HasForeignKey(cm => cm.IdUR);

            modelBuilder.Entity<Usuario>()
                .Property(u => u.NombreUsuario)
                .HasColumnAnnotation(
                "Index",
                new IndexAnnotation(new IndexAttribute("IX_NombreUsuario") { IsUnique = true }));

            modelBuilder.Entity<Socio>()
                .Property(s => s.CI)
                .HasColumnAnnotation(
                "Index",
                new IndexAnnotation(new IndexAttribute("IX_CI") { IsUnique = true }));

            modelBuilder.Entity<UR>()
                .Property(u => u.Mes)
                .HasColumnAnnotation(
                "Index",
                new IndexAnnotation(new IndexAttribute("IX_MES") { IsUnique = true }));

            // Configuración PagoSocio - Socio
            modelBuilder.Entity<PagoSocio>()
                .HasRequired(p => p.Socio)
                .WithMany()  
                .HasForeignKey(p => p.IdSocio)
                .WillCascadeOnDelete(false);

            // Configuración PagoSocio - CuotaMensual
            modelBuilder.Entity<PagoSocio>()
                .HasRequired(p => p.CuotaMensual)
                .WithMany()  
                .HasForeignKey(p => p.IdCuotaMensual)
                .WillCascadeOnDelete(false);

            // ✅ Comprobante
            modelBuilder.Entity<Comprobante>()
                .HasRequired(c => c.PagoSocio)
                .WithMany() // o .WithMany(p => p.Comprobantes) si agregás la colección en PagoSocio
                .HasForeignKey(c => c.IdPagoSocio)
                .WillCascadeOnDelete(false);

            // Único por pago (1:1 lógico)
            modelBuilder.Entity<Comprobante>()
                .Property(c => c.IdPagoSocio)
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("IX_Comprobante_IdPagoSocio") { IsUnique = true })
                );

            // Número de comprobante único
            modelBuilder.Entity<Comprobante>()
                .Property(c => c.NumeroComprobante)
                .HasMaxLength(20)
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("IX_Comprobante_Numero") { IsUnique = true })
                );

            // Proveedor - RUT único
            modelBuilder.Entity<Proveedor>()
                .Property(p => p.RUT)
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("IX_Proveedor_RUT") { IsUnique = true })
            );

            // PagoProveedor - Proveedor (1:N)
            modelBuilder.Entity<PagoProveedor>()
                .HasRequired(pp => pp.Proveedor)
                .WithMany() 
                .HasForeignKey(pp => pp.IdProveedor)
                .WillCascadeOnDelete(false);

            // ✅ ComprobanteProveedor
            modelBuilder.Entity<ComprobanteProveedor>()
                .HasRequired(c => c.PagoProveedor)
                .WithMany() 
                .HasForeignKey(c => c.IdPagoProveedor)
                .WillCascadeOnDelete(false);

            // Único por pago (1:1 lógico)
            modelBuilder.Entity<ComprobanteProveedor>()
                .Property(c => c.IdPagoProveedor)
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("IX_ComprobanteProveedor_IdPagoProveedor") { IsUnique = true })
                );

            // Número de comprobante único
            modelBuilder.Entity<ComprobanteProveedor>()
                .Property(c => c.NumeroComprobante)
                .HasMaxLength(20)
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("IX_ComprobanteProveedor_Numero") { IsUnique = true })
                );
            
            ///A VER SI ESTO VA O NOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO
            // Caja - MovimientoCaja (1:N)
            modelBuilder.Entity<Caja>()
                .HasMany(c => c.Movimientos)
                .WithRequired(m => m.Caja)
                .HasForeignKey(m => m.IdCaja)
                .WillCascadeOnDelete(false); // Evita borrar la caja si borrás movimientos

            // MovimientoCaja puede tener un PagoSocio opcional
            modelBuilder.Entity<MovimientoCaja>()
                .HasOptional(m => m.PagoSocio)
                .WithMany()
                .HasForeignKey(m => m.IdPagoSocio)
                .WillCascadeOnDelete(false);

            // MovimientoCaja puede tener un PagoProveedor opcional
            modelBuilder.Entity<MovimientoCaja>()
                .HasOptional(m => m.PagoProveedor)
                .WithMany()
                .HasForeignKey(m => m.IdPagoProveedor)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }

        // 🚨 NUEVO: Interceptar SaveChanges
        public override int SaveChanges()
        {
            AgregarHistorial();
            AgregarMovimientosCaja();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AgregarHistorial();
            AgregarMovimientosCaja();
            return await base.SaveChangesAsync(cancellationToken);
        }


        private void AgregarHistorial()
        {
            var now = DateTime.Now;

            // Historial de Pagos Socios
            foreach (var entry in ChangeTracker.Entries<PagoSocio>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var historial = new HistorialPagoSocio
                    {
                        IdPagoSocio = entry.Entity.IdPagoSocio,
                        IdSocio = entry.Entity.IdSocio,
                        IdCuotaMensual = entry.Entity.IdCuotaMensual,
                        Monto = entry.Entity.Monto,
                        FechaPago = entry.Entity.FechaPago,
                        FormaPago = entry.Entity.FormaPago,
                        TipoPago = entry.Entity.TipoPago,
                        NumeroTransferencia = entry.Entity.NumeroTransferencia,
                        NumeroCheque = entry.Entity.NumeroCheque,
                        NumeroTransaccion = entry.Entity.NumeroTransaccion,
                        Observaciones = entry.Entity.Observaciones,
                        Accion = entry.State == EntityState.Added ? "Creado" :
                                 entry.State == EntityState.Modified ? "Editado" : "Eliminado",
                        FechaAccion = now
                    };

                    HistorialPagosSocios.Add(historial);
                }
            }

            // Historial de Pagos Proveedores
            foreach (var entry in ChangeTracker.Entries<PagoProveedor>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var historial = new HistorialPagoProveedor
                    {
                        IdPagoProveedor = entry.Entity.IdPagoProveedor,
                        IdProveedor = entry.Entity.IdProveedor,
                        Monto = entry.Entity.Monto,
                        FechaPago = entry.Entity.FechaPago,
                        FormaPago = entry.Entity.FormaPago,
                        TipoPago = entry.Entity.TipoPago,
                        NumeroTransferencia = entry.Entity.NumeroTransferencia,
                        NumeroCheque = entry.Entity.NumeroCheque,
                        NumeroTransaccion = entry.Entity.NumeroTransaccion,
                        Observaciones = entry.Entity.Observaciones,
                        Accion = entry.State == EntityState.Added ? "Creado" :
                                 entry.State == EntityState.Modified ? "Editado" : "Eliminado",
                        FechaAccion = now
                    };

                    HistorialPagosProveedores.Add(historial);
                }
            }
        }

        private void AgregarMovimientosCaja()
        {
            var now = DateTime.Now;

            // Buscar la caja
            var caja = Cajas.FirstOrDefault(c => c.IdCaja == 1);
            if (caja == null)
            {
                caja = new Caja
                {
                    FechaApertura = now,
                    SaldoInicial = 0,
                    SaldoEfectivo = 0,
                    SaldoBanco = 0
                };
                Cajas.Add(caja);
            }

            // Entradas de pagos de socios
            foreach (var entry in ChangeTracker.Entries<PagoSocio>())
            {
                if (entry.State == EntityState.Added)
                {
                    var medio = entry.Entity.FormaPago.ToLower() == "efectivo" ? "Efectivo" : "Banco";
                    var socio = this.Socios.Find(entry.Entity.IdSocio);

                    MovimientosCaja.Add(new MovimientoCaja
                    {
                        Fecha = entry.Entity.FechaPago,
                        Monto = entry.Entity.Monto,
                        EsEntrada = true,
                        Medio = medio,
                        Descripcion = "Pago socio C.I. " + (socio != null ? socio.CI : entry.Entity.IdSocio),
                        IdPagoSocio = entry.Entity.IdPagoSocio,
                        IdCaja = caja.IdCaja
                    });

                    if (medio == "Efectivo") caja.SaldoEfectivo += entry.Entity.Monto;
                    else caja.SaldoBanco += entry.Entity.Monto;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    // Borrar el movimiento asociado
                    var mov = MovimientosCaja.FirstOrDefault(m => m.IdPagoSocio == entry.Entity.IdPagoSocio);
                    if (mov != null) MovimientosCaja.Remove(mov);

                    // Ajustar saldo
                    if (mov != null)
                    {
                        if (mov.Medio == "Efectivo") caja.SaldoEfectivo -= mov.Monto;
                        else caja.SaldoBanco -= mov.Monto;
                    }
                }
            }

            // Salidas de pagos a proveedores
            foreach (var entry in ChangeTracker.Entries<PagoProveedor>())
            {
                if (entry.State == EntityState.Added)
                {
                    var medio = entry.Entity.FormaPago.ToLower() == "efectivo" ? "Efectivo" : "Banco";
                    var proveedor = this.Proveedores.Find(entry.Entity.IdProveedor);

                    MovimientosCaja.Add(new MovimientoCaja
                    {
                        Fecha = entry.Entity.FechaPago,
                        Monto = entry.Entity.Monto,
                        EsEntrada = false,
                        Medio = medio,
                        Descripcion = "Pago proveedor RUT " + (proveedor != null ? proveedor.RUT : entry.Entity.IdProveedor),
                        IdPagoProveedor = entry.Entity.IdPagoProveedor,
                        IdCaja = caja.IdCaja
                    });

                    if (medio == "Efectivo") caja.SaldoEfectivo -= entry.Entity.Monto;
                    else caja.SaldoBanco -= entry.Entity.Monto;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    // Borrar el movimiento asociado
                    var mov = MovimientosCaja.FirstOrDefault(m => m.IdPagoProveedor == entry.Entity.IdPagoProveedor);
                    if (mov != null) MovimientosCaja.Remove(mov);

                    // Ajustar saldo
                    if (mov != null)
                    {
                        if (mov.Medio == "Efectivo") caja.SaldoEfectivo += mov.Monto;
                        else caja.SaldoBanco += mov.Monto;
                    }
                }
            }
        }


    }
}
