using System;
using Desafio_Api.DTO;
using Desafio_Api.Container;
using System.Text.Json.Serialization;

namespace Desafio_Api.Models
{
    public class Cliente
    {
        public long Id { get; set; }
        
        public string Nome { get; set; }
        
        public string Email { get; set; }
       
        public string Senha { get; set; }
       
        public string Documento { get; set; }
       
        public DateTime DataCadastro { get; set; }
        
        public bool Status { get; set; }

        public ClienteContainer ConverteDTO(Cliente cliente)
        {
            ClienteDTO clienteTemporario = new ClienteDTO();
            clienteTemporario.Id = cliente.Id;
            clienteTemporario.Nome = cliente.Nome;
            clienteTemporario.Documento = cliente.Documento;
            clienteTemporario.Email = cliente.Email;
            clienteTemporario.Senha = cliente.Senha;
            clienteTemporario.DataCadastro =  clienteTemporario.DataCadastro;
            ClienteContainer clienteHateoas = new ClienteContainer();
            clienteHateoas.Cliente = clienteTemporario;
            return clienteHateoas;
        }
    }
}