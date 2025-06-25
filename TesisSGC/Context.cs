using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;


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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ✅ Socio - Cuenta (1:1 con clave compartida)
            modelBuilder.Entity<Socio>()
                .HasRequired(s => s.Cuenta)
                .WithRequiredPrincipal(c => c.Socio);

            // ✅ Socio - Usuario (1 a 0..1) usando FK IdUsuario
            modelBuilder.Entity<Socio>()
                .HasOptional(s => s.Usuario)
                .WithMany()
                .HasForeignKey(s => s.IdUsuario)
                .WillCascadeOnDelete(false);

            // ✅ Socio - CuotasMensuales (1 a muchos)
            modelBuilder.Entity<Socio>()
                .HasMany(s => s.CuotasMensuales)
                .WithRequired(cm => cm.Socio)
                .HasForeignKey(cm => cm.IdSocio);

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

            base.OnModelCreating(modelBuilder);
        }
    }
}
