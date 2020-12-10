using Desafio_Api.Models;
using System.Collections.Generic;
using System;

namespace Desafio_Api.DTO
{
    public class VendaDTO
    {
        public VendaDTO()
        {
            Produtos = new List<ProdutoDTO>();
        }
        public long Id { get; set; }
        public Fornecedor Fornecedor { get; set; }
        public Cliente Cliente { get; set; }
        public List<ProdutoDTO> Produtos{ get; set; }
        public double TotalCompra { get; set; }
        public string DataCompra { get; set; }

    }
}