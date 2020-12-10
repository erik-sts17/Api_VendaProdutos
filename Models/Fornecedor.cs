using Desafio_Api.Container;
using Desafio_Api.HATEOAS;
using Desafio_Api.DTO;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Desafio_Api.Models
{
    public class Fornecedor
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public string Cnpj { get; set; }
        public bool Status { get; set; }

        public FornecedorContainer ConverteDTO(Fornecedor fornecedor)
        {
            FornecedorDTO fornecedorTemporario = new FornecedorDTO();
            fornecedorTemporario.Id = fornecedor.Id;
            fornecedorTemporario.Nome = fornecedor.Nome;
            fornecedorTemporario.Cnpj = fornecedor.Cnpj;
            FornecedorContainer fornecedorHateoas = new FornecedorContainer();
            fornecedorHateoas.Fornecedor = fornecedorTemporario;
            return fornecedorHateoas;
        }
    }
}