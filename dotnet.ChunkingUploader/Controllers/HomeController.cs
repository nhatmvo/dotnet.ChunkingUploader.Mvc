﻿using dotnet.LargeFileUploader.Models;
using dotnet.LargeFileUploader.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace dotnet.LargeFileUploader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFileStorageService _fileStorageService;

        public HomeController(ILogger<HomeController> logger,
            IFileStorageService fileStorageService)
        {
            _logger = logger;
            _fileStorageService = fileStorageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult UploadFile(IFormCollection formCollection)
        {
            var fileName = formCollection["fileName"].ToString();
            var sequence = Convert.ToInt32(formCollection["sequence"]);
            var total = Convert.ToInt32(formCollection["total"]);
            _fileStorageService.FormFileHandling(fileName, sequence, total, formCollection.Files.First());
            return View();
        }
    }
}