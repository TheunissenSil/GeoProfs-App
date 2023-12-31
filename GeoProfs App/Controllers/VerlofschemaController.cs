﻿using Microsoft.AspNetCore.Mvc;
using GeoProfs_App.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using GeoProfs_App.Data;
using Microsoft.AspNetCore.Authorization;

namespace GeoProfs_App.Controllers
{

    // Moet ingelogd zijn
    [Authorize]
    public class VerlofschemaController : Controller
    {
        private readonly GeoProfs_AppContext _dbContext;

        public VerlofschemaController(GeoProfs_AppContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Return the view
        public IActionResult Index(int? month, int? year)
        {
            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;

            DateTime startDate = new DateTime(selectedYear, selectedMonth, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            var users = _dbContext.Users.ToList();
            var verlofaanvragen = GetVerlofaanvragen(users, startDate, endDate);
            var verlofaanvragen2 = GetVerlofaanvragen(users, startDate, endDate);

            var viewModel = new VerlofaanvraagViewModel
            {
                Verlofaanvragen = verlofaanvragen,
                Verlofaanvraag = new Verlofaanvraag
                {
                    StartDate = startDate,
                    EndDate = endDate
                },
                Users = users
            };

            return View(viewModel);
        }

        // Lijst van verlofaanvragen
        private List<Verlofaanvraag> GetVerlofaanvragen(List<ApplicationUser> users, DateTime startDate, DateTime endDate)
        {
            var verlofaanvragen = _dbContext.Verlofaanvragen
                .Where(v => v.StartDate <= endDate && v.EndDate >= startDate)
                .Include(v => v.User)
                .ToList();

            var result = new List<Verlofaanvraag>();

            foreach (var user in users)
            {
                var userVerlofaanvragen = verlofaanvragen
                    .Where(v => v.UserId == user.Id && v.Status != "pending")
                    .ToList();

                if (userVerlofaanvragen.Count > 0)
                {
                    foreach (var verlofaanvraag in userVerlofaanvragen)
                    {
                        // Check if the leave request spans multiple months
                        DateTime leaveStart = verlofaanvraag.StartDate;
                        DateTime leaveEnd = verlofaanvraag.EndDate;

                        while (leaveStart <= leaveEnd)
                        {
                            if (leaveStart.Month == startDate.Month && leaveStart.Year == startDate.Year)
                            {
                                result.Add(verlofaanvraag);
                            }

                            leaveStart = leaveStart.AddMonths(1);
                        }
                    }
                }
                else
                {
                    // If no leave request for this user, create an empty one for the current month
                    var emptyVerlofaanvraag = new Verlofaanvraag
                    {
                        User = user,
                        StartDate = startDate,
                        EndDate = endDate
                    };

                    result.Add(emptyVerlofaanvraag);
                }
            }

            return result;
        }

    }
}