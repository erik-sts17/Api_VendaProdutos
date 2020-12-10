using Microsoft.AspNetCore.Mvc;
using Desafio_Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Desafio_Api.DTO;
using Desafio_Api.Models;
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Desafio_Api.HATEOAS;
using Desafio_Api.Container;
using System.Text.Json;

namespace Desafio_Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Api/v1/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _database;

        private Hateoas Hateoas;

        public ClientesController(ApplicationDbContext database)
        {
            _database = database;
            Hateoas = new Hateoas("localhost:5001/api/v1/Clientes");
            Hateoas.AddAction("BUSCAR_CLIENTE", "GET");
            Hateoas.AddAction("DELETAR_CLIENTE", "DELETE");
            Hateoas.AddAction("EDITAR_CLIENTE", "PUT");
            Hateoas.AddAction("EDITAR_CLIENTE", "PATCH");
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody]LoginDTO usuario)
        {
            try
            {
                var user = _database.Clientes.First(credenciais => credenciais.Email == usuario.Email);
                if (user != null)
                {
                    if (user.Senha == usuario.CriarHash(usuario.Senha))
                    {
                        string chaveDeSeguranca = "chave_seguranca_desafio_apiErik";
                        var chaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveDeSeguranca));
                        var credenciaisAcesso = new SigningCredentials(chaveSimetrica, SecurityAlgorithms.HmacSha256Signature);

                        var claims = new List<Claim>();
                        claims.Add(new Claim("email", usuario.Email));
                        claims.Add(new Claim(ClaimTypes.Role, "Admin"));

                        var JWT = new JwtSecurityToken(
                            issuer: "Api_Vendas",
                            expires: DateTime.Now.AddHours(1),
                            audience: "usuario_comum",
                            signingCredentials : credenciaisAcesso,
                            claims : claims       
                    );
                        return Ok(new JwtSecurityTokenHandler().WriteToken(JWT));
                    }
                    else
                    {
                        Response.StatusCode = 401;
                        return new ObjectResult(new{msg = "Não autorizado"});
                    }
                }
                else
                {
                    Response.StatusCode = 401;
                    return new ObjectResult(new{msg = "Não autorizado"});
                }
            }
            catch (System.Exception)
            {
                Response.StatusCode = 401;
                return new ObjectResult(new{msg = "Não autorizado"});
            }
           
        }

        [HttpPost]
        public IActionResult Post([FromBody]ClienteDTO clienteTemporario)
        {
            try
            {
                Cliente cliente = new Cliente();

                if (clienteTemporario.Nome.Length <= 0)
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new{msg = "Nome: Mínimo um caractere"});
                }
                else
                {
                    cliente.Nome = clienteTemporario.Nome;
                }

                if (clienteTemporario.Email.Contains("@") && clienteTemporario.Email.Length > 6)
                {
                    cliente.Email = clienteTemporario.Email;
                }
                else
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new{msg = "Email inválido"});
                }
                if (clienteTemporario.Senha.Length < 3)
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new{msg = "Senha: Mínimo três caracteres"});
                }
                else
                {
                    cliente.Senha = clienteTemporario.CriarHash(clienteTemporario.Senha);
                }
                if (clienteTemporario.Documento.Length < 9)
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new{msg = "Documento inválido"});
                }
                else
                {
                    var cli = clienteTemporario.Documento.Replace("/", "").Replace(".","").Replace("-","");
                    cliente.Documento = cli;
                }          
                cliente.Status = true;
                cliente.DataCadastro = DateTime.Now;
                _database.Clientes.Add(cliente);
                _database.SaveChanges();
                Response.StatusCode = 201;
                return new ObjectResult(new{msg = "Cliente criado"});
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
            var clientes = _database.Clientes.Where(c => c.Status == true).ToList();
            if (clientes.Count == 0)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Nenhum cliente encontrado"});
            }
            else
            {
                List<ClienteContainer> clienteView = new List<ClienteContainer>();
                for (int i = 0; i < clientes.Count; i++)
                {
                    var clienteHateoas = clientes[i].ConverteDTO(clientes[i]);
                    clienteHateoas.Links = Hateoas.GetActions(clienteHateoas.Cliente.Id.ToString());
                    clienteView.Add(clienteHateoas);
                }
                return Ok(clienteView);
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var cliente = _database.Clientes.Where(c => c.Status == true).First(c => c.Id == id);
                var clienteHateoas = cliente.ConverteDTO(cliente);
                clienteHateoas.Links = Hateoas.GetActions(clienteHateoas.Cliente.Id.ToString());
                return Ok(clienteHateoas);
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Cliente não encontrado"});
            }
        }

        [HttpGet("nome/{nome}")]
        public IActionResult Get(string nome)
        {
            try
            {
                var cliente = _database.Clientes.Where(c => c.Status == true).First(c => c.Nome == nome);
                var clienteHateoas = cliente.ConverteDTO(cliente);
                clienteHateoas.Links = Hateoas.GetActions(clienteHateoas.Cliente.Id.ToString());
                return Ok(clienteHateoas);
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Cliente não encontrado"});
            }
        }

    
        [HttpGet("desc")]
        public IActionResult GetDesc(){
            try
            {
                var clientes = _database.Clientes.Where(c => c.Status == true).ToList();
                if (clientes.Count == 0)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new {msg = "Nenhum cliente encontrado"});
                }
                else
                {
                    List<ClienteContainer> clienteView = new List<ClienteContainer>();
                    for (int i = 0; i < clientes.Count; i++)
                    {
                        var clienteHateoas = clientes[i].ConverteDTO(clientes[i]);
                        clienteHateoas.Links = Hateoas.GetActions(clienteHateoas.Cliente.Id.ToString());
                        clienteView.Add(clienteHateoas);
                    }
                    var listaDescrescente = clienteView.OrderByDescending(c => c.Cliente.Nome);
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
                var clientes = _database.Clientes.Where(c => c.Status == true).ToList();
                if (clientes.Count == 0)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new {msg = "Nenhum cliente encontrado"});
                }
                else
                {
                    List<ClienteContainer> clienteView = new List<ClienteContainer>();
                    for (int i = 0; i < clientes.Count; i++)
                    {
                        var clienteHateoas = clientes[i].ConverteDTO(clientes[i]);
                        clienteHateoas.Links = Hateoas.GetActions(clienteHateoas.Cliente.Id.ToString());
                        clienteView.Add(clienteHateoas);
                    }
                    var listaCrescente = clienteView.OrderBy(c => c.Cliente.Nome);
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
        public IActionResult Patch([FromBody] ClienteDTO clienteTemporario)
        {
            if (clienteTemporario.Id > 0)
            {
                try
                {
                    var cliente = _database.Clientes.Where(c => c.Status == true).First(c => c.Id == clienteTemporario.Id);

                    if (clienteTemporario.Nome == null)
                    {
                        cliente.Nome = cliente.Nome;
                    }
                    else
                    {
                        if (clienteTemporario.Nome.Length <= 0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Nome: Mínimo um caractere"});
                        }
                        else
                        {
                            cliente.Nome = clienteTemporario.Nome;
                        }
                    }
                    
                    if (clienteTemporario.Email == null)
                    {
                        cliente.Email = cliente.Email;
                    }
                    else
                    {
                        if (clienteTemporario.Email.Contains("@") && clienteTemporario.Email.Length > 6)
                        {
                            cliente.Email = clienteTemporario.Email;
                        }
                        else
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Email inválido"});
                        }
                    }

                    if (clienteTemporario.Documento == null)
                    {
                        cliente.Documento = cliente.Documento;    
                    }
                    else
                    {
                         if (clienteTemporario.Senha.Length < 3)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Senha: Mínimo três caracteres"});
                        }
                        else
                        {
                            cliente.Senha = clienteTemporario.Senha;
                        }
                    }

                    if (clienteTemporario.Senha == null)
                    {
                        cliente.Senha = cliente.Senha;
                    }
                    else
                    {
                        if (clienteTemporario.Documento.Length < 9)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Documento inválido"});
                        }
                        else
                        {
                            var cli = clienteTemporario.Documento.Replace("/", "").Replace(".","").Replace("-","");
                            cliente.Documento = cli;
                        }     
                    }
                    _database.SaveChanges();
                    return Ok(new{msg = "Cliente atualizado"});
                }
                catch (System.Exception)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new{msg="Cliente não encontrado"});
                }
            }
            else
            {
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "Id inválido"});
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody] ClienteDTO clienteTemporario)
        {
            if (clienteTemporario.Id > 0)
            {
                try
                {
                    var cliente = _database.Clientes.Where(c => c.Status == true).First(c => c.Id == clienteTemporario.Id);

                    if (clienteTemporario.Nome == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg = "Informe o nome"});
                    }
                    else
                    {
                        if (clienteTemporario.Nome.Length <= 0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Nome: Mínimo um caractere"});
                        }
                        else
                        {
                            cliente.Nome = clienteTemporario.Nome;
                        }
                    }
                    
                    if (clienteTemporario.Email == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg = "Informe o email"});
                    }
                    else
                    {
                        if (clienteTemporario.Email.Contains("@") && clienteTemporario.Email.Length > 6)
                        {
                            cliente.Email = clienteTemporario.Email;
                        }
                        else
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Email inválido"});
                        }
                    }

                    if (clienteTemporario.Senha == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg = "Informe a senha"});    
                    }
                    else
                    {
                         if (clienteTemporario.Senha.Length < 3)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Senha: Mínimo três caracteres"});
                        }
                        else
                        {
                            cliente.Senha = clienteTemporario.Senha;
                        }
                    }

                    if (clienteTemporario.Documento == null)
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new{msg = "Informe o documento"});
                    }
                    else
                    {
                        if (clienteTemporario.Documento.Length < 9)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new{msg = "Documento inválido"});
                        }
                        else
                        {
                            var cli = clienteTemporario.Documento.Replace("/", "").Replace(".","").Replace("-","");
                            cliente.Documento = cli;
                        }     
                    }
                    _database.SaveChanges();
                    return Ok(new{msg = "Cliente atualizado"});
                }
                catch (System.Exception)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new{msg="Cliente não encontrado"});
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
                var cliente = _database.Clientes.First(c => c.Id == id);
                cliente.Status = false;
                _database.SaveChanges();
                return Ok(new{msg = "Cliente deletado"});
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new {msg = "Cliente não encontrado"});
            }
        }
    }
}