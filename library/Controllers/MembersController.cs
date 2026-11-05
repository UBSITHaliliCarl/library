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
    public MembersController(MemberRepository members)
    {
        _members = members;
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
        CheckMember(member);
        if (ModelState.IsValid)
        {
            try
            {
                await _members.AddAsync(member);
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                ModelState.AddModelError(nameof(Member.Email), "That email address is already used by another member.");
            }
        }
        return View(member);
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
    // POST /Members/Edit : the form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Member member)
    {
        CheckMember(member);
        if (ModelState.IsValid)
        {
            try
            {
                await _members.UpdateAsync(member);
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                ModelState.AddModelError(nameof(Member.Email), "That email address is already used by another member.");
            }
        }
        return View(member);
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
            // The member still has loans, and the database refuses to delete them.
            TempData["Error"] = "That member still has loans, so they cannot be deleted.";
        }
        return RedirectToAction(nameof(Index));
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