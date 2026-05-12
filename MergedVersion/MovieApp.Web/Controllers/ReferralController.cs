using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Web.Models;

namespace MovieApp.Web.Controllers;

public class ReferralController : Controller
{
    private readonly IAmbassadorRepository ambassadorRepository;
    private readonly IReferralCodeGenerator referralCodeGenerator;
    private readonly IReferralLogService referralLogService;

    private const int CurrentUserId = 4;
    private const string CurrentUsername = "Admin";
    private const int TestEventId = 1;

    public ReferralController(
        IAmbassadorRepository ambassadorRepository,
        IReferralCodeGenerator referralCodeGenerator,
        IReferralLogService referralLogService)
    {
        this.ambassadorRepository = ambassadorRepository;
        this.referralCodeGenerator = referralCodeGenerator;
        this.referralLogService = referralLogService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await this.BuildModelAsync();
        return this.View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateCode()
    {
        var existingCode = await this.ambassadorRepository.GetReferralCodeAsync(CurrentUserId);

        if (string.IsNullOrWhiteSpace(existingCode))
        {
            var code = this.referralCodeGenerator.Generate(CurrentUsername, CurrentUserId);
            await this.ambassadorRepository.CreateAmbassadorProfileAsync(CurrentUserId, code);
            TempData["Success"] = "Referral code generated successfully.";
        }
        else
        {
            TempData["Error"] = "You already have a referral code.";
        }

        return this.RedirectToAction(nameof(this.Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyCode(string codeToApply)
    {
        if (string.IsNullOrWhiteSpace(codeToApply))
        {
            TempData["Error"] = "Please enter a referral code.";
            return this.RedirectToAction(nameof(this.Index));
        }

        var ambassadorId =
            await this.ambassadorRepository.GetUserIdByReferralCodeAsync(codeToApply.Trim());

        if (ambassadorId is null)
        {
            TempData["Error"] = "Invalid referral code.";
            return this.RedirectToAction(nameof(this.Index));
        }

        if (ambassadorId.Value == CurrentUserId)
        {
            TempData["Error"] = "You cannot use your own referral code.";
            return this.RedirectToAction(nameof(this.Index));
        }

        await this.referralLogService.LogReferralUsageAsync(
            codeToApply.Trim(),
            CurrentUserId,
            TestEventId);

        TempData["Success"] = "Referral code applied successfully.";
        return this.RedirectToAction(nameof(this.Index));
    }

    private async Task<ReferralIndexViewModel> BuildModelAsync()
    {
        var code = await this.ambassadorRepository.GetReferralCodeAsync(CurrentUserId);
        var balance = await this.ambassadorRepository.GetRewardBalanceAsync(CurrentUserId);
        var history = await this.ambassadorRepository.GetReferralHistoryAsync(CurrentUserId);

        return new ReferralIndexViewModel
        {
            ReferralCode = code,
            RewardBalance = balance,
            History = history,
        };
    }
}