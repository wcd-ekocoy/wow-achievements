using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WowAchievementsApp.Models;
using WowAchievementsApp.Services;

namespace WowAchievementsApp.Controllers;

public class HomeController : Controller
{
    private readonly BlizzardService _blizzardService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(BlizzardService blizzardService, ILogger<HomeController> logger)
    {
        _blizzardService = blizzardService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            try
            {
                // Get access token from authentication
                var accessToken = await HttpContext.GetTokenAsync("BattleNet", "access_token");
                _logger.LogInformation("Access token retrieved: {HasToken}", !string.IsNullOrEmpty(accessToken));
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    // If no token, redirect to login
                    _logger.LogWarning("No access token found, redirecting to login");
                    return RedirectToAction("Login", "Account");
                }

                // Get region from cookie
                var region = Request.Cookies["region"] ?? "us";
                _logger.LogInformation("Using region: {Region}", region);

                // Fetch user profile with characters
                _logger.LogInformation("Fetching profile summary...");
                var profile = await _blizzardService.GetProfileSummary(accessToken, region);
                
                _logger.LogInformation("Profile fetch completed. Profile is null: {IsNull}", profile == null);
                
                // Flatten all characters from all accounts
                var allCharacters = new List<Character>();
                if (profile?.WowAccounts != null)
                {
                    _logger.LogInformation("Found {Count} WoW accounts", profile.WowAccounts.Count);
                    foreach (var account in profile.WowAccounts)
                    {
                        if (account.Characters != null)
                        {
                            _logger.LogInformation("Account {AccountId} has {CharacterCount} characters", account.Id, account.Characters.Count);
                            allCharacters.AddRange(account.Characters);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Profile or WowAccounts is null");
                }

                ViewBag.Characters = allCharacters;
                ViewBag.Region = region;
                
                _logger.LogInformation("Total characters found: {Count}", allCharacters.Count);
                
                // Debug info
                if (allCharacters.Count == 0)
                {
                    ViewBag.DebugInfo = $"Profile: {(profile != null ? "Found" : "Null")}, " +
                                       $"WowAccounts: {(profile?.WowAccounts != null ? profile.WowAccounts.Count.ToString() : "Null")}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching characters: {Message}", ex.Message);
                ViewBag.ErrorMessage = $"Error loading characters: {ex.Message}";
                ViewBag.DebugInfo = $"Exception: {ex.GetType().Name} - {ex.Message}";
            }
        }

        return View();
    }

    [Authorize]
    public async Task<IActionResult> Achievements(string realmSlug, string characterName, int page = 1)
    {
        if (string.IsNullOrEmpty(realmSlug) || string.IsNullOrEmpty(characterName))
        {
            return RedirectToAction("Index");
        }

        if (page < 1) page = 1;

        try
        {
            // Get access token from authentication
            var accessToken = await HttpContext.GetTokenAsync("BattleNet", "access_token");
            _logger.LogInformation("Access token retrieved for achievements: {HasToken}", !string.IsNullOrEmpty(accessToken));
            
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("No access token found for achievements, redirecting to login");
                return RedirectToAction("Login", "Account");
            }

            // Get region from cookie
            var region = Request.Cookies["region"] ?? "us";
            _logger.LogInformation("Fetching achievements for {CharacterName} on {RealmSlug} in region {Region}", characterName, realmSlug, region);

            // Fetch character achievements
            var achievements = await _blizzardService.GetCharacterAchievements(accessToken, region, realmSlug, characterName);

            _logger.LogInformation("Achievements fetch completed. Achievements is null: {IsNull}", achievements == null);
            if (achievements != null)
            {
                _logger.LogInformation("Total achievements: {Count}, Total points: {Points}", achievements.Achievements?.Count ?? 0, achievements.TotalPoints);
            }

            if (achievements == null)
            {
                ViewBag.ErrorMessage = "Failed to load achievements. The API returned null. Check server logs for details.";
                ViewBag.CharacterName = characterName;
                ViewBag.RealmSlug = realmSlug;
                ViewBag.Region = region;
                return View(null);
            }

            // Pagination Logic
            int pageSize = 100;
            var allAchievements = achievements.Achievements ?? new List<Achievement>();
            
            // Apply sorting (Moved from View to Controller for consistent pagination)
            var sortedAchievements = allAchievements
                .OrderByDescending(a => a.CompletedTimestamp.HasValue)
                .ThenByDescending(a => a.CompletedTimestamp)
                .ToList();

            var totalItems = sortedAchievements.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginatedList = sortedAchievements
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new AchievementsViewModel
            {
                CharacterAchievements = achievements,
                PaginatedAchievements = paginatedList,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                RealmSlug = realmSlug,
                CharacterName = characterName,
                Region = region
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching achievements: {Message}", ex.Message);
            var errorMsg = ex.Message;
            if (ex.InnerException != null)
            {
                errorMsg += $" ({ex.InnerException.Message})";
            }
            ViewBag.ErrorMessage = $"Error loading achievements: {errorMsg}";
            ViewBag.CharacterName = characterName;
            ViewBag.RealmSlug = realmSlug;
            ViewBag.Region = Request.Cookies["region"] ?? "us";
            return View(null);
        }
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
}
