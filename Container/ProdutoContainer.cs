using Desafio_Api.DTO;
using Desafio_Api.HATEOAS;

namespace Desafio_Api.Container
{
    public class ProdutoContainer
    {
        public ProdutoDTO Produto{ get; set; }
        public Link[] Links { get; set; }
    }
}