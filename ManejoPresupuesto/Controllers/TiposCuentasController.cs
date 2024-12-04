﻿using Dapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly string connectionString;
        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios)
        {
            RepositorioTiposCuentas = repositorioTiposCuentas;
            ServicioUsuarios = servicioUsuarios;
        }

        public IRepositorioTiposCuentas RepositorioTiposCuentas { get; }
        public IServicioUsuarios ServicioUsuarios { get; }

        public async Task<IActionResult> Index()
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await RepositorioTiposCuentas.Obtener(usuarioId);
            return View(tiposCuentas); ;
        }

        public IActionResult Crear() {


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }
            tipoCuenta.UsuarioId = ServicioUsuarios.ObtenerUsuarioId();

            var yaExisteTipoCuenta = await RepositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);


            if (yaExisteTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe");

                return View(tipoCuenta);
            }

            await RepositorioTiposCuentas.Crear(tipoCuenta);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await RepositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var tipoCuentaExiste = await RepositorioTiposCuentas.ObtenerPorId(tipoCuenta.Id, usuarioId);
            if (tipoCuentaExiste is null) {

                return RedirectToAction("NoEncontrado", "Home");

            }
            await RepositorioTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await RepositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await RepositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await RepositorioTiposCuentas.Borrar(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var yaExisteTipoCuenta = await RepositorioTiposCuentas.Existe(nombre, usuarioId);
            if (yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }

            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {

            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await RepositorioTiposCuentas.Obtener(usuarioId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if(idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenados = ids.Select((valor, indice) => 
            new TipoCuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();

            await RepositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);

            return Ok();

        }
    }
}
