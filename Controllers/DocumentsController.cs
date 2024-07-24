﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using Workflows.Data;
using Workflows.Models;
using Workflows.Services;
using Workflows.ViewModels;

namespace Workflows.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly WorkflowsContext _context;
        private readonly IRelationshipService _relationshipService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DocumentsController(WorkflowsContext context,IRelationshipService relationshipService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _relationshipService = relationshipService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Documents
        public async Task<IActionResult> Index()
        {
            var documents = await _context.Document.ToListAsync();
            var documentTypes = _context.DocumentType.ToDictionary(dt => dt.Id);
            var interns = _context.Intern.ToDictionary(i => i.Id);
          
            using var db = new KtdaleaveContext();
            var departments = db.Departments.ToDictionary(d => d.DepartmentCode);

            var viewModel = documents.Select(document => new DocumentViewModel
            {
                Document = document,
                DocumentType = documentTypes.GetValueOrDefault(document.DocumentTypeId),
                Intern = interns.GetValueOrDefault(document.Intern_id),
                DepartmentName = departments.TryGetValue(document.DepartmentCode, out var department)
                ? department.DepartmentName
                : null
            }).ToList();

            return View(viewModel);
        }

       

        // GET: Documents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Document
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // GET: Documents/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Documents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Requisition_id,Intern_id,DocumentType,DepartmentCode,FileName,FileType,FileSize,CreatedAt,UpdatedAt")] Document document)
        {
            if (ModelState.IsValid)
            {
                _context.Add(document);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(document);
        }

        // GET: Documents/Edit/5
        public async Task<IActionResult> Edit(int? id, string returnUrl)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Document.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["ReturnUrl"] = returnUrl;
            }

            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Requisition_id,Intern_id,DocumentType,DepartmentCode,FileName,FileType,FileSize,CreatedAt,UpdatedAt")] Document document)
        {
            if (id != document.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Returns to the where the edit function was called
                if (TempData.TryGetValue("ReturnUrl", out object returnUrl))
                {
                    return Redirect(returnUrl.ToString());
                }
                return RedirectToAction(nameof(Index));
            }
            return View(document);
        }

        // GET: Documents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Document
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Document.FindAsync(id);
            if (document != null)
            {
                _context.Document.Remove(document);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocumentExists(int id)
        {
            return _context.Document.Any(e => e.Id == id);
        }

        // View PDF Files in the browser ///////
        [HttpGet]
        public async Task<IActionResult> GetDocument(int id)
        {
            var document = await _context.Document.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", document.FileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
           
            var contentType = GetContentType(filePath, document.FileType, document.MimeType);

            // Use Console.WriteLine to output the document details
            Console.WriteLine($"ContentType: {contentType}");

            // For PDFs and images, try to display inline
            if (contentType.StartsWith("image/") || contentType == "application/pdf")
            {
                Console.WriteLine($"View: {contentType}");
                Response.Headers.Append("Content-Disposition", new ContentDispositionHeaderValue("inline")
                {
                    FileName = document.FileName
                }.ToString());

                return File(fileBytes, contentType);
            }
            else
            {
                Console.WriteLine($"Download: {contentType}");
                // For other file types, force download
                return PhysicalFile(filePath, contentType, document.FileName);
            }
        }

        private string GetContentType(string path, string fileType, string mimeType) 
        {
            // Use the provided mimeType if available
            if (!string.IsNullOrEmpty(mimeType))
            {
                return mimeType;
            }
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.GetValueOrDefault(ext, "application/octet-stream");
        }
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                { ".txt", "text/plain" },
                { ".pdf", "application/pdf" },
                { ".doc", "application/vnd.ms-word" },
                { ".docx", "application/vnd.ms-word" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".csv", "text/csv" }
            };
        }
    }


}
