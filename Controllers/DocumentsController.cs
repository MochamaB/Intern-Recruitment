using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using Workflows.Attributes;
using Workflows.Data;
using Workflows.Models;
using Workflows.Services;
using Workflows.ViewModels;

namespace Workflows.Controllers
{
    [CustomAuthorize]
    public class DocumentsController : Controller
    {
        private readonly WorkflowsContext _context;
        private readonly IRelationshipService _relationshipService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public DocumentsController(WorkflowsContext context,IRelationshipService relationshipService, IWebHostEnvironment webHostEnvironment, 
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _relationshipService = relationshipService;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Documents
        public async Task<IActionResult> Index()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userRole = httpContext.Session.GetString("EmployeeRole");
            var userPayroll = httpContext.Session.GetString("EmployeePayrollNo");
            var sessionDepartmentID = httpContext.Session.GetString("EmployeeDepartmentID");

            using var db = new KtdaleaveContext();
            var departmentCode = await db.Departments
                .Where(d => d.DepartmentId == sessionDepartmentID)
               .Select(e => e.DepartmentCode)
               .FirstOrDefaultAsync();

            var documents = await _context.Document
               .Where(r => userRole == "Admin" || userRole == "HR" || r.DepartmentCode == departmentCode)
                .OrderByDescending(e => e.Id).Take(50).ToListAsync();

            var documentTypes = _context.DocumentType.ToDictionary(dt => dt.Id);
            var interns = _context.Intern.ToDictionary(i => i.Id);
          
          
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

            var document = await _relationshipService.GetDocumentWithRelatedDataAsync((int)id);
            if (document == null)
            {
                return NotFound();
            }

            var viewModel = new DocumentEditViewModel
            {
                Id = document.Id,
                Requisition_id = document.Requisition_id,
                Intern_id = document.Intern_id,
                DocumentTypeId = document.DocumentTypeId,
                DepartmentCode = document.DepartmentCode,
                FileName = document.FileName,
                FileType = document.FileType,   
                MimeType = document.MimeType,
                FileSize = document.FileSize,
                CreatedAt = document.CreatedAt,
                UpdatedAt = DateTime.Now,
                DepartmentName = document.Department?.DepartmentName,
                Firstname = document.Intern?.Firstname,
                Lastname = document.Intern?.Lastname,
                DocType = document.DocumentType?.DocumentName
            };

            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["ReturnUrl"] = returnUrl;
            }

            return View(viewModel);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DocumentEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var document = await _relationshipService.GetDocumentWithRelatedDataAsync(model.Id);
                if (document == null)
                {
                    return NotFound();
                }

                if (model.NewFile != null && model.NewFile.Length > 0)
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", document.FileName);

                    // Delete old file
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                    // Generate the new filename
                    string newFileName = model.FileName;

                    // Save new file
                    var newFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", newFileName);
                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await model.NewFile.CopyToAsync(stream);
                    }

                    // Update document properties
                    document.FileName = newFileName;
                    document.FileType = model.NewFile.ContentType;
                    document.MimeType = model.NewFile.ContentType;
                    document.FileSize = model.NewFile.Length;
                }

                document.Requisition_id = model.Requisition_id;
                document.Intern_id = model.Intern_id;
                document.DocumentTypeId = model.DocumentTypeId;
                document.DepartmentCode = model.DepartmentCode;
                document.CreatedAt = model.CreatedAt;
                document.UpdatedAt = DateTime.Now;

                _context.Update(document);
                await _context.SaveChangesAsync();


                // Returns to the where the edit function was called
                if (TempData.TryGetValue("ReturnUrl", out object returnUrl))
                {
                    TempData["SuccessMessage"] = "New Document uploaded successfully!";
                    return Redirect(returnUrl.ToString());
                }
                TempData["SuccessMessage"] = "New Document uploaded successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ErrorMessage = "Please check the page for validation errors.";
            return View(model);
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
