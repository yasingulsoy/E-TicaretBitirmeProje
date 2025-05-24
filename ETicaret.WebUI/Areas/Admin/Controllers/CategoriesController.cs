﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ETicaret.Core.Entities;
using ETicaret.Data.Context;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing;
using ETicaret.WebUI.Utils;
using Microsoft.AspNetCore.Authorization; //SelectList için yükledi

namespace ETicaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class CategoriesController : Controller
    {
        private readonly DatabaseContext _context;

        public CategoriesController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Categories/Create
        public async Task<IActionResult> Create()
        {
            //Viewbag ile kategori ıd ve namesini aldık.
            ViewBag.Kategoriler = new SelectList(_context.Categories,"Id","Name");
            return View();
        }

        // POST: Admin/Categories/Create
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Category category, IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                category.Image = await FileHelper.FileLoaderAsync(Image, "/Img/Categories/");//Değişiklikleri Categori klasöründe yapacak.
                await _context.AddAsync(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Kategoriler = new SelectList(_context.Categories, "Id", "Name");//Ekleme işleminde problem olursa return viewden önce yazarak kategorilerin dropdowna yüklenmesini sağladık.
            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewBag.Kategoriler = new SelectList(_context.Categories, "Id", "Name");
            return View(category);
        }

        // POST: Admin/Categories/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category, IFormFile? Image,bool cbResmiSil= false)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (cbResmiSil)
                        category.Image = string.Empty;  //Eğer resmi sil butonu işaretlenmiş ise Resmi boş bir string olarak tanımlar ve resmi siler
                    if (Image is not null) //Eğer resim null değilse yani yeni resim yüklenmişse aşağıdaki işlem gerçekleşir.
                        category.Image = await FileHelper.FileLoaderAsync(Image,"/Img/Categories/");//Değişiklikleri Categori klasöründe yapar.
                     _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Kategoriler = new SelectList(_context.Categories, "Id", "Name");
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                if (!string.IsNullOrEmpty(category.Image))
                {
                    FileHelper.FileRemover(category.Image, "/Img/Categories/");
                }
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
