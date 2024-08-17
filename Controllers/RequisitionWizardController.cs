using Microsoft.AspNetCore.Mvc;
using Workflows.Models;
using Workflows.Extensions;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Workflows.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Workflows.Services;
using Workflows.ViewModels;
using Workflows.Attributes;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;

namespace Workflows.Controllers
{
    [CustomAuthorize] /// Used to ensure authenticated users view this class/pages
    public class RequisitionWizardController : Controller
    {
        private readonly WorkflowsContext _context;
        private const string RequisitionWizardViewPath = "~/Views/Wizards/RequisitionWizard.cshtml";
        private const string RequisitionWizardCheckDepartment = "~/Views/Wizards/CheckDepartment.cshtml";
        private readonly IApprovalService _approvalService;
        //   private readonly IDocumentService _documentService;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RequisitionWizardController(WorkflowsContext context, IApprovalService approvalService, IWebHostEnvironment webHostEnvironment,
          IEmailService emailService //   IDocumentService documentService
            )
        {
            _context = context;
            _approvalService = approvalService;
            //   _documentService = documentService;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
        }

        private List<string> GetSteps()
        {
            return new List<string> { "Intern", "Requisition", "Documents", "Approvals", "Summary" };
        }

        [HttpGet]
        public IActionResult checkDepartment()
        {
            // Clear the session data when the wizard is started
            HttpContext.Session.Remove("WizardDepartment");
            HttpContext.Session.Remove("WizardRequisition");
            HttpContext.Session.Remove("WizardIntern");
            HttpContext.Session.Remove("WizardApprovalSteps");
            HttpContext.Session.Remove("WizardDocumentList");

            using (var db = new KtdaleaveContext())
            {
                var departments = db.Departments.ToList();

                // Create a SelectList from the departments
                var departmentItems = new SelectList(departments, "DepartmentCode", "DepartmentName");


                // Pass the SelectList to ViewBag
                ViewBag.DepartmentItems = departmentItems;

               
                return View(RequisitionWizardCheckDepartment);
            }

           
        }
        // POST: Requisition/CheckDepartment
        [HttpPost]
        public async Task<IActionResult> CheckDepartment(CheckDepartmentViewModel department)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please select a department.";
                return View(RequisitionWizardCheckDepartment);
            }

            // Get the max number of active requisitions for the department
            var maxRequisitionsSetting = await _context.Setting.FirstOrDefaultAsync(s => s.DepartmentCode == department.DepartmentCode && s.Key == "noOfInterns");

            if (maxRequisitionsSetting == null || !int.TryParse(maxRequisitionsSetting.Value, out int maxRequisitions))
            {
                ViewBag.ErrorMessage =  "Unable to determine maximum requisitions for this department.";
                ModelState.AddModelError("", "Unable to determine maximum requisitions for this department.");
                return View(RequisitionWizardCheckDepartment);
            }

            // Count active requisitions for the department
            var activeRequisitionsCount = await _context.Requisition
                .CountAsync(r => r.DepartmentCode == department.DepartmentCode && r.Status == "Active");

            Console.WriteLine($"activeRequisitionsCount: {activeRequisitionsCount}"); // Log the activeRequisitionsCount
            Console.WriteLine($"maxRequisitions: {maxRequisitions}"); // Log the activeRequisitionsCount

            if (activeRequisitionsCount >= maxRequisitions)
            {
                ViewBag.ErrorMessage = "A requisition for this department cannot be added.";
                ModelState.AddModelError("", $"This department has reached the maximum number of active requisitions ({maxRequisitions}). Contact HR Department for assistance");
                return View(RequisitionWizardCheckDepartment);
            }
            HttpContext.Session.SetObject("WizardDepartment", department); // Save to session

            // If we've passed all checks, proceed to the intern function
            return RedirectToAction("Intern");
        }

        //////////////////////////////////////////////// Step 1: Create Intern ///////////////////////////////////
        [HttpGet]
        public IActionResult Intern(Intern? intern = null)
        {
            var departmentcode = HttpContext.Session.GetObject<CheckDepartmentViewModel>("WizardDepartment");
            // Check if the request method is GET to clear validation errors on initial load
            if (HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.Clear();
            }
            //Load the session data if its available or create new
            intern = HttpContext.Session.GetObject<Intern>("WizardIntern") ?? new Intern();

            using (var db = new KtdaleaveContext())
            {
                var department = db.Departments.FirstOrDefault(d => d.DepartmentCode == departmentcode.DepartmentCode);


                // Pass the SelectList to ViewBag
                ViewBag.DepartmentCode = department.DepartmentCode;
                ViewBag.DepartmentName = department.DepartmentName;

                ViewBag.Steps = GetSteps();
                ViewBag.CurrentStep = "Intern";
                ViewBag.CurrentStepIndex = 0;

                return View(RequisitionWizardViewPath,intern);
            }
        }
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIntern([Bind("Id,DepartmentCode,Idnumber,Firstname,Lastname,Othernames,Email,PhoneNumber,Status,Certification,Course,School,CreatedAt,UpdatedAt")] Intern intern)
        {
            if (ModelState.IsValid)
            {
                intern.Status = "Inactive";
                intern.CreatedAt = DateTime.Now;
                intern.UpdatedAt = DateTime.Now;

                HttpContext.Session.SetObject("WizardIntern", intern); // Save to session
                TempData["SuccessMessage"] = "New Intern Added successfully!";
                return RedirectToAction("Requisition");
            }
            ViewBag.ErrorMessage = "Please check the page for validation errors.";
            return Intern(intern);
        }

        ////////////////////////////////////// Step 2: Create Requisition //////////////////////////////////////
        [HttpGet]
        public IActionResult Requisition(Requisition? requisition = null)
        {
            var intern = HttpContext.Session.GetObject<Intern>("WizardIntern");
            if (intern == null)
            {
                // Handle the case where the intern is not in the session
                return RedirectToAction("Intern");
            }
            // Check if the request method is GET to clear validation errors on initial load
            if (HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.Clear();
            }
            //Load the session data if its available or create new
            requisition = HttpContext.Session.GetObject<Requisition>("WizardRequisition") ?? new Requisition();

            /////QUERY THE DEPARTMENT FOR THE REQUISITION
            int internDepartmentCode = intern.DepartmentCode;
            using (var db = new KtdaleaveContext())
            {
                var department = db.Departments.FirstOrDefault(d => d.DepartmentCode == internDepartmentCode);

                // Create a SelectList from the departments
                //  var departmentItems = new SelectList(department, "DepartmentCode", "DepartmentName");
                requisition.Start_Date = DateTime.Now;
                ViewBag.DepartmentCode = department.DepartmentCode;
                ViewBag.DepartmentName = department.DepartmentName;
                ViewBag.Intern_id = intern.Id;
                ViewBag.InternFirstname = intern.Firstname;
                ViewBag.InternLastname = intern.Lastname;
                ViewBag.Steps = GetSteps();
                ViewBag.CurrentStep = "Requisition";
                return View(RequisitionWizardViewPath, requisition);
            }
        }

        [HttpPost]
        public IActionResult CreateRequisition([Bind("Id,DepartmentCode,Intern_id,AddedBy,Station,Status,Start_Date,End_Date,CreatedAt,UpdatedAt")] Requisition requisition)
        {
            if (ModelState.IsValid)
            {
            //    string employeePayrollNo = HttpContext.Session.GetString("EmployeePayrollNo");
                var employee = HttpContext.Session.GetObject<EmployeeBkp>("Employee");
                requisition.PayrollNo = employee.PayrollNo;
                requisition.Status = "Inactive";
                requisition.CreatedAt = DateTime.Now;
                requisition.UpdatedAt = DateTime.Now;

                HttpContext.Session.SetObject("WizardRequisition", requisition);

                ////Create Approval Steps//////
                // Create the approval steps////
                /// Get the PayrollNo for the HOD,HR Officer,HOH
                int DepartmentCode = requisition.DepartmentCode;
                string EmployeePayroll = employee.PayrollNo;
                using (var db = new KtdaleaveContext())
                {
                    var department = db.Departments.FirstOrDefault(d => d.DepartmentCode == DepartmentCode);
                    var emp = db.EmployeeBkps.FirstOrDefault(d => d.PayrollNo == EmployeePayroll);
                    string HOD;
                    if (department != null && !string.IsNullOrEmpty(department.DepartmentHd))
                    {
                        HOD = department.DepartmentHd; /// HOD PAYROLL
                    }
                    else
                    {
                        HOD = emp.Hod ?? emp.Supervisor ??  emp.PayrollNo;
                    }

                    string HROfficer = "HUMAN RESOURCE ASSISTANT";
                    var HRemployee = db.EmployeeBkps.FirstOrDefault(d => d.Designation == HROfficer && d.EmpisCurrActive == 0);
                    var HROfficerPayrollNO = HRemployee.PayrollNo;

                    string HOHOfficer = "Head of HR Operations";
                    var HOHemployee = db.EmployeeBkps.FirstOrDefault(d => d.Designation == HOHOfficer && d.EmpisCurrActive == 0);
                    var HOH = HOHemployee.PayrollNo; //HOH Payroll

                    //Create the Steps using IApproval Service////
                    var approvalSteps = _approvalService.CreateApprovalSteps(requisition, HOD, HROfficerPayrollNO,HOH);

                    // Store the approval steps in the session
                    HttpContext.Session.SetObject("WizardApprovalSteps", approvalSteps);
                    TempData["SuccessMessage"] = "New Requisition added successfully!";
                    return RedirectToAction("Documents");
                }
            }
          //  ViewBag.ErrorMessage = "Please check the page for validation errors.";
            return Requisition(requisition);
        }

        ////////////////////////////////////// Step 3: Create Documents //////////////////////////////////////
        [HttpGet]
        public IActionResult Documents(Document? document = null)
        {
            var requisition = HttpContext.Session.GetObject<Requisition>("WizardRequisition");
           if (requisition == null)
            {
                // Handle the case where the approval steps are not found in the session
                return RedirectToAction("Requisition");
            }
            // Check if the request method is GET to clear validation errors on initial load
            if (HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.Clear();

            }
            document = document ?? new Document();
            var documentList = HttpContext.Session.GetObject<List<Document>>("WizardDocumentList") ?? new List<Document>();
        
            var documentTypes = _context.DocumentType.ToList();
            ViewBag.DocumentTypes = documentTypes;
            ViewBag.Steps = GetSteps();
            ViewBag.CurrentStep = "Documents";
            return View(RequisitionWizardViewPath, document);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDocument(Document document, List<IFormFile> files)
        {
            // Retrieve the requisition object from the session
            var requisition = HttpContext.Session.GetObject<Requisition>("WizardRequisition");
            var intern = HttpContext.Session.GetObject<Intern>("WizardIntern");
            var documentTypes = _context.DocumentType.ToList();
            var documentList = new List<Document>();
           

            if (files.Count != documentTypes.Count)
            {
                for (int i = files.Count; i < documentTypes.Count; i++)
                {
                    ModelState.AddModelError("File", "Please upload a file.");
                }
                ViewBag.ErrorMessage = "Please check the page for validation errors.";
                return Documents(document);
            }


            // Loop through returned and generate data for the Document entity
            for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var documentType = documentTypes[i];
               

                    if (file != null && file.Length > 0)
                    {
                    // Generate the new filename
                    string newFileName = $"{intern.Firstname}_{intern.Lastname}_{documentType.DocumentName}{Path.GetExtension(file.FileName)}";

                    // Save the file temporarily
                    var tempFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", newFileName);
                    // Ensure the temporary directory exists
               //     if (!Directory.Exists(tempFilePath))
                 //   {
                  //      Directory.CreateDirectory(tempFilePath);
                 //   }
                    using (var stream = new FileStream(tempFilePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
    
                            document = new Document
                            {
                                Requisition_id = requisition.Id,
                                Intern_id = requisition.Intern_id,
                                DocumentTypeId = documentType.Id,
                                DepartmentCode = requisition.DepartmentCode,
                                FileName = newFileName,
                                FileType = file.ContentType,
                                MimeType = file.ContentType,
                                FileSize = file.Length,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                    // Use Console.WriteLine to output the document details
                    Console.WriteLine($"Document created: {document.Requisition_id}, {document.Intern_id}, {document.DepartmentCode}");

                    documentList.Add(document);
            
                    }
                }
            
                HttpContext.Session.SetObject("WizardDocumentList", documentList);
                TempData["SuccessMessage"] = "All files uploaded successfully!";
            return RedirectToAction("Approvals");
                
               
        }

        ////////////////////////////////////// Step 4: Get Approvals //////////////////////////////////////
        [HttpGet]
        public IActionResult Approvals(Requisition? requisition = null)
        {
            // Retrieve the approval steps from the session
            var approvalSteps = HttpContext.Session.GetObject<List<Approval>>("WizardApprovalSteps");

            if (requisition == null || approvalSteps == null)
            {
                // Handle the case where the approval steps are not found in the session
                return RedirectToAction("Requisition");
            }

            // Create a list to hold the view models
            // Create a dictionary to hold the employee names
            var employeeName = new Dictionary<string, string>();

            var employeeSelects = new Dictionary<string, SelectList>();
            // HR Department Code. To be changed later to get it through name and not hardcoded.
            int HRDepartmentCode = 104;
            string HRdepartmentCodeString = HRDepartmentCode.ToString();
            using (var db = new KtdaleaveContext())
            {
                foreach (var approvalStep in approvalSteps)
                {
                    var employee = db.EmployeeBkps.FirstOrDefault(d => d.PayrollNo == approvalStep.PayrollNo);
                    // Add to the dictionary
                    if (employee != null && !employeeName.ContainsKey(approvalStep.PayrollNo))
                    {
                        employeeName[approvalStep.PayrollNo] = employee.Fullname;
                    }
                    List<EmployeeBkp> employees;
                    if (approvalStep.ApprovalStep == "HOD Approval")
                    {
                        var department = db.Departments.FirstOrDefault(d => d.DepartmentCode == approvalStep.DepartmentCode);
                        // Fetch employees in the same department as the approval
                        employees = db.EmployeeBkps.Where(e => e.Department == department.DepartmentId &&
                        e.EmpisCurrActive == 0).OrderBy(e => e.Fullname).ToList();
                    }
                    else if (approvalStep.ApprovalStep == "HR Officer Approval" || approvalStep.ApprovalStep == "HOH Approval")
                    {
                        employees = db.EmployeeBkps.Where(e => e.Department == HRdepartmentCodeString &&
                        e.EmpisCurrActive == 0).OrderBy(e => e.Fullname).ToList();
                    }
                    else
                    {
                        employees = new List<EmployeeBkp>();
                    }
                    employeeSelects[approvalStep.ApprovalStep] = new SelectList(employees, "PayrollNo", "Fullname", approvalStep.PayrollNo);

                }
            }
            ViewBag.EmployeeNames = employeeName;
            ViewBag.EmployeeSelects = employeeSelects;
            ViewBag.Steps = GetSteps();
            ViewBag.CurrentStep = "Approvals";
            return View(RequisitionWizardViewPath, approvalSteps);

        }

        [HttpPost]
        public IActionResult CreateApproval(IFormCollection form)
        {
            // Retrieve the approval steps from the session
            var approvalSteps = HttpContext.Session.GetObject<List<Approval>>("WizardApprovalSteps");
            if (approvalSteps != null)
            {
                // Find the approval step to update (HOD Approval or step 1)
                var stepToUpdate = approvalSteps.FirstOrDefault(step => step.StepNumber == 1 || step.ApprovalStep == "HOD Approval");

                if (stepToUpdate != null)
                {
                    string key = $"ApprovalSteps[{stepToUpdate.ApprovalStep}].PayrollNo";
                    if (form.ContainsKey(key))
                    {
                        string newPayrollNo = form[key];
                        if (!string.IsNullOrEmpty(newPayrollNo))
                        {
                            // Update the PayrollNo
                            stepToUpdate.PayrollNo = newPayrollNo;

                            // Save the updated list back to the session
                            HttpContext.Session.SetObject("WizardApprovalSteps", approvalSteps);
                        }
                    }
                }
            }


            //  HttpContext.Session.Set("Approval", approval);
            TempData["SuccessMessage"] = "All approval levels created successfully!";
            return RedirectToAction("Summary");

        }
        ////////////////////////////////////// Step 5: Finish the wizard //////////////////////////////////////
        [HttpGet]
        public IActionResult Summary()
        {
            var intern = HttpContext.Session.GetObject<Intern>("WizardIntern");
            var requisition = HttpContext.Session.GetObject<Requisition>("WizardRequisition");
            var approvalSteps = HttpContext.Session.GetObject<List<Approval>>("WizardApprovalSteps");
            var documentList = HttpContext.Session.GetObject<List<Document>>("WizardDocumentList");

            if (intern == null || requisition == null || approvalSteps == null || documentList == null)
            {
                // Handle missing session data
                return RedirectToAction("Requisition"); // or another appropriate action
            }
            using (var db = new KtdaleaveContext())
            {
                var department = db.Departments.FirstOrDefault(d => d.DepartmentCode == requisition.DepartmentCode);
                var documentViewModels = documentList.Select(document =>
                {
                    var documentType = _context.DocumentType.FirstOrDefault(dt => dt.Id == document.DocumentTypeId);
                    return new DocumentViewModel
                    {
                        Document = document,
                        DocumentType = documentType
                    };
                }).ToList();

                var viewModel = new SummaryViewModel
                {
                    Intern = intern,
                    Requisition = requisition,
                    ApprovalSteps = approvalSteps.Select(approval =>
                    {
                        var employee = db.EmployeeBkps.FirstOrDefault(e => e.PayrollNo == approval.PayrollNo);
                        return new ApprovalViewModel
                        {
                            Approval = approval,
                            EmployeeName = employee != null ? employee.Fullname : string.Empty
                        };
                    }).ToList(),
                    DocumentList = documentViewModels,
                    DepartmentName = department != null ? department.DepartmentName : string.Empty,
                   
                };

                ViewBag.Steps = GetSteps();
                ViewBag.CurrentStep = "Summary";
                return View(RequisitionWizardViewPath, viewModel);
            }
        }


        [HttpPost]
        public async Task<IActionResult> CompleteWizard()
        {

            var intern = HttpContext.Session.GetObject<Intern>("WizardIntern");
            var requisition = HttpContext.Session.GetObject<Requisition>("WizardRequisition");
            var approvalSteps = HttpContext.Session.GetObject<List<Approval>>("WizardApprovalSteps");
            var documentList = HttpContext.Session.GetObject<List<Document>>("WizardDocumentList");
            if (requisition != null && intern != null && approvalSteps != null && documentList != null)
            {
                // Save the intern
                _context.Intern.Add(intern);
                await _context.SaveChangesAsync();

                // Save the requisition
                requisition.Intern_id = intern.Id;
                _context.Requisition.Add(requisition);
                await _context.SaveChangesAsync();

                // Update the Requisition_id for each approval
                approvalSteps.ForEach(a => a.Requisition_id = requisition.Id);

                // Save the approval steps
                _context.Approval.AddRange(approvalSteps);
                await _context.SaveChangesAsync();

                // Update the Requisition_id for each document
                documentList.ForEach(d => d.Requisition_id = requisition.Id);


                // Move the temporary files to a permanent location
                int DepartmentCode = requisition.DepartmentCode;
                using (var db = new KtdaleaveContext())
                {
                    var department = db.Departments.FirstOrDefault(d => d.DepartmentCode == DepartmentCode);
                    if (department != null)
                    {
                        string departmentName = department.DepartmentName;

                        foreach (var document in documentList)
                        {
                            var tempFilePath = Path.Combine("wwwroot/uploads/", document.FileName);
                       //     var departmentDirectory = Path.Combine("wwwroot/uploads/permanent", departmentName);
                        //    var permanentFilePath = Path.Combine(departmentDirectory, document.FileName);

                            // Ensure the department directory exists
                      //      if (!Directory.Exists(departmentDirectory))
                      //      {
                       //         Directory.CreateDirectory(departmentDirectory);
                       //     }

                            // Move the file to the permanent location
                            if (System.IO.File.Exists(tempFilePath))
                            {
                                System.IO.File.Move(tempFilePath, tempFilePath);
                            }

                            // Update the file path in the document object
                            document.FilePath = tempFilePath;
                            document.Intern_id = requisition.Intern_id;
                        }
                    }
                }


                // Save the documents
                _context.Document.AddRange(documentList);
                await _context.SaveChangesAsync();

                // Send Email to the person who raised requisition and the first approver
                using (var db = new KtdaleaveContext())
                {
                    // Get Employee who raised requisition.
                    var requester = db.EmployeeBkps.FirstOrDefault(d => d.PayrollNo == requisition.PayrollNo);
                    var requesterEmail = requester.EmailAddress;
                  
                    try
                    {
                        await _emailService.SendRequistionCreatedNotificationAsync(requesterEmail, requisition.Id);
                    }
                    catch (ApplicationException ex)
                    {
                        TempData["ErrorMessage"] = "Email was not sent.Contact Webmaster ICT and notify the intended person(s) manually";
                    }

                    // Get the first approval's payroll number
                    var firstApprovalPayrollNo = approvalSteps[0].PayrollNo;
                    var FirstApprover = db.EmployeeBkps.FirstOrDefault(d => d.PayrollNo == firstApprovalPayrollNo);
                    await _emailService.SendApprovalPendingNotificationAsync(FirstApprover.EmailAddress, requisition.Id);

                }



                // Clear the session
                HttpContext.Session.Remove("WizardDepartment");
                HttpContext.Session.Remove("WizardRequisition");
                HttpContext.Session.Remove("WizardIntern");
                HttpContext.Session.Remove("WizardApprovalSteps");
                HttpContext.Session.Remove("WizardDocumentList");

                // Redirect to the Requisition Index action
                TempData["SuccessMessage"] = "Intern Requisition has been created successfully.Go to approvals to track progress";
                return RedirectToAction("Index", "Requisitions");
            }

            return RedirectToAction("Summary");


        }


        }

}
