using Domains.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Services.DataServices;
using System.Data.SqlTypes;
using Web.Filters;

namespace Web.Controllers {
    public class ShortUrlController : Controller {
        private readonly ShortUrlDataService _dataService;

        public ShortUrlController(ShortUrlDataService dataService) {
            _dataService = dataService;
        }

        public async Task<IActionResult> Index() {
            var shortUrls = await _dataService.GetAsync();
            return View(shortUrls);
        }

        public async Task<IActionResult> RedirectToUrl(string hash) {
            try {
                var shortUrl = await _dataService.GetByHashAsync(hash);
                await _dataService.IncrementTransitionsCount(shortUrl);
                string url = Uri.EscapeUriString(shortUrl.Url);
                return Redirect(url);
            }
            catch (SqlNullValueException ex) {
                return RedirectToAction(nameof(Error), new { ex.Message });
            }
        }

        public IActionResult Error(string message) {
            return View(model: message);
        }

        public async Task<IActionResult> Delete(int id) {
            await _dataService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Add() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UrlIsValidFilter]
        public async Task<IActionResult> Add(AddShortUrlViewModel model) {
            if (ModelState.IsValid) {
                try {
                    await _dataService.AddAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (SqlAlreadyFilledException ex) {
                    ModelState.AddModelError("Url", ex.Message);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id) {
            try {
                var shortUrl = await _dataService.GetAsync(id);
                var updateModel = new UpdateShortUrlViewModel() { Id = shortUrl.Id, Url = shortUrl.Url };
                return View(updateModel);
            }
            catch (SqlNullValueException ex) {
                return RedirectToAction(nameof(Error), new { ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UrlIsValidFilter]
        public async Task<IActionResult> Edit(UpdateShortUrlViewModel model) {
            if (ModelState.IsValid) {
                try {
                    await _dataService.UpdateAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (SqlAlreadyFilledException ex) {
                    ModelState.AddModelError("Url", ex.Message);
                }
            }
            return View(model);
        }
    }
}
