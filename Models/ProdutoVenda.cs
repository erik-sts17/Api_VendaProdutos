namespace Desafio_Api.Models
{
    public class ProdutoVenda
    {
        public long VendaId { get; set; }
        public Venda Venda { get; set; }
        public long ProdutoId { get; set; }
        public Produto Produto { get; set; }
    }
}