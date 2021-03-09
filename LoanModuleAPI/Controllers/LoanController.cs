using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanModuleAPI.Data;
using LoanModuleAPI.Models;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace LoanModuleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Loan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoans()
        {
            return await _context.Loans.ToListAsync();
        }

        // GET: api/Loan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound();
            }

            return loan;
        }

        // PUT: api/Loan/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoan(int id, Loan loan)
        {
            if (id != loan.ID)
            {
                return BadRequest();
            }

            _context.Entry(loan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [HttpPost]
        public async Task<IActionResult> UploadSingle(IFormFile file, [FromForm] Loan loan)
        {

            DateTime foo = DateTime.Now;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

            var loanObj = new Loan()
            {
                Amount = loan.Amount,
                LoanID = unixTime,
                Tenor = loan.Tenor,
                UserID = "ME",
                CreatedDate = DateTime.Now
            };

            _context.Loans.Add(loanObj);
            var success = await _context.SaveChangesAsync();

            long size = file.Length;

            if (success > 0)
            {

                // Extract file name from whatever was posted by browser
                var fileName = System.IO.Path.GetFileName(file.FileName);

                // If file with same name exists delete it
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }


                //Assigning Unique Filename (Guid)
                var myUniqueFileName = Convert.ToString(Guid.NewGuid());


                //Getting file Extension
                var fileExtension = Path.GetExtension(fileName);

                // concatenating  FileName + FileExtension
                var newFileName = String.Concat(myUniqueFileName, fileExtension);

                var filepath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads")).Root + $@"\{newFileName}";


                using (var fileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
                {
                    file.CopyTo(fileStream);
                }

                var objfiles = new SupportDoc()
                {
                    LoanID = unixTime,
                    FileName = newFileName,
                    CreatedDate = DateTime.Now
                };

                _context.SupportDocs.Add(objfiles);
                await _context.SaveChangesAsync();
            }

            return Ok(new { size, success });
        }

        // POST: api/Loan Multiple Files
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        //public async Task<ActionResult<Loan>> PostLoan(Loan loan)
        public async Task<IActionResult> PostLoan([FromForm] List<IFormFile> files, [FromForm] Loan loan)
        {

            DateTime foo = DateTime.Now;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

            var loanObj = new Loan()
            {
                Amount = loan.Amount,
                LoanID = unixTime,
                Tenor = loan.Tenor,
                UserID = "ME",
                CreatedDate = DateTime.Now
            };

            _context.Loans.Add(loanObj);
            var success = await _context.SaveChangesAsync();

            long size = files.Sum(f => f.Length);

            if (success > 0)
            {
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            //Getting FileName
                            var fileName = Path.GetFileName(file.FileName);

                            //Assigning Unique Filename (Guid)
                            var myUniqueFileName = Convert.ToString(Guid.NewGuid());

                            //Getting file Extension
                            var fileExtension = Path.GetExtension(fileName);

                            // concatenating  FileName + FileExtension
                            var newFileName = String.Concat(myUniqueFileName, fileExtension);

                            // Combines two strings into a path.
                            var filepath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads")).Root + $@"\{newFileName}";

                            using (FileStream fileStream = System.IO.File.Create(filepath))
                            {
                                await file.CopyToAsync(fileStream);
                                fileStream.Flush();
                            }

                            var objfiles = new SupportDoc()
                            {
                                LoanID = unixTime,
                                FileName = newFileName,
                                CreatedDate = DateTime.Now
                            };

                            _context.SupportDocs.Add(objfiles);
                            await _context.SaveChangesAsync();

                        }
                    }
                }

            }

            return Ok(new { count = files.Count, size, success });


        }

        // DELETE: api/Loan/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Loan>> DeleteLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            return loan;
        }

        private bool LoanExists(int id)
        {
            return _context.Loans.Any(e => e.ID == id);
        }
    }
}
