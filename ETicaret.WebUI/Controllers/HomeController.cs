using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ETicaret.WebUI.Models;
using ETicaret.Data.Context;
using Microsoft.EntityFrameworkCore;
using ETicaret.Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ETicaret.Service.Abstract;
using System.Threading.Tasks;

namespace ETicaret.WebUI.Controllers;

public class HomeController : Controller
{
    private readonly IService<Product> _serviceProduct;
    private readonly IService<Slider> _serviceSlider;
    private readonly IService<News> _serviceNews;
    private readonly IService<Contact> _serviceContact;

    public HomeController(IService<Product> serviceProduct, IService<Slider> serviceSlider, IService<News> serviceNews, IService<Contact> serviceContact)
    {
        _serviceProduct = serviceProduct;
        _serviceSlider = serviceSlider;
        _serviceNews = serviceNews;
        _serviceContact = serviceContact;
    }

    public async Task<IActionResult> Index()
    {
        //veri taban�ndan al�nan verileri  olu�turdu�umuz model de�i�keninin i�erisine al�n�p model de�i�keni view ile d�nd�r�r.
        var model = new HomePageViewModel()
        {
            Sliders=await _serviceSlider.GetAllAsync(),
            News=await _serviceNews.GetAllAsync(news=>news.IsActive),
            Products= await _serviceProduct.GetAllAsync(p=>p.IsActive && p.IsHome)
        };
        return View(model);
    }
    public IActionResult Privacy()
    {
        return View();
    }
    [Route("AccessDenied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    public IActionResult ContactUs()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> ContactUsAsync(Contact contact)
    {
        if (ModelState.IsValid) {
            try
            {
                await _serviceContact.AddAsync(contact);
                var sonuc = await _serviceContact.SaveChangesAsync();
                if (sonuc > 0)
                {
                    TempData["Message"] = @"<div class=""alert alert-success alert-dismissible fade show"" role=""alert"">
                    <strong>Mesaj�n�z Ba�ar�yla G�nderildi.</strong>
                    <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
                    </div>";
                    return RedirectToAction("ContactUs");
                }
            }
            catch (Exception ) 
            {
                ModelState.AddModelError("", "Hata Olu�tu!");
            }
        }
        return View(contact);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
