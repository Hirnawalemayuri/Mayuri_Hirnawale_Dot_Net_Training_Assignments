using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace VisitorSecurityClearanceSystem
{
    // Models
    public class Visitor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string CompanyName { get; set; }
        public string Purpose { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }
        public VisitorStatus Status { get; set; }
    }

    public enum VisitorStatus
    {
        Pending,
        Approved,
        Rejected
    }

    // DbContext
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Visitor> Visitors { get; set; }
    }

    // Services
    public interface IVisitorService
    {
        Visitor CreateVisitor(Visitor visitor);
        Visitor GetVisitorById(int id);
        IEnumerable<Visitor> GetVisitorsByStatus(VisitorStatus status);
        void UpdateVisitor(Visitor visitor);
        void DeleteVisitor(int id);
    }

    public class VisitorService : IVisitorService
    {
        private readonly AppDbContext _context;

        public VisitorService(AppDbContext context)
        {
            _context = context;
        }

        public Visitor CreateVisitor(Visitor visitor)
        {
            visitor.Status = VisitorStatus.Pending;
            _context.Visitors.Add(visitor);
            _context.SaveChanges();
            return visitor;
        }

        public Visitor GetVisitorById(int id)
        {
            return _context.Visitors.FirstOrDefault(v => v.Id == id);
        }

        public IEnumerable<Visitor> GetVisitorsByStatus(VisitorStatus status)
        {
            return _context.Visitors.Where(v => v.Status == status).ToList();
        }

        public void UpdateVisitor(Visitor visitor)
        {
            _context.Visitors.Update(visitor);
            _context.SaveChanges();
        }

        public void DeleteVisitor(int id)
        {
            var visitor = _context.Visitors.FirstOrDefault(v => v.Id == id);
            if (visitor != null)
            {
                _context.Visitors.Remove(visitor);
                _context.SaveChanges();
            }
        }
    }

    // Controllers
    public class VisitorController
    {
        private readonly IVisitorService _visitorService;

        public VisitorController(IVisitorService visitorService)
        {
            _visitorService = visitorService;
        }

        public IActionResult CreateVisitor([FromBody] Visitor visitor)
        {
            var newVisitor = _visitorService.CreateVisitor(visitor);
            // Send email with pass PDF
            SendPassEmail(newVisitor);
            return Ok(newVisitor);
        }

        private void SendPassEmail(Visitor visitor)
        {
            // Implementation to send email with pass PDF
            // For simplicity, I'll use a placeholder method here
            // You should integrate with SendGrid or other email service providers
        }

        public IActionResult GetVisitorById(int id)
        {
            var visitor = _visitorService.GetVisitorById(id);
            if (visitor == null)
                return NotFound();

            return Ok(visitor);
        }

        public IActionResult GetVisitorsByStatus(VisitorStatus status)
        {
            var visitors = _visitorService.GetVisitorsByStatus(status);
            return Ok(visitors);
        }

        public IActionResult UpdateVisitor(int id, [FromBody] Visitor visitor)
        {
            var existingVisitor = _visitorService.GetVisitorById(id);
            if (existingVisitor == null)
                return NotFound();

            existingVisitor.Name = visitor.Name;
            existingVisitor.Email = visitor.Email;
            // Update other properties as needed

            _visitorService.UpdateVisitor(existingVisitor);
            return Ok(existingVisitor);
        }

        public IActionResult DeleteVisitor(int id)
        {
            var existingVisitor = _visitorService.GetVisitorById(id);
            if (existingVisitor == null)
                return NotFound();

            _visitorService.DeleteVisitor(id);
            return NoContent();
        }
    }

    // Startup
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("VisitorDb"));

            services.AddScoped<IVisitorService, VisitorService>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    // Main Program
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}