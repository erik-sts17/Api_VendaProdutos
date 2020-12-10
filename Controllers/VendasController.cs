using Microsoft.AspNetCore.Mvc;
using Desafio_Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Desafio_Api.DTO;
using Desafio_Api.Models;
using System;
using System.Collections.Generic;
using Desafio_Api.HATEOAS;
using Desafio_Api.Container;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Desafio_Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Api/v1/[controller]")]
    [ApiController]
    public class VendasController : ControllerBase
    {
        private readonly ApplicationDbContext _database;

        private Hateoas Hateoas;

        public VendasController(ApplicationDbContext database)
        {
            _database = database;
            Hateoas = new Hateoas("localhost:5001/api/v1/Vendas");
            Hateoas.AddAction("BUSCAR_VENDA", "GET");
            Hateoas.AddAction("DELETAR_VENDA", "DELETE");
            Hateoas.AddAction("EDITAR_VENDA", "PUT");
            Hateoas.AddAction("EDITAR_VENDA", "PATCH");
        }

        [HttpPost]
        public IActionResult Post([FromBody]VendaDTO vendaTemporaria)
        {
            Venda venda = new Venda();
            if (vendaTemporaria.Cliente.Id <= 0)
            {
                Response.StatusCode = 400;
                return new ObjectResult(new{msg="Cliente: Id inválido"});
            }
            else
            {
                try
                {
                    venda.Cliente = _database.Clientes.First(c => c.Id == vendaTemporaria.Cliente.Id);
                }
                catch (System.Exception)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new{msg="Cliente não encontrado"});
                }
            }
            if (vendaTemporaria.Fornecedor.Id <= 0)
            {
                Response.StatusCode = 400;
                return new ObjectResult(new{msg="Fornecedor: Id inválido"});
            }
            else
            {
                try
                {
                    venda.Fornecedor = _database.Fornecedores.First(c => c.Id == vendaTemporaria.Fornecedor.Id); 
                }
                catch (System.Exception)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new{msg="Fornecedor não encontrado"});
                } 
            }  
            try
            {
                if (DateTime.Parse(vendaTemporaria.DataCompra) > DateTime.Now)
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new{msg="Data inválida"});
                }
                
                var data = vendaTemporaria.DataCompra;
                venda.DataCompra = DateTime.Parse(data);
            }
            catch (System.Exception)
            {        
                Response.StatusCode = 400;
                return new ObjectResult(new{msg="Data inválida"});
            }
            if (vendaTemporaria.Produtos.Count == 0)
            {
                Response.StatusCode = 400;
                return new ObjectResult(new{msg="Mínimo um produto"});
            }
            else
            {
                for (int i = 0; i < vendaTemporaria.Produtos.Count; i++)
                {
                    try
                    {
                        ProdutoVenda produtosVendidos = new ProdutoVenda();
                        if (vendaTemporaria.Produtos[i].Id <= 0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg="Produto: Id inválido"});
                        }
                        var produtoBanco = _database.Produtos.First(p => p.Id == vendaTemporaria.Produtos[i].Id);
                        if (produtoBanco.Quantidade <= 0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg="Produto sem estoque"});
                        }
                        produtosVendidos.Produto = produtoBanco;
                        produtosVendidos.Venda = venda;
                        venda.Produtos.Add(produtosVendidos);
                    }
                    catch (System.Exception)
                    {
                        Response.StatusCode = 404;
                        return new ObjectResult(new {msg = "Produto não encontrado"});
                    } 
                }     
            }
            double total = 0.0;
            for (int i = 0; i < vendaTemporaria.Produtos.Count; i++)
            {
                if (venda.Produtos[i].Produto.Promocao == true)
                {
                    total += venda.Produtos[i].Produto.Valor - venda.Produtos[i].Produto.ValorPromocao;
                    venda.Produtos[i].Produto.Quantidade --;
                }
                else
                {
                    total += venda.Produtos[i].Produto.Valor;
                    venda.Produtos[i].Produto.Quantidade --;
                }
            }
            venda.Status = true;  
            venda.TotalCompra = total;
            _database.AddRange(venda);
            _database.SaveChanges();
            Response.StatusCode = 201;
            return new ObjectResult(new{msg = "Venda criada"});   
        }

        [HttpGet]
        public IActionResult Get()
        {
            var vendas = _database.Vendas.Where(v => v.Status == true).Include(f => f.Fornecedor).Include(c => c.Cliente).Include(p => p.Produtos).ThenInclude(p => p.Produto).ToList();
            if (vendas.Count == 0)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Nenhuma venda encontrada"});
            }
            else
            {
                List<VendasContainer> vendaView= new List<VendasContainer>();
                for (int i = 0; i < vendas.Count; i++)
                {
                    var vendaHateoas = vendas[i].ConverteDTO(vendas[i]);
                    vendaHateoas.Links = Hateoas.GetActions(vendaHateoas.Venda.Id.ToString());
                    vendaView.Add(vendaHateoas);
                }  
                return Ok(vendaView);
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var venda = _database.Vendas.Where(v => v.Status == true).Include(f => f.Fornecedor).Include(c => c.Cliente).Include(p => p.Produtos).ThenInclude(p => p.Produto).First(v => v.Id == id);
                var vendaHateoas = venda.ConverteDTO(venda);
                vendaHateoas.Links = Hateoas.GetActions(vendaHateoas.Venda.Id.ToString());
                return Ok(vendaHateoas);
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Venda não encontrada"});
            }
        }

        [HttpGet("desc")]
        public IActionResult GetDesc(){
            try
            {
                var vendas = _database.Vendas.Where(v => v.Status == true).Include(f => f.Fornecedor).Include(c => c.Cliente).Include(p => p.Produtos).ThenInclude(p => p.Produto).ToList();
                if (vendas.Count == 0)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new {msg = "Nenhuma venda encontrada"});
                }
                else
                {
                    List<VendasContainer> vendaView= new List<VendasContainer>();
                    for (int i = 0; i < vendas.Count; i++)
                    {
                        var vendaHateoas = vendas[i].ConverteDTO(vendas[i]);
                        vendaHateoas.Links = Hateoas.GetActions(vendaHateoas.Venda.Id.ToString());
                        vendaView.Add(vendaHateoas);
                    }  
                    var listaDescrescente = vendaView.OrderByDescending(v => v.Venda.DataCompra);
                    return Ok(listaDescrescente);
                }
            }
            catch (System.Exception)
            {
                Response.StatusCode = 400;
                return new ObjectResult("");
            }
        }

        [HttpGet("asc")]
        public IActionResult GetAsc(){
            try
            {
                var vendas = _database.Vendas.Where(v => v.Status == true).Include(f => f.Fornecedor).Include(c => c.Cliente).Include(p => p.Produtos).ThenInclude(p => p.Produto).ToList();
                if (vendas.Count == 0)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new {msg = "Nenhuma venda encontrada"});
                }
                else
                {
                    List<VendasContainer> vendaView= new List<VendasContainer>();
                    for (int i = 0; i < vendas.Count; i++)
                    {
                        var vendaHateoas = vendas[i].ConverteDTO(vendas[i]);
                        vendaHateoas.Links = Hateoas.GetActions(vendaHateoas.Venda.Id.ToString());
                        vendaView.Add(vendaHateoas);
                    }  
                    var listaCrescente = vendaView.OrderBy(v => v.Venda.DataCompra);
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
        public IActionResult Patch([FromBody] VendaDTO vendaTemporaria)
        {
            if (vendaTemporaria.Id >= 0)
            {
                try
                {
                    var venda = _database.Vendas.Include(f => f.Fornecedor).Include(c => c.Cliente).Include(p => p.Produtos).ThenInclude(p => p.Produto).First(v => v.Id == vendaTemporaria.Id);
                    if (vendaTemporaria.Fornecedor == null)
                    {
                       venda.Fornecedor = venda.Fornecedor;
                    }
                    else
                    {
                        if (vendaTemporaria.Fornecedor.Id <=0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg="Fornecedor: Id inválido"});
                        }
                        else
                        {
                            try
                            {
                                var fornecedor = _database.Fornecedores.First(f => f.Id == vendaTemporaria.Fornecedor.Id);
                                venda.Fornecedor = fornecedor;
                            }
                            catch (System.Exception)
                            {
                                Response.StatusCode = 404;
                                return new ObjectResult(new{msg="Fornecedor não encontrado"});
                            }
                        }
                    }
                    if (vendaTemporaria.Cliente == null)
                    {
                        venda.Cliente = venda.Cliente;
                    }
                    else
                    {
                        if (vendaTemporaria.Cliente.Id <=0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg="Cliente: Id inválido"});
                        }
                        else
                        {
                            try
                            {
                                var cliente = _database.Clientes.First(c => c.Id == vendaTemporaria.Cliente.Id);
                                venda.Cliente = cliente;
                            }
                            catch (System.Exception)
                            {
                                Response.StatusCode = 404;
                                return new ObjectResult(new{msg="Cliente não encontrado"});
                            }
                        }
                    }

                    if (vendaTemporaria.DataCompra == null)
                    {
                        venda.DataCompra = venda.DataCompra;
                    }
                    else
                    {
                        try
                        {
                            if (DateTime.Parse(vendaTemporaria.DataCompra) > DateTime.Now)
                            {
                                Response.StatusCode = 400;
                                return new ObjectResult(new{msg="Data inválida"});
                            }
                            var data = vendaTemporaria.DataCompra;
                            venda.DataCompra = DateTime.Parse(data);
                        }
                        catch (System.Exception)
                        {        
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg="Data inválida"});
                        }
                    }

                    if (vendaTemporaria.Produtos.Count == 0)
                    {
                        venda.Produtos = venda.Produtos;
                    }
                    else
                    {
                        for (int i = 0; i < venda.Produtos.Count; i++)
                        {
                            venda.Produtos.Clear();
                        }
                        for (int i = 0; i < vendaTemporaria.Produtos.Count; i++)
                        {   
                            try
                            {
                                ProdutoVenda produtosVendidos = new ProdutoVenda();
                                if (vendaTemporaria.Produtos[i].Id <= 0)
                                {
                                    Response.StatusCode = 400;
                                    return new ObjectResult(new{msg="Produto: Id inválido"});
                                }
                                var produtoBanco = _database.Produtos.First(p => p.Id == vendaTemporaria.Produtos[i].Id);
                                if (produtoBanco.Quantidade <= 0)
                                {
                                    Response.StatusCode = 400;
                                    return new ObjectResult(new{msg="Produto sem estoque"});
                                }
                                else
                                {
                                    produtosVendidos.Produto = produtoBanco;
                                    produtosVendidos.Venda = venda;
                                    venda.Produtos.Add(produtosVendidos);
                                }   
                            }
                            catch (System.Exception)
                            {
                                Response.StatusCode = 404;
                                return new ObjectResult(new{msg="Produto não encontrado"});
                            }     
                        }
                        double total = 0.0;
                        for (int i = 0; i < venda.Produtos.Count; i++)
                        {
                            if (venda.Produtos[i].Produto.Promocao == true)
                            {
                                total += venda.Produtos[i].Produto.Valor - venda.Produtos[i].Produto.ValorPromocao;
                            }
                            else
                            {
                                total += venda.Produtos[i].Produto.Valor;
                            }
                        }
                        venda.TotalCompra = total;
                    }
                    _database.SaveChanges();
                    return Ok(new{msg = "Venda atualizada"});
                }
               catch (System.Exception)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new{msg="Venda não encontrada"});
                }
            }
            else
            {
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "Id inválido"});
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody] VendaDTO vendaTemporaria)
        {
            if (vendaTemporaria.Id >= 0)
            {
                try
                {
                    var venda = _database.Vendas.Include(f => f.Fornecedor).Include(c => c.Cliente).Include(p => p.Produtos).ThenInclude(p => p.Produto).First(v => v.Id == vendaTemporaria.Id);
                    if (vendaTemporaria.Fornecedor == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new {msg = "Informe o fornecedor"});
                    }
                    else
                    {
                        if (vendaTemporaria.Fornecedor.Id <=0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg="Fornecedor: Id inválido"});
                        }
                        else
                        {
                            try
                            {
                                var fornecedor = _database.Fornecedores.First(f => f.Id == vendaTemporaria.Fornecedor.Id);
                                venda.Fornecedor = fornecedor;
                            }
                            catch (System.Exception)
                            {
                                Response.StatusCode = 404;
                                return new ObjectResult(new{msg="Fornecedor não encontrado"});
                            }
                        }
                    }
                    if (vendaTemporaria.Cliente == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new {msg = "Informe o cliente"});
                    }
                    else
                    {
                        if (vendaTemporaria.Cliente.Id <=0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg="Cliente: Id inválido"});
                        }
                        else
                        {
                            try
                            {
                                var cliente = _database.Clientes.First(c => c.Id == vendaTemporaria.Cliente.Id);
                                venda.Cliente = cliente;
                            }
                            catch (System.Exception)
                            {
                                Response.StatusCode = 404;
                                return new ObjectResult(new{msg="Cliente não encontrado"});
                            }
                        }
                    }

                    if (vendaTemporaria.DataCompra == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg="Informe a data"});
                    }
                    else
                    {
                        try
                        {
                            if (DateTime.Parse(vendaTemporaria.DataCompra) > DateTime.Now)
                            {
                                Response.StatusCode = 400;
                                return new ObjectResult(new{msg="Data inválida"});
                            }
                            var data = vendaTemporaria.DataCompra;
                            venda.DataCompra = DateTime.Parse(data);
                        }
                        catch (System.Exception)
                        {        
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg="Data inválida"});
                        }
                    }

                    if (vendaTemporaria.Produtos.Count == 0)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new {msg = "Informe os produtos"});
                    }
                    else
                    {
                        for (int i = 0; i < venda.Produtos.Count; i++)
                        {
                            venda.Produtos.Clear();
                        }
                        for (int i = 0; i < vendaTemporaria.Produtos.Count; i++)
                        {   
                            try
                            {
                                ProdutoVenda produtosVendidos = new ProdutoVenda();
                                if (vendaTemporaria.Produtos[i].Id <= 0)
                                {
                                    Response.StatusCode = 400;
                                    return new ObjectResult(new{msg="Produto: Id inválido"});
                                }
                                var produtoBanco = _database.Produtos.First(p => p.Id == vendaTemporaria.Produtos[i].Id);
                                if (produtoBanco.Quantidade <= 0)
                                {
                                    Response.StatusCode = 400;
                                    return new ObjectResult(new{msg="Produto sem estoque"});
                                }
                                else
                                {
                                    produtosVendidos.Produto = produtoBanco;
                                    produtosVendidos.Venda = venda;
                                    venda.Produtos.Add(produtosVendidos);
                                }   
                            }
                            catch (System.Exception)
                            {
                                Response.StatusCode = 404;
                                return new ObjectResult(new{msg="Produto não encontrado"});
                            }     
                        }
                        double total = 0.0;
                        for (int i = 0; i < venda.Produtos.Count; i++)
                        {
                            venda.Produtos[i].Produto.Quantidade --;
                            if (venda.Produtos[i].Produto.Promocao == true)
                            {
                                total += venda.Produtos[i].Produto.Valor - venda.Produtos[i].Produto.ValorPromocao;
                            }
                            else
                            {
                                total += venda.Produtos[i].Produto.Valor;
                            }
                           
                        }
                        venda.TotalCompra = total;
                    }
                    _database.SaveChanges();
                    return Ok(new{msg = "Venda atualizada"});
                }
               catch (System.Exception)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new{msg="Venda não encontrada"});
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
                var venda = _database.Vendas.First(c => c.Id == id);
                venda.Status = false;
                _database.SaveChanges();
                return Ok(new{msg = "Venda deletada"});
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Venda não encontrada"});
            }
        }
    }
}