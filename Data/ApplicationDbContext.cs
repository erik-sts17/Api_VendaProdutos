using Microsoft.EntityFrameworkCore;
using Desafio_Api.Models;

namespace Desafio_Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Cliente> Clientes {get;set;}
        public DbSet<Fornecedor> Fornecedores {get;set;}
        public DbSet<Produto> Produtos {get;set;}
        public DbSet<Venda> Vendas {get;set;}
        public DbSet<ProdutoVenda> ProdutosVendas {get;set;}

        
         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProdutoVenda>().HasKey(p => new{p.ProdutoId, p.VendaId});
            modelBuilder.Entity<ProdutoVenda>().HasOne(t => t.Produto).WithMany(f => f.Vendas).HasForeignKey(t => t.ProdutoId);
            modelBuilder.Entity<ProdutoVenda>().HasOne(t => t.Venda).WithMany(f => f.Produtos).HasForeignKey(t => t.VendaId);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}