using Microsoft.AspNetCore.Mvc;
using Desafio_Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Desafio_Api.DTO;
using Desafio_Api.Models;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Desafio_Api.HATEOAS;
using Desafio_Api.Container;

namespace Desafio_Api.Controllers
{   
    [Authorize(Roles = "Admin")]
    [Route("Api/v1/[controller]")]
    [ApiController]
   
    public class FornecedoresController : ControllerBase
    {
        private readonly ApplicationDbContext _database;
        private Hateoas Hateoas;
        public FornecedoresController(ApplicationDbContext database)
        {
            _database = database;
            Hateoas = new Hateoas("localhost:5001/api/v1/Fornecedores");
            Hateoas.AddAction("BUSCAR_FORNECEDOR", "GET");
            Hateoas.AddAction("DELETAR_FORNECEDOR", "DELETE");
            Hateoas.AddAction("EDITAR_FORNECEDOR", "PUT");
            Hateoas.AddAction("EDITAR_FORNECEDOR", "PATCH");
        }

        [HttpPost]
        public IActionResult Post([FromBody]FornecedorDTO fornecedorTemporario)
        {
            try
            {
                Fornecedor fornecedor = new Fornecedor();
                if (fornecedorTemporario.Cnpj.Length < 14 || fornecedorTemporario.Cnpj.Length > 18)
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new {msg = "Cnpj inválido"});
                }
                else
                {
                    var forne = fornecedorTemporario.Cnpj.Replace("/", "").Replace(".","").Replace("-","");
                    fornecedor.Cnpj = forne;
                }
                if (fornecedorTemporario.Nome.Length <= 0)
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new {msg = "Mínimo um caractere"});
                }
                else
                {
                    fornecedor.Nome = fornecedorTemporario.Nome;
                }
            
                fornecedor.Status = true;
                _database.Fornecedores.Add(fornecedor);
                _database.SaveChanges();
                Response.StatusCode = 201;
                return new ObjectResult(new{msg = "Fornecedor criado"});
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
            var fornecedores = _database.Fornecedores.Where(c => c.Status == true).ToList();
            if (fornecedores.Count == 0)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Nenhum fornecedor encontrado"});
            }
            else
            {
                List<FornecedorContainer> fornecedorView= new List<FornecedorContainer>();
                for (int i = 0; i < fornecedores.Count; i++)
                {
                    var fornecedorHateoas = fornecedores[i].ConverteDTO(fornecedores[i]);
                    fornecedorHateoas.Links = Hateoas.GetActions(fornecedorHateoas.Fornecedor.Id.ToString());
                    fornecedorView.Add(fornecedorHateoas);
                }
                return Ok(fornecedorView);
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var fornecedor = _database.Fornecedores.Where(c => c.Status == true).First(c => c.Id == id);
                var fornecedorHateoas = fornecedor.ConverteDTO(fornecedor);
                fornecedorHateoas.Links = Hateoas.GetActions(fornecedorHateoas.Fornecedor.Id.ToString());
                return Ok(fornecedorHateoas);
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Fornecedor não encontrado"});
            }
        }

        [HttpGet("nome/{nome}")]
        public IActionResult Get(string nome)
        {
            try
            {
                var fornecedor = _database.Fornecedores.Where(c => c.Status == true).First(c => c.Nome == nome);
                var fornecedorHateoas = fornecedor.ConverteDTO(fornecedor);
                fornecedorHateoas.Links = Hateoas.GetActions(fornecedorHateoas.Fornecedor.Id.ToString());
                return Ok(fornecedorHateoas);
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Fornecedor não encontrado"});
            }
        }

        [HttpGet("desc")]
        public IActionResult GetDesc()
        {
            try
            {
                var fornecedores = _database.Fornecedores.Where(f => f.Status == true).ToList();
                if (fornecedores.Count == 0)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new {msg = "Nenhum fornecedor encontrado"});
                }
                else
                {
                    List<FornecedorContainer> fornecedorView= new List<FornecedorContainer>();
                    for (int i = 0; i < fornecedores.Count; i++)
                    {
                        var fornecedorHateoas = fornecedores[i].ConverteDTO(fornecedores[i]);
                        fornecedorHateoas.Links = Hateoas.GetActions(fornecedorHateoas.Fornecedor.Id.ToString());
                        fornecedorView.Add(fornecedorHateoas);
                    }
                    var listaDescrecente = fornecedorView.OrderByDescending(f => f.Fornecedor.Nome);
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
                var fornecedores = _database.Fornecedores.Where(c => c.Status == true).ToList();
                if (fornecedores.Count == 0)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new {msg = "Nenhum fornecedor encontrado"});
                }
                else
                {
                    List<FornecedorContainer> fornecedorView= new List<FornecedorContainer>();
                    for (int i = 0; i < fornecedores.Count; i++)
                    {
                        var fornecedorHateoas = fornecedores[i].ConverteDTO(fornecedores[i]);
                        fornecedorHateoas.Links = Hateoas.GetActions(fornecedorHateoas.Fornecedor.Id.ToString());
                        fornecedorView.Add(fornecedorHateoas);
                    }
                    var listaCrescente = fornecedorView.OrderBy(f => f.Fornecedor.Nome);
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
        public IActionResult Patch([FromBody] FornecedorDTO fornecedorTemporario)
        {
            if (fornecedorTemporario.Id > 0)
            {
                try
                {
                    var fornecedor = _database.Fornecedores.Where(c => c.Status == true).First(c => c.Id == fornecedorTemporario.Id);

                    if (fornecedorTemporario.Nome == null)
                    {
                        fornecedor.Nome = fornecedor.Nome;
                    }
                    else
                    {
                         if (fornecedorTemporario.Nome.Length <= 0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new {msg = "Nome: Mínimo um caractere"});
                        }
                        else
                        {
                            fornecedor.Nome = fornecedorTemporario.Nome;
                        }
                    }

                    if (fornecedorTemporario.Cnpj == null)
                    {
                        fornecedor.Cnpj = fornecedor.Cnpj;
                    }
                    else
                    {
                        if (fornecedorTemporario.Cnpj.Length < 14 || fornecedorTemporario.Cnpj.Length > 18)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new {msg = "Cnpj inválido"});
                        }
                        else
                        {
                            var forne = fornecedorTemporario.Cnpj.Replace("/", "").Replace(".","").Replace("-","");
                            fornecedor.Cnpj = forne;
                        }
                    }
                     _database.SaveChanges();
                    return Ok(new{msg = "Fornecedor atualizado"});
                }
                catch (System.Exception)
                { 
                    Response.StatusCode = 404;
                    return new ObjectResult(new{msg="Fornecedor não encontrado"});
                }
            }
            else
            {
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "Id inválido"});
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody] FornecedorDTO fornecedorTemporario)
        {
            if (fornecedorTemporario.Id > 0)
            {
                try
                {
                    var fornecedor = _database.Fornecedores.Where(c => c.Status == true).First(c => c.Id == fornecedorTemporario.Id);
                    if (fornecedorTemporario.Cnpj == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new {msg = "Informe o cnpj"});
                    }
                    else
                    {
                        if (fornecedorTemporario.Cnpj.Length < 14 || fornecedorTemporario.Cnpj.Length > 18)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new {msg = "Cnpj inválido"});
                        }
                        else
                        {
                            var forne = fornecedorTemporario.Cnpj.Replace("/", "").Replace(".","").Replace("-","");
                            fornecedor.Cnpj = forne;
                        }
                    }
            
                    if (fornecedorTemporario.Nome == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new {msg = "Informe o nome"});
                    }
                    else
                    {
                         if (fornecedorTemporario.Nome.Length <= 0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new {msg = "Nome: Mínimo um caractere"});
                        }
                        else
                        {
                            fornecedor.Nome = fornecedorTemporario.Nome;
                        }
                    }
                    _database.SaveChanges();
                    return Ok(new{msg = "Fornecedor atualizado"});
                }
                catch (System.Exception)
                { 
                    Response.StatusCode = 404;
                    return new ObjectResult(new{msg="Fornecedor não encontrado"});
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
                var fornecedor = _database.Fornecedores.First(c => c.Id == id);
                fornecedor.Status = false;
                _database.SaveChanges();
                return Ok(new{msg = "Fornecedor deletado"});
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Fornecedor não encontrado"});
            }
        }
    }
}