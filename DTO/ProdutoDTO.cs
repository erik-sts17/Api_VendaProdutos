using Desafio_Api.Models;
using System.Collections.Generic;


namespace Desafio_Api.DTO
{
    public class ProdutoDTO
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public string CodigoProduto { get; set; }
        public double Valor { get; set; }
        public bool Promocao { get; set; }
        public double ValorPromocao { get; set; }
        public string Categoria { get; set; }
        public long Quantidade { get; set; }
        public string Imagem { get; set; }
        public Fornecedor Fornecedor { get; set; }

        public Produto Converte(ProdutoDTO produto)
        {
            Produto produtoTemporario = new Produto();
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
            return produtoTemporario;
        }

    }
}