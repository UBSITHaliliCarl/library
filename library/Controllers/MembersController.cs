using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using library.Data;
using library.Models;
namespace library.Controllers;

public class MembersController : Controller
{
    private static readonly string[] MemberTypes = { "Regular", "Student", "Faculty" };
    private readonly MemberRepository _members;
    private readonly LoanRepository _loans;

    // ASP.NET Core hands in the repositories, because Program.cs registered them.
    public MembersController(MemberRepository members, LoanRepository loans)
    {
        _members = members;
        _loans = loans;
    }

    // GET /Members : the list.
    public async Task<IActionResult> Index()
    {
        var member = await _members.GetAllAsync();
        return View(member);
    }
    // GET /Members/Create : show the form with sensible starting values.
    public IActionResult Create()
    {
        return View(new Member
        {
            MemberType = "Regular",
            JoinDate = DateTime.Today.ToString("yyyy-MM-dd")
        });
    }
    // POST /Members/Create : the form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member member)
    {
        if (!ModelState.IsValid)
        {
            return View(member);
        }

        try
        {
            await _members.AddAsync(member);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError("Email", "Another member already uses this email.");
            return View(member);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET /Members/Edit/5 : show the form filled in.
    public async Task<IActionResult> Edit(long id)
    {
        var member = await _members.GetByIdAsync(id);
        if (member == null)
        {
            return NotFound();
        }
        return View(member);
    }
    // POST /Members/Edit/5 : the changed form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Member member)
    {
        if (!ModelState.IsValid)
        {
            return View(member);
        }

        try
        {
            await _members.UpdateAsync(member);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError("Email", "Another member already uses this email.");
            return View(member);
        }

        return RedirectToAction(nameof(Index));
    }
    // GET /Members/Delete/5 : ask "are you sure?"
    public async Task<IActionResult> Delete(long id)
    { 
        var member = await _members.GetByIdAsync(id);
        if (member == null)
        {
            return NotFound();
        }
        return View(member);
    }
    // POST /Members/Delete/5 : the delete was confirmed.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            await _members.DeleteAsync(id);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            var member = await _members.GetByIdAsync(id);

            if (member == null)
            {
                return NotFound();
            }
            ViewBag.Error = "This member has loans on record, so the database will not delete them.";
            return View("Delete", member);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET /Members/Details/5 : one member and their loans.
    public async Task<IActionResult> Details(long id)
    {
        var member = await _members.GetByIdAsync(id);
        if (member == null)
        {
            return NotFound();
        }

        var details = new MemberDetails
        {
            Member = member,
            Loans = await _loans.GetByMemberAsync(id)
        };
        return View(details);
    }

    // Checks that go beyond the model's own rules.
    private void CheckMember(Member member)
    {
        // Email and Phone are optional in the database, so drop the automatic
        // "required" errors that non-nullable strings get, then check the email's shape.
        ModelState.Remove(nameof(Member.Email));
        ModelState.Remove(nameof(Member.Phone));
        if (!string.IsNullOrWhiteSpace(member.Email) && !new EmailAddressAttribute().IsValid(member.Email))
        {
            ModelState.AddModelError(nameof(Member.Email), "Enter a valid email address.");
        }
        if (!MemberTypes.Contains(member.MemberType))
        {
            ModelState.AddModelError(nameof(Member.MemberType), "Choose Regular, Student or Faculty.");
        }
        if (!DateOnly.TryParseExact(member.JoinDate, "yyyy-MM-dd", out _))
        {
            ModelState.AddModelError(nameof(Member.JoinDate), "Enter a valid join date.");
        }
    }
} 