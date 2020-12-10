using Microsoft.AspNetCore.Mvc;
using Desafio_Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Desafio_Api.DTO;
using Desafio_Api.Models;
using System.Collections.Generic;
using Desafio_Api.HATEOAS;
using Desafio_Api.Container;
using Microsoft.AspNetCore.Authorization;

namespace Desafio_Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Api/v1/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly ApplicationDbContext _database;

        private Hateoas Hateoas;
    
        public ProdutosController(ApplicationDbContext database)
        {
            _database = database;
            Hateoas = new Hateoas("localhost:5001/api/v1/Produtos");
            Hateoas.AddAction("BUSCAR_PRODUTO", "GET");
            Hateoas.AddAction("DELETAR_PRODUTO", "DELETE");
            Hateoas.AddAction("EDITAR_PRODUTO", "PUT");
            Hateoas.AddAction("EDITAR_PRODUTO", "PATCH");
        }

        [HttpPost]
        public IActionResult Post([FromBody]ProdutoDTO produtoTemporario)
        {   
            try
            {
                Produto produto = new Produto();
                if (produtoTemporario.Nome.Length == 0)
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new{msg = "Nome: Mínimo um caractere"});
                }
                else
                {
                    produto.Nome = produtoTemporario.Nome;
                }

                if (produtoTemporario.Valor <= 0.0)
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new{msg = "Valor inválido"});
                }
                else
                {
                    produto.Valor = produtoTemporario.Valor;
                }

                if (produtoTemporario.Imagem.Length < 5)
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new{msg = "Imagem: Mínimo cinco caracteres"});
                }
                else
                {
                    if (produtoTemporario.Imagem.Contains("."))
                    {
                        produto.Imagem = produtoTemporario.Imagem;
                    }
                    else
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg = "Insira a extensão da imagem. Exemplo: .png"});
                    }
                    
                }

                if (produtoTemporario.ValorPromocao > 0.0 && produtoTemporario.ValorPromocao <= produtoTemporario.Valor)
                {
                    produto.ValorPromocao = produtoTemporario.ValorPromocao;
                }
                else
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new{msg = "Valor da promoção inválido"});
                }
                
                if (produtoTemporario.Categoria.Length == 0)
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new{msg = "Categoria: Mínimo um caractere"});
                }
                else
                {
                    produto.Categoria = produtoTemporario.Categoria;
                }

                if (produtoTemporario.Fornecedor.Id <=0)
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new{msg = "Fornecedor: Id inválido"});
                }
                else
                {
                    try
                    {
                        var fornecedor = _database.Fornecedores.First(p => p.Id == produtoTemporario.Fornecedor.Id);
                        produto.Fornecedor = fornecedor;
                    }
                    catch (System.Exception)
                    {
                        Response.StatusCode = 404;
                        return new ObjectResult(new{msg = "Fornecedor não encontrado"});
                    }
                }

                if (produtoTemporario.Quantidade <= 0)
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new{msg = "Quantidade inválida"});
                }
                else
                {
                    produto.Quantidade = produtoTemporario.Quantidade;
                } 
                var fornecedorProd = _database.Fornecedores.First(p => p.Id == produtoTemporario.Fornecedor.Id);
                produto.Promocao = produtoTemporario.Promocao;     
                produto.CodigoProduto = produto.Nome + "cod";
                produto.Status = true;
                _database.Produtos.Add(produto);
                _database.SaveChanges();
                Response.StatusCode = 201;
                return new ObjectResult(new{msg = "Produto criado"});
            }
            catch (System.Exception)
            {
                Response.StatusCode = 400;
                return new ObjectResult("");
            } 
        }

        [HttpGet]
        public IActionResult Get()
        {
            var produtos = _database.Produtos.Where(p => p.Status == true).Include(p => p.Fornecedor).ToList();
            if (produtos.Count == 0)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Nenhum produto encontrado"});
            }
            else
            {
                List<ProdutoContainer> produtoView= new List<ProdutoContainer>();
                for (int i = 0; i < produtos.Count; i++)
                {
                    var produtoHateoas = produtos[i].ConverteDTO(produtos[i]);
                    produtoHateoas.Links = Hateoas.GetActions(produtoHateoas.Produto.Id.ToString());
                    produtoView.Add(produtoHateoas);
                }
                return Ok(produtoView);
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var produto = _database.Produtos.Where(p => p.Status == true).Include(f => f.Fornecedor).First(p => p.Id == id);
                var produtoHateoas = produto.ConverteDTO(produto);
                produtoHateoas.Links = Hateoas.GetActions(produtoHateoas.Produto.Id.ToString());
                return Ok(produtoHateoas);
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Produto não encontrado"});
            }
        }

         [HttpGet("nome/{nome}")]
        public IActionResult Get(string nome)
        {
            try
            {
                var produto = _database.Produtos.Where(p => p.Status == true).Include(f => f.Fornecedor).First(p => p.Nome == nome);
                var produtoHateoas = produto.ConverteDTO(produto);
                produtoHateoas.Links = Hateoas.GetActions(produtoHateoas.Produto.Id.ToString());
                return Ok(produtoHateoas);
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Produto não encontrado"});
            }
        }

        [HttpGet("desc")]
        public IActionResult GetDesc()
        {
            try
            {
                var produtos = _database.Produtos.Where(p => p.Status == true).Include(f => f.Fornecedor).ToList();
                if (produtos.Count == 0)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new {msg = "Nenhum produto encontrado"});
                }
                else
                {
                    List<ProdutoContainer> produtoView= new List<ProdutoContainer>();
                    for (int i = 0; i < produtos.Count; i++)
                    {
                        var produtoHateoas = produtos[i].ConverteDTO(produtos[i]);
                        produtoHateoas.Links = Hateoas.GetActions(produtoHateoas.Produto.Id.ToString());
                        produtoView.Add(produtoHateoas);
                    }
                    var listaDescrecente = produtoView.OrderByDescending(f => f.Produto.Nome);
                    return Ok(listaDescrecente);
                }
            }
            catch (System.Exception)
            {
                Response.StatusCode = 400;
                return new ObjectResult("");
            }
        }

        [HttpGet("asc")]
        public IActionResult GetAsc()
        {
            try
            {
                var produtos = _database.Produtos.Where(c => c.Status == true).Include(f => f.Fornecedor).ToList();
                if (produtos.Count == 0)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new {msg = "Nenhum produto encontrado"});
                }
                else
                {
                    List<ProdutoContainer> produtoView= new List<ProdutoContainer>();
                    for (int i = 0; i < produtos.Count; i++)
                    {
                        var produtoHateoas = produtos[i].ConverteDTO(produtos[i]);
                        produtoHateoas.Links = Hateoas.GetActions(produtoHateoas.Produto.Id.ToString());
                        produtoView.Add(produtoHateoas);
                    }
                    var listaCrescente = produtoView.OrderBy(c => c.Produto.Nome);
                    return Ok(listaCrescente);
                }
            }
        
            catch (System.Exception)
            {
                Response.StatusCode = 400;
                return new ObjectResult("");
            }
        }

        [HttpPatch]
        public IActionResult Patch([FromBody] ProdutoDTO produtoTemporario)
        {
            if (produtoTemporario.Id > 0)
            {
                try
                {
                    var produto = _database.Produtos.Where(p => p.Status == true).First(p => p.Id == produtoTemporario.Id);
                    if (produtoTemporario.Nome == null)
                    {
                        produto.Nome = produto.Nome;
                    }
                    else
                    {
                        if (produtoTemporario.Nome.Length <= 0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Mínimo um caractere"});
                        }
                        else
                        {
                            produto.Nome = produtoTemporario.Nome;
                        }
                    }

                    if (produtoTemporario.Valor == 0)
                    {
                        produto.Valor = produto.Valor;
                    }
                    else
                    {
                         if (produtoTemporario.Valor <= 0.0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Valor inválido"});
                        }
                        else
                        {
                            produto.Valor = produtoTemporario.Valor;
                        }
                    }

                    if (produtoTemporario.ValorPromocao == 0)
                    {
                        produto.ValorPromocao = produto.ValorPromocao;
                    }

                    else
                    {
                        if (produtoTemporario.ValorPromocao < 0.0 || produtoTemporario.ValorPromocao > produto.Valor)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Valor da promoção inválido"});
                        }
                        else
                        {
                            produto.ValorPromocao = produtoTemporario.ValorPromocao;
                        }
                    }

                    if (produtoTemporario.Promocao == false)
                    {
                        produto.Promocao = produto.Promocao;
                    }
                    else
                    {
                        produto.Promocao = produtoTemporario.Promocao;
                    }

                    if (produtoTemporario.Categoria == null)
                    {
                        produto.Categoria = produto.Categoria;
                    }
                    else
                    {
                        if (produtoTemporario.Categoria.Length <= 0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Mínimo um caractere"});
                        }
                        else
                        {
                            produto.Categoria = produtoTemporario.Categoria;
                        }
                    }
                    if (produtoTemporario.Imagem == null)
                    {
                        produto.Imagem = produto.Imagem;
                    }
                    else
                    {
                        if (produtoTemporario.Imagem.Length < 5)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Imagem: Mínimo cinco caracteres"});
                        }
                        else
                        {
                            if (produtoTemporario.Imagem.Contains("."))
                            {
                                produto.Imagem = produtoTemporario.Imagem;
                            }
                            else
                            {
                                Response.StatusCode = 400;
                                return new ObjectResult(new{msg = "Insira a extensão da imagem. Exemplo: .png"});
                            }  
                        }
                    }
                    if (produtoTemporario.Fornecedor == null)
                    {
                        produto.Fornecedor = produto.Fornecedor;
                    }
                    else
                    {
                        try
                        {
                            var forne = _database.Fornecedores.First(f => f.Id == produtoTemporario.Fornecedor.Id);
                            produto.Fornecedor = forne;
                        }
                        catch (System.Exception)
                        {
                            Response.StatusCode = 404;
                            return new ObjectResult(new {msg = "Fornecedor não encontrado"});
                        }
                    }

                    if (produtoTemporario.Quantidade == 0)
                    {
                        produto.Quantidade = produto.Quantidade;
                    }
                    else
                    {
                        if (produtoTemporario.Quantidade > 0)
                        {
                            produto.Quantidade = produtoTemporario.Quantidade;
                        }
                        else
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new {msg = "Quantidade inválida"});
                        }
                    }
                    _database.SaveChanges();
                    return Ok(new{msg = "Produto atualizado"});
                }
                catch (System.Exception)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new {msg = "Produto não encontrado"});
                }
            }
            else
            {
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "Id inválido"});
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProdutoDTO produtoTemporario)
        {
            if (produtoTemporario.Id > 0)
            {
                try
                {
                    var produto = _database.Produtos.Where(p => p.Status == true).First(p => p.Id == produtoTemporario.Id);
                    if (produtoTemporario.Nome == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg = "Informe o nome"});
                    }
                    else
                    {
                        if (produtoTemporario.Nome.Length <= 0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Mínimo um caractere"});
                        }
                        else
                        {
                            produto.Nome = produtoTemporario.Nome;
                        }
                    }

                    if (produtoTemporario.Promocao == false)
                    {
                        produto.Promocao = produto.Promocao;
                    }
                    else
                    {
                        produto.Promocao = produtoTemporario.Promocao;
                    }

                    if (produtoTemporario.ValorPromocao == 0)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg = "Informe o valor da promoção"});
                    }
                    else
                    {
                        if (produtoTemporario.ValorPromocao < 0.0 || produtoTemporario.ValorPromocao > produtoTemporario.Valor)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Valor da promoção inválido"});
                        }
                        else
                        {
                            produto.ValorPromocao = produtoTemporario.ValorPromocao;
                        }
                    }

                    if (produtoTemporario.Valor == 0)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg = "Informe o valor"});
                    }
                    else
                    {
                         if (produtoTemporario.Valor <= 0.0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Valor inválido"});
                        }
                        else
                        {
                            produto.Valor = produtoTemporario.Valor;
                        }
                    }
                    if (produtoTemporario.Categoria == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg = "Informe a categoria"});
                    }
                    else
                    {
                        if (produtoTemporario.Categoria.Length <= 0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Mínimo um caractere"});
                        }
                        else
                        {
                            produto.Categoria = produtoTemporario.Categoria;
                        }
                    }
                    if (produtoTemporario.Imagem == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg = "Informe a imagem"});
                    }
                    else
                    {
                        if (produtoTemporario.Imagem.Length < 5)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Imagem: Mínimo cinco caracteres"});
                        }
                        else
                        {
                            if (produtoTemporario.Imagem.Contains("."))
                            {
                                produto.Imagem = produtoTemporario.Imagem;
                            }
                            else
                            {
                                Response.StatusCode = 400;
                                return new ObjectResult(new{msg = "Insira a extensão da imagem. Exemplo: .png"});
                            }  
                        }
                    }

                    if (produtoTemporario.Quantidade == 0)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg = "Informe a quantidade"});
                    }
                    else
                    {
                        if (produtoTemporario.Quantidade > 0)
                        {
                            produto.Quantidade = produtoTemporario.Quantidade;
                        }
                        else
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new {msg = "Quantidade inválida"});
                        }
                    }

                    if (produtoTemporario.Fornecedor == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg = "Informe o fornecedor"});
                    }
                    else
                    {
                        try
                        {
                            var forne = _database.Fornecedores.First(f => f.Id == produtoTemporario.Fornecedor.Id);
                            produto.Fornecedor = forne;
                        }
                        catch (System.Exception)
                        {
                            Response.StatusCode = 404;
                            return new ObjectResult(new {msg = "Fornecedor não encontrado"});
                        }
                    }
                    _database.SaveChanges();
                    return Ok(new{msg = "Produto atualizado"});
                }
                catch (System.Exception)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new {msg = "Produto não encontrado"});
                }
            }
            else
            {
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "Id inválido"});
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var produto = _database.Produtos.First(c => c.Id == id);
                produto.Status = false;
                _database.SaveChanges();
                return Ok(new{msg = "Produto deletado"});
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Produto não encontrado"});
            }
        }
    }
}