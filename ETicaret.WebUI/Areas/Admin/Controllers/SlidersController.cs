﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ETicaret.Core.Entities;
using ETicaret.Data.Context;
using ETicaret.WebUI.Utils;
using Microsoft.AspNetCore.Authorization;

namespace ETicaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class SlidersController : Controller
    {
        private readonly DatabaseContext _context;

        public SlidersController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Sliders
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sliders.ToListAsync());
        }

        // GET: Admin/Sliders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // GET: Admin/Sliders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Sliders/Create
  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Slider slider,IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                slider.Image=await FileHelper.FileLoaderAsync(Image,"/Img/Slider/");
                await  _context.AddAsync(slider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(slider);
        }

        // GET: Admin/Sliders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }
            return View(slider);
        }

        // POST: Admin/Sliders/Edit/5
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,Slider slider, IFormFile? Image, bool cbResmiSil = false)
        {
            if (id != slider.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    if (cbResmiSil)
                        slider.Image = string.Empty; //Eğer resmi sil butonu işaretlenmiş ise Resmi boş bir string olarak tanımlar ve resmi siler
                    if (Image is not null)//Eğer resim null değilse yani yeni resim yüklenmişse aşağıdaki işlem gerçekleşir.
                        slider.Image = await FileHelper.FileLoaderAsync(Image, "/Img/Slider/");//Değişiklikleri Slider klasöründe yapar.

                    _context.Update(slider);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SliderExists(slider.Id))
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
            return View(slider);
        }

        // GET: Admin/Sliders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // POST: Admin/Sliders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider != null)
            {
                if (!string.IsNullOrEmpty(slider.Image))
                {
                    FileHelper.FileRemover(slider.Image, "/Img/News/"); //Eğer null veya boş değilse news dosyasında ki resmi sil.
                }
                _context.Sliders.Remove(slider);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SliderExists(int id)
        {
            return _context.Sliders.Any(e => e.Id == id);
        }
    }
}
