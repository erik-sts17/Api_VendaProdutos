using Microsoft.EntityFrameworkCore.Migrations;

namespace Desafio_Api.Migrations
{
    public partial class PoupulaBanco : Migration
    {
        protected override void Up(MigrationBuilder mb)
        {
            mb.Sql("INSERT INTO Clientes(Nome, Email, Senha, Documento, DataCadastro, Status) VALUES('Erik Rocha', 'erik@gmail.com','12345678','48069599685', '2020-11-09 13:26:35.113720', '1')");
            mb.Sql("INSERT INTO Fornecedores(Nome, Cnpj, Status) VALUES('Apple', '12111222555578','1')");
            mb.Sql("INSERT INTO Produtos(Nome, CodigoProduto, Valor, Promocao, ValorPromocao, Categoria, Imagem, Quantidade, FornecedorId, Status) VALUES('Iphone', 'Iphone12','2600.00', '1', '100.00', 'Eletronicos', 'iphone.png', '3', (SELECT Id FROM Fornecedores WHERE Nome='Apple'), '1')");
            mb.Sql("INSERT INTO Produtos(Nome, CodigoProduto, Valor, Promocao, ValorPromocao, Categoria, Imagem, Quantidade, FornecedorId, Status) VALUES('MacBook Pro', 'Macbook45','8000.00', '0', '1000.00', 'Eletronicos', 'macbook.png', '2', (SELECT Id FROM Fornecedores WHERE Nome='Apple'), '1')");
            mb.Sql("INSERT INTO Vendas(FornecedorId, ClienteId, TotalCompra, DataCompra, Status) VALUES((SELECT Id FROM Fornecedores WHERE Nome='Apple'), (SELECT Id FROM Clientes WHERE Nome='Erik Rocha'),'10500.00',  '2020-11-09 14:26:35.113720', '1' )");
            mb.Sql("INSERT INTO ProdutosVendas(VendaId, ProdutoId) VALUES((SELECT Id FROM Vendas WHERE TotalCompra='2500.00'), (SELECT Id FROM Produtos WHERE Nome='Iphone'))");
            mb.Sql("INSERT INTO ProdutosVendas(VendaId, ProdutoId) VALUES((SELECT Id FROM Vendas WHERE TotalCompra='2500.00'), (SELECT Id FROM Produtos WHERE Nome='MacBook Pro'))");
        }

        protected override void Down(MigrationBuilder mb)
        {

        }
    }
}
