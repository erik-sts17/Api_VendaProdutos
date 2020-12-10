using System.Collections.Generic;
using Desafio_Api.DTO;
using Desafio_Api.Container;
using System.Text.Json.Serialization;

namespace Desafio_Api.Models
{
    public class Produto
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public string CodigoProduto { get; set; }
        public double Valor { get; set; }
        public bool Promocao { get; set; }
        public double ValorPromocao { get; set; }
        public string Categoria { get; set; }
        public string Imagem { get; set; }
        public long Quantidade { get; set; }
        public Fornecedor Fornecedor { get; set; }
        public List<ProdutoVenda> Vendas { get; set; }
        public bool Status { get; set; }

        public ProdutoContainer ConverteDTO(Produto produto)
        {
            ProdutoDTO produtoTemporario = new ProdutoDTO();
            produtoTemporario.Id = produto.Id;
            produtoTemporario.Nome = produto.Nome;
            produtoTemporario.Valor = produto.Valor;
            produtoTemporario.CodigoProduto = produto.CodigoProduto;
            produtoTemporario.Fornecedor = produto.Fornecedor;
            produtoTemporario.Quantidade = produto.Quantidade;
            produtoTemporario.Imagem = produto.Imagem;
            produtoTemporario.Promocao = produto.Promocao;
            produtoTemporario.ValorPromocao = produto.ValorPromocao;
            produtoTemporario.Categoria = produto.Categoria;
            ProdutoContainer produtoHateoas = new ProdutoContainer();
            produtoHateoas.Produto = produtoTemporario;
            return produtoHateoas ;
        }
    }
}