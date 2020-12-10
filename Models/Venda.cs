using System.Collections.Generic;
using System;
using Desafio_Api.DTO;
using Desafio_Api.Container;

namespace Desafio_Api.Models
{
    public class Venda
    {
        public Venda()
        {
            Produtos = new List<ProdutoVenda>();
        }
        public long Id { get; set; }
        public Fornecedor Fornecedor { get; set; }
        public Cliente Cliente { get; set; }
        public List<ProdutoVenda> Produtos { get; set; }
        public double TotalCompra { get; set; }
        public DateTime DataCompra { get; set; }
        public bool Status { get; set; }

        public VendasContainer ConverteDTO(Venda venda)
        {
            VendaDTO vendaTemporaria = new VendaDTO();
            vendaTemporaria.Id = venda.Id;
            vendaTemporaria.Cliente = venda.Cliente;
            vendaTemporaria.Fornecedor = venda.Fornecedor;
            vendaTemporaria.TotalCompra = venda.TotalCompra;
            vendaTemporaria.DataCompra = venda.DataCompra.ToShortDateString();
            for (int i = 0; i < venda.Produtos.Count; i++)
            {
                ProdutoDTO produtoTemporario = new ProdutoDTO();
                var produto = venda.Produtos[i].Produto;
                produtoTemporario.Id = produto.Id;
                produtoTemporario.Nome = produto.Nome;
                produtoTemporario.Valor = produto.Valor;
                produtoTemporario.Promocao = produto.Promocao;
                produtoTemporario.ValorPromocao = produto.ValorPromocao; 
                produtoTemporario.CodigoProduto = produto.CodigoProduto;
                produtoTemporario.Categoria = produto.Categoria;
                produtoTemporario.Quantidade = produto.Quantidade;
                produtoTemporario.Imagem = produto.Imagem;
                produtoTemporario.Fornecedor = produto.Fornecedor;
                vendaTemporaria.Produtos.Add(produtoTemporario);
            }
            VendasContainer vendaHateoas = new VendasContainer();
            vendaHateoas.Venda = vendaTemporaria;
            return vendaHateoas;
        }
    }
}