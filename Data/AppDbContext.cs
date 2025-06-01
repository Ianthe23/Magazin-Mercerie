using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace magazin_mercerie
{
    public class AppDbContext : DbContext
    {
        public DbSet<Produs> Produse { get; set; }
        public DbSet<Comanda> Comenzi { get; set; }
        public DbSet<ComandaProdus> ComandaProduse { get; set; }
        public DbSet<Client> Clienti { get; set; }
        public DbSet<Angajat> Angajati { get; set; }
        public DbSet<AngajatMagazin> AngajatiMagazin { get; set; }
        public DbSet<Patron> Patroni { get; set; }
        public DbSet<User> Utilizatori { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public AppDbContext() 
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=magazin-mercerie.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Produs>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Comanda>()
                .HasKey(c => c.Id);
            
            modelBuilder.Entity<Comanda>()
                .HasOne(c => c.Client)
                .WithMany(c => c.Comenzi)
                .HasForeignKey(c => c.ClientId);

            modelBuilder.Entity<Comanda>()
                .HasOne(c => c.Angajat)
                .WithMany(a => a.Comenzi)
                .HasForeignKey(c => c.AngajatId);

            // Configure ComandaProdus junction table with quantity and price
            modelBuilder.Entity<ComandaProdus>()
                .HasKey(cp => new { cp.ComandaId, cp.ProdusId });

            modelBuilder.Entity<ComandaProdus>()
                .Property(cp => cp.ProdusId)
                .HasColumnName("ProduseId");

            modelBuilder.Entity<ComandaProdus>()
                .HasOne(cp => cp.Comanda)
                .WithMany(c => c.ComandaProduse)
                .HasForeignKey(cp => cp.ComandaId);

            modelBuilder.Entity<ComandaProdus>()
                .HasOne(cp => cp.Produs)
                .WithMany()
                .HasForeignKey(cp => cp.ProdusId);

            // Ignore the computed Produse property to prevent unwanted FK relationship
            modelBuilder.Entity<Comanda>()
                .Ignore(c => c.Produse);

            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);
        }
    }
}
