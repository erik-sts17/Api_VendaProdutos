using System.Text.Json;
using Desafio_Api.DTO;
using Desafio_Api.HATEOAS;

namespace Desafio_Api.Container
{
    public class ClienteContainer
    {
        public ClienteDTO Cliente{get; set;}
        public Link[] Links { get; set; }
    }
}