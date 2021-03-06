using GIMNASIO.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GIMNASIO.Controllers
{
    public class ProductoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnviroment;

        public ProductoController(AppDbContext context, IWebHostEnvironment hostEnviroment)
        {
            _context = context;
            _hostEnviroment = hostEnviroment;
        }

        //Listar los Productos
        public IActionResult ListarProductos(int page, EventArgs e)
        {
            IndexViewModel indexViewModel = new IndexViewModel();

            if(page == 0)
            {
                indexViewModel.Pagina = 1;
            }
            else
            {
                indexViewModel.Pagina = page;
            }

            int muestra = 6;
            int cantidad = _context.tblProductos.ToList().Count / muestra;
            if(cantidad % muestra == 0)
            {
                indexViewModel.CantidadPagina = cantidad;
            }
            else
            {
                indexViewModel.CantidadPagina = cantidad + 1;
            }

            indexViewModel.ListaProductos = _context.tblProductos.Include(e => e.Categoria)
                .Include(e => e.Estado)
                .Skip((indexViewModel.Pagina - 1) * muestra)
                .Take(6).ToList();
            TempData["NextPage"] = indexViewModel.Pagina + 1;
            TempData["PreviusPage"] = indexViewModel.Pagina - 1;
            return View(indexViewModel);

        }

        //llamar a la view de crear
        [HttpGet]
        public IActionResult CrearProducto()
        {
            ViewData["EstadoId"] = new SelectList(_context.tblEstados.ToList(), "EstadoId", "EstadoNombre");
            ViewData["CategoriaId"] = new SelectList(_context.tblCategorias.ToList(), "CategoriaId", "Nombre");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearProducto(Producto P)
        {
            if (ModelState.IsValid)
            {
                if (P.ImagenFile == null)
                {
                    P.imagen = "NoImagen.png";
                }
                else
                {
                    //para subir imagen
                    string wwwRootPath = _hostEnviroment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(P.ImagenFile.FileName);
                    string extension = Path.GetExtension(P.ImagenFile.FileName);
                    P.imagen = fileName + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                    string path = Path.Combine(wwwRootPath + "/imagen/" + P.imagen);
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await P.ImagenFile.CopyToAsync(fileStream);
                    }
                }

                _context.Add(P);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Producto Agregado Exitosamente!";
                return RedirectToAction(nameof(ListarProductos));
            }
            return View(P);
        }

        //Eliminar
        [HttpGet]
        public IActionResult EliminarProducto(int ProductoId)
        {
            ViewData["EstadoId"] = new SelectList(_context.tblEstados.ToList(), "EstadoId", "EstadoNombre");
            ViewData["CategoriaId"] = new SelectList(_context.tblCategorias.ToList(), "CategoriaId", "Nombre");
            var P = _context.tblProductos.Where(p => p.ProductoId.Equals(ProductoId))
                .Include(e => e.Categoria)
                .Include(e => e.Estado)
                .FirstOrDefault();
            return View(P);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarProducto(Producto P)
        {
            var pEliminado = _context.tblProductos.Where(p => p.ProductoId == P.ProductoId).FirstOrDefault();
            _context.Entry(pEliminado).State = EntityState.Deleted;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Producto Eliminado Exitosamente!";
            return RedirectToAction(nameof(ListarProductos));
        }



    }
}
