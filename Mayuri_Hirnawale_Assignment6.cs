Configure DbContext:

public class AppDbContext : DbContext
{
    public DbSet<EmployeeBasicDetails> EmployeeBasicDetails { get; set; }
    public DbSet<EmployeeAdditionalDetails> EmployeeAdditionalDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("your_connection_string_here");
    }
}

CRUD Operations:

public class EmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
    }

    // Create
    public async Task<EmployeeBasicDetails> CreateEmployeeBasicDetails(EmployeeBasicDetails employee)
    {
        _context.EmployeeBasicDetails.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    // Read
    public async Task<EmployeeBasicDetails> GetEmployeeBasicDetailsById(string id)
    {
        return await _context.EmployeeBasicDetails.FindAsync(id);
    }

    // Update
    public async Task<EmployeeBasicDetails> UpdateEmployeeBasicDetails(EmployeeBasicDetails employee)
    {
        _context.EmployeeBasicDetails.Update(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    // Delete
    public async Task<bool> DeleteEmployeeBasicDetails(string id)
    {
        var employee = await _context.EmployeeBasicDetails.FindAsync(id);
        if (employee == null)
        {
            return false;
        }

        _context.EmployeeBasicDetails.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }
}


Import/Export Excel:

public class ExcelService
{
    private readonly AppDbContext _context;

    public ExcelService(AppDbContext context)
    {
        _context = context;
    }

    public async Task ImportEmployeesFromExcel(string filePath)
    {
        using (var workbook = new XLWorkbook(filePath))
        {
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // Assuming the first row contains column names

            foreach (var row in rows)
            {
                var employee = new EmployeeBasicDetails
                {
                    FirstName = row.Cell(1).Value.ToString(),
                    LastName = row.Cell(2).Value.ToString(),
                    Email = row.Cell(3).Value.ToString(),
                    Mobile = row.Cell(4).Value.ToString(),
                    ReportingManagerName = row.Cell(5).Value.ToString(),
                    Address = new Address { /* set properties based on your data */ }
                };

                var additionalDetails = new EmployeeAdditionalDetails
                {
                    PersonalDetails = new PersonalDetails_
                    {
                        DateOfBirth = DateTime.Parse(row.Cell(6).Value.ToString())
                    },
                    WorkInformation = new WorkInfo_
                    {
                        DateOfJoining = DateTime.Parse(row.Cell(7).Value.ToString())
                    }
                };

                _context.EmployeeBasicDetails.Add(employee);
                _context.EmployeeAdditionalDetails.Add(additionalDetails);
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task ExportEmployeesToExcel(string filePath)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Employees");

            // Add headers
            worksheet.Cell(1, 1).Value = "Sr.No";
            worksheet.Cell(1, 2).Value = "First Name";
            worksheet.Cell(1, 3).Value = "Last Name";
            worksheet.Cell(1, 4).Value = "Email";
            worksheet.Cell(1, 5).Value = "Phone No";
            worksheet.Cell(1, 6).Value = "Reporting Manager Name";
            worksheet.Cell(1, 7).Value = "Date Of Birth";
            worksheet.Cell(1, 8).Value = "Date of Joining";

            var employees = await _context.EmployeeBasicDetails.Include(e => e.Address).ToListAsync();
            var additionalDetails = await _context.EmployeeAdditionalDetails.Include(e => e.PersonalDetails).Include(e => e.WorkInformation).ToListAsync();

            for (int i = 0; i < employees.Count; i++)
            {
                var employee = employees[i];
                var additional = additionalDetails.FirstOrDefault(a => a.EmployeeBasicDetailsUId == employee.Id);

                worksheet.Cell(i + 2, 1).Value = i + 1;
                worksheet.Cell(i + 2, 2).Value = employee.FirstName;
                worksheet.Cell(i + 2, 3).Value = employee.LastName;
                worksheet.Cell(i + 2, 4).Value = employee.Email;
                worksheet.Cell(i + 2, 5).Value = employee.Mobile;
                worksheet.Cell(i + 2, 6).Value = employee.ReportingManagerName;
                worksheet.Cell(i + 2, 7).Value = additional?.PersonalDetails?.DateOfBirth.ToShortDateString();
                worksheet.Cell(i + 2, 8).Value = additional?.WorkInformation?.DateOfJoining.ToShortDateString();
            }

            workbook.SaveAs(filePath);
        }
    }
}
Controller setup (for ASP.NET Core):

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;
    private readonly ExcelService _excelService;

    public EmployeesController(EmployeeService employeeService, ExcelService excelService)
    {
        _employeeService = employeeService;
        _excelService = excelService;
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Upload a file");
        }

        var filePath = Path.GetTempFileName();

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        await _excelService.ImportEmployeesFromExcel(filePath);

        return Ok();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var filePath = Path.GetTempFileName();

        await _excelService.ExportEmployeesToExcel(filePath);

        var memory = new MemoryStream();
        using (var stream = new FileStream(filePath, FileMode.Open))
        {
            await stream.CopyToAsync(memory);
        }

        memory.Position = 0;

        return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "employees.xlsx");
    }
}
