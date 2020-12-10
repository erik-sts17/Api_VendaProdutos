using Desafio_Api.DTO;
using Desafio_Api.HATEOAS;

namespace Desafio_Api.Container
{
    public class VendasContainer
    {
        public VendaDTO Venda{ get; set; }
        public Link[] Links { get; set; }
        
    }
}